#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class GlobalMouseHook : MonoBehaviour
{
    // ------- Public events you can subscribe to -------
    // Position is in desktop/screen coordinates (pixels, across monitors).
    public static event Action<Vector2> OnLeftDown;
    public static event Action<Vector2> OnLeftUp;
    public static event Action<Vector2> OnRightDown;
    public static event Action<Vector2> OnRightUp;
    public static event Action<Vector2> OnMiddleDown;
    public static event Action<Vector2> OnMiddleUp;
    public static event Action<int, Vector2> OnWheel; // delta (120 per notch), position
    public static event Action<int, Vector2> OnXButtonDown; // 1 or 2, position
    public static event Action<int, Vector2> OnXButtonUp;   // 1 or 2, position

    // ------- Unity events you can hook up in inspector -------
    public UnityEvent<Vector2> leftClickEvent;

    // ------- Hook internals -------
    private static IntPtr _hookHandle = IntPtr.Zero;
    private static HookProc _hookProcDelegate; // keep delegate alive
    private static readonly ConcurrentQueue<Action> _dispatch = new ConcurrentQueue<Action>();

    // Win32 constants
    private const int WH_MOUSE_LL = 14;

    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MBUTTONUP = 0x0208;
    private const int WM_MOUSEWHEEL = 0x020A;
    private const int WM_XBUTTONDOWN = 0x020B;
    private const int WM_XBUTTONUP = 0x020C;

    private const int WHEEL_DELTA = 120;
    private const int XBUTTON1 = 1;
    private const int XBUTTON2 = 2;

    // Win32 types
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;   // HIWORD: wheel delta or XButton id
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    // Delegates & P/Invoke
    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private static short HIWORD(uint val) => (short)((val >> 16) & 0xFFFF);

    // ------- Lifecycle -------
    private void OnEnable()
    {
        InstallHook();
        SetupEvents();
    }

    private void OnDisable()
    {
        RemoveHook();
    }

    private void OnApplicationQuit()
    {
        RemoveHook();
    }

    private void Update()
    {
        // Marshal hook-thread events to Unity main thread.
        while (_dispatch.TryDequeue(out var action))
        {
            try { action?.Invoke(); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
        }
    }

    // ------- Hook install/remove -------
    private static void InstallHook()
    {
        if (_hookHandle != IntPtr.Zero) return;

        _hookProcDelegate = HookCallback; // keep alive
        IntPtr hModule;
        try
        {
            using (var cur = Process.GetCurrentProcess())
            using (var mod = cur.MainModule)
            {
                hModule = GetModuleHandle(mod.ModuleName);
            }
        }
        catch
        {
            // Fallback: sometimes GetModuleHandle(null) also works, but module name is preferred.
            hModule = GetModuleHandle(null);
        }

        _hookHandle = SetWindowsHookEx(WH_MOUSE_LL, _hookProcDelegate, hModule, 0);
        if (_hookHandle == IntPtr.Zero)
        {
            UnityEngine.Debug.LogError($"[GlobalMouseHook] SetWindowsHookEx failed (err {Marshal.GetLastWin32Error()}).");
        }
        else
        {
            UnityEngine.Debug.Log("[GlobalMouseHook] Low-level mouse hook installed.");
        }
    }

    private static void RemoveHook()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            if (!UnhookWindowsHookEx(_hookHandle))
            {
                UnityEngine.Debug.LogError($"[GlobalMouseHook] UnhookWindowsHookEx failed (err {Marshal.GetLastWin32Error()}).");
            }
            else
            {
                UnityEngine.Debug.Log("[GlobalMouseHook] Low-level mouse hook removed.");
            }
            _hookHandle = IntPtr.Zero;
            _hookProcDelegate = null;
            // drain queue to avoid stale actions sneaking in after teardown
            while (_dispatch.TryDequeue(out _)) { }
        }
    }

    // ------- UnityEvent setup -------
    private void SetupEvents()
    {
        OnLeftDown += leftClickEvent.Invoke;
    }

    // ------- The hook callback (runs on OS hook thread; DON'T touch Unity here) -------
    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // nCode < 0 => pass to next hook without processing
        if (nCode >= 0)
        {
            try
            {
                var info = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                var pos = new Vector2(info.pt.x, info.pt.y);
                int msg = wParam.ToInt32();

                switch (msg)
                {
                    case WM_LBUTTONDOWN:
                        _dispatch.Enqueue(() => OnLeftDown?.Invoke(pos));
                        break;
                    case WM_LBUTTONUP:
                        _dispatch.Enqueue(() => OnLeftUp?.Invoke(pos));
                        break;
                    case WM_RBUTTONDOWN:
                        _dispatch.Enqueue(() => OnRightDown?.Invoke(pos));
                        break;
                    case WM_RBUTTONUP:
                        _dispatch.Enqueue(() => OnRightUp?.Invoke(pos));
                        break;
                    case WM_MBUTTONDOWN:
                        _dispatch.Enqueue(() => OnMiddleDown?.Invoke(pos));
                        break;
                    case WM_MBUTTONUP:
                        _dispatch.Enqueue(() => OnMiddleUp?.Invoke(pos));
                        break;
                    case WM_MOUSEWHEEL:
                        {
                            // wheel delta is HIWORD(mouseData), signed (multiples of 120)
                            int delta = HIWORD(info.mouseData);
                            _dispatch.Enqueue(() => OnWheel?.Invoke(delta, pos));
                            break;
                        }
                    case WM_XBUTTONDOWN:
                        {
                            int which = HIWORD(info.mouseData); // 1 or 2
                            if (which == XBUTTON1 || which == XBUTTON2)
                                _dispatch.Enqueue(() => OnXButtonDown?.Invoke(which, pos));
                            break;
                        }
                    case WM_XBUTTONUP:
                        {
                            int which = HIWORD(info.mouseData);
                            if (which == XBUTTON1 || which == XBUTTON2)
                                _dispatch.Enqueue(() => OnXButtonUp?.Invoke(which, pos));
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                // Never throw on hook thread
                UnityEngine.Debug.LogException(ex);
            }
        }

        // Always pass to next hook; returning non-zero would swallow input system-wide.
        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }
}
#endif
