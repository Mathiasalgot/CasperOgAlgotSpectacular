using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class Scr_WindowFocusHelper : MonoBehaviour
{
    private const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")] 
    private static extern bool AllowSetForegroundWindow(int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    public static void FocusUnityWindow()
    {
#if UNITY_EDITOR
        return;
#endif
        string windowTitle = Application.productName;
        IntPtr hWnd = FindWindow(null, windowTitle);

        if (hWnd == IntPtr.Zero)
        {
            UnityEngine.Debug.LogWarning("Could not find Unity window: " + windowTitle);
            return;
        }

        // Restore if minimized
        ShowWindow(hWnd, SW_RESTORE);

        // Get thread IDs
        IntPtr foreground = GetForegroundWindow();
        uint foreThread = GetWindowThreadProcessId(foreground, out _);
        uint appThread = GetCurrentThreadId();

        // Attach threads to allow SetForegroundWindow
        if (foreThread != appThread)
        {
            AttachThreadInput(foreThread, appThread, true);
            SetForegroundWindow(hWnd);
            AttachThreadInput(foreThread, appThread, false);
        }
        else
        {
            // Tell Windows to allow this process to set foreground
            AllowSetForegroundWindow(-1);

            SetForegroundWindow(hWnd);
        }
    }
}
