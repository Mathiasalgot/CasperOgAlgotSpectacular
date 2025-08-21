using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Scr_WindowDragger : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HTCAPTION = 0x2;

    private const int HTBOTTOMRIGHT = 17;

    public bool dragging;
    public bool scaling;

    void Update()
    {
        // Example: left mouse down anywhere on screen starts window drag
#if !UNITY_EDITOR
        if (dragging)
        {

            IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            ReleaseCapture();
            SendMessage(hWnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);

        }

        if (scaling)
        {
            IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            StartResize(hWnd, HTBOTTOMRIGHT);
        }

#endif
    }

    //Needs rework, Move to window transparency and change the window size there
    void StartResize(IntPtr hWnd, int hitTest)
    {
#if !UNITY_EDITOR
        ReleaseCapture();
        SendMessage(hWnd, WM_NCLBUTTONDOWN, hitTest, 0);
#endif
    }

    public void StartDrag()
    {
        // Start dragging when the mouse button is pressed down
        dragging = true;
    }

    public void StartScale()
    {
        scaling = true;
    }

    public void MouseUp()
    {
        // Stop dragging when the mouse button is released
        dragging = false;
        scaling = false;
    }
}
