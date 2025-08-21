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

    public bool dragging;

    void Update()
    {
        // Example: left mouse down anywhere on screen starts window drag
        if (dragging)
        {
#if !UNITY_EDITOR
            IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            ReleaseCapture();
            SendMessage(hWnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
#endif
        }
    }

    public void MouseDown()
    {
        // Start dragging when the mouse button is pressed down
        dragging = true;
    }

    public void MouseUp()
    {
        // Stop dragging when the mouse button is released
        dragging = false;
    }
}
