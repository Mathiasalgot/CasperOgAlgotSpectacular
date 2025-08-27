#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class GlobalInputHook : MonoBehaviour
{
    // =========================
    // Public C# events
    // =========================
    public static event Action<bool, int> OnAnyPress;   // (isKeyboard, code)  VK for keyboard, negative codes for mouse (see constants)
    public static event Action OnRightMouseDown;
    public static event Action OnCtrlShiftEnter;

    // =========================
    // Optional UnityEvents (Inspector wiring)
    // =========================
    [Header("UnityEvents (optional)")]
    public UnityEvent<bool, int> anyPressEvent;   // (isKeyboard, code)
    public UnityEvent rightMouseDownEvent;
    public UnityEvent ctrlShiftEnterEvent;

    [Header("Hotkey behavior")]
    [Tooltip("When true, Ctrl+Shift+Enter will bring the Unity window to the foreground before invoking events.")]
    public bool bringWindowToFrontOnHotkey = true;

    // =========================
    // Internals
    // =========================
    private static IntPtr _mouseHook = IntPtr.Zero;
    private static IntPtr _keyboardHook = IntPtr.Zero;

    private static HookProc _mouseProc;
    private static HookProc _keyboardProc;

    private static readonly ConcurrentQueue<Action> _dispatch = new ConcurrentQueue<Action>();

    private IntPtr _hwnd; // stored at runtime for focusing the window

    // -------------------------
    // Win32 constants
    // -------------------------
    private const int WH_MOUSE_LL = 14;
    private const int WH_KEYBOARD_LL = 13;

    // Keyboard messages
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    // Mouse messages
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_XBUTTONDOWN = 0x020B;

    // X buttons
    private const int XBUTTON1 = 1;
    private const int XBUTTON2 = 2;

    // Mouse codes for OnAnyPress (negative to distinguish from VKs)
    public const int MOUSE_LEFT = -1;
    public const int MOUSE_RIGHT = -2;
    public const int MOUSE_MIDDLE = -3;
    public const int MOUSE_X1 = -4;
    public const int MOUSE_X2 = -5;

    // VKs we care about for the combo
    private const int VK_RETURN = 0x0D;
    private const int VK_LCONTROL = 0xA2;
    private const int VK_RCONTROL = 0xA3;
    private const int VK_LSHIFT = 0xA0;
    private const int VK_RSHIFT = 0xA1;

    // -------------------------
    // Win32 structs
    // -------------------------
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int x; public int y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;   // HIWORD: X buttons (1/2)
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    // -------------------------
    // P/Invoke
    // -------------------------
    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();

    private static short HIWORD(uint val) => (short)((val >> 16) & 0xFFFF);

    // Modifier key state for combo
    private static bool _ctrl, _shift;

    // =========================
    // Unity lifecycle
    // =========================
    private void OnEnable()
    {
        _hwnd = Process.GetCurrentProcess().MainWindowHandle;
        InstallKeyboardHook();
        InstallMouseHook();

        // Wire UnityEvents to C# events (optional)
        OnAnyPress += (isKb, code) => anyPressEvent?.Invoke(isKb, code);
        OnRightMouseDown += () => rightMouseDownEvent?.Invoke();
        OnCtrlShiftEnter += () => ctrlShiftEnterEvent?.Invoke();
    }

    private void OnDisable()
    {
        // Unwire UnityEvents
        OnAnyPress = null;
        OnRightMouseDown = null;
        OnCtrlShiftEnter = null;

        RemoveMouseHook();
        RemoveKeyboardHook();

        // drain queue
        while (_dispatch.TryDequeue(out _)) { }
    }

    private void Update()
    {
        while (_dispatch.TryDequeue(out var a))
        {
            try { a?.Invoke(); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
        }
    }

    // =========================
    // Hook install/remove
    // =========================
    private static void InstallKeyboardHook()
    {
        if (_keyboardHook != IntPtr.Zero) return;

        _keyboardProc = KeyboardCallback;
        IntPtr hMod;
        try
        {
            using (var p = Process.GetCurrentProcess())
            using (var m = p.MainModule)
                hMod = GetModuleHandle(m.ModuleName);
        }
        catch { hMod = GetModuleHandle(null); }

        _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, hMod, 0);
        if (_keyboardHook == IntPtr.Zero)
            UnityEngine.Debug.LogError($"[GlobalInputHook] Keyboard hook failed (err {Marshal.GetLastWin32Error()}).");
        else
            UnityEngine.Debug.Log("[GlobalInputHook] Keyboard hook installed.");
    }

    private static void InstallMouseHook()
    {
        if (_mouseHook != IntPtr.Zero) return;

        _mouseProc = MouseCallback;
        IntPtr hMod;
        try
        {
            using (var p = Process.GetCurrentProcess())
            using (var m = p.MainModule)
                hMod = GetModuleHandle(m.ModuleName);
        }
        catch { hMod = GetModuleHandle(null); }

        _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, hMod, 0);
        if (_mouseHook == IntPtr.Zero)
            UnityEngine.Debug.LogError($"[GlobalInputHook] Mouse hook failed (err {Marshal.GetLastWin32Error()}).");
        else
            UnityEngine.Debug.Log("[GlobalInputHook] Mouse hook installed.");
    }

    private static void RemoveKeyboardHook()
    {
        if (_keyboardHook != IntPtr.Zero)
        {
            if (!UnhookWindowsHookEx(_keyboardHook))
                UnityEngine.Debug.LogError($"[GlobalInputHook] Unhook keyboard failed (err {Marshal.GetLastWin32Error()}).");
            _keyboardHook = IntPtr.Zero;
            _keyboardProc = null;
        }
    }

    private static void RemoveMouseHook()
    {
        if (_mouseHook != IntPtr.Zero)
        {
            if (!UnhookWindowsHookEx(_mouseHook))
                UnityEngine.Debug.LogError($"[GlobalInputHook] Unhook mouse failed (err {Marshal.GetLastWin32Error()}).");
            _mouseHook = IntPtr.Zero;
            _mouseProc = null;
        }
    }

    // =========================
    // Callbacks (run on OS hook thread)
    // =========================
    private static IntPtr KeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            int vk = unchecked((int)info.vkCode);
            int msg = wParam.ToInt32();

            switch (msg)
            {
                case WM_KEYDOWN:
                case WM_SYSKEYDOWN:
                    if (vk == VK_LCONTROL || vk == VK_RCONTROL) _ctrl = true;
                    if (vk == VK_LSHIFT || vk == VK_RSHIFT) _shift = true;

                    _dispatch.Enqueue(() => OnAnyPress?.Invoke(true, vk));

                    if (_ctrl && _shift && vk == VK_RETURN)
                    {
                        _dispatch.Enqueue(() =>
                        {
                            
                            OnCtrlShiftEnter?.Invoke();
                            
                        });
                    }
                    break;

                case WM_KEYUP:
                case WM_SYSKEYUP:
                    if (vk == VK_LCONTROL || vk == VK_RCONTROL) _ctrl = false;
                    if (vk == VK_LSHIFT || vk == VK_RSHIFT) _shift = false;
                    break;
            }
        }
        return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }

    private static IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var info = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            int msg = wParam.ToInt32();

            switch (msg)
            {
                case WM_LBUTTONDOWN:
                    _dispatch.Enqueue(() => OnAnyPress?.Invoke(false, MOUSE_LEFT));
                    break;

                case WM_RBUTTONDOWN:
                    _dispatch.Enqueue(() => OnAnyPress?.Invoke(false, MOUSE_RIGHT));
                    _dispatch.Enqueue(() => OnRightMouseDown?.Invoke());
                    break;

                case WM_MBUTTONDOWN:
                    _dispatch.Enqueue(() => OnAnyPress?.Invoke(false, MOUSE_MIDDLE));
                    break;

                case WM_XBUTTONDOWN:
                    {
                        int which = HIWORD(info.mouseData);
                        int code = (which == XBUTTON1) ? MOUSE_X1 : (which == XBUTTON2 ? MOUSE_X2 : 0);
                        if (code != 0)
                            _dispatch.Enqueue(() => OnAnyPress?.Invoke(false, code));
                        break;
                    }
            }
        }
        return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

}
#endif
