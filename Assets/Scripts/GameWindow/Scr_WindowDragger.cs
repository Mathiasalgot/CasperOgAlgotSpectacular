using System;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class Scr_WindowDragger : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int HTCAPTION = 0x2;
    private const int HWND_TOPMOST = -1;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int left, top, right, bottom; }

    public bool dragging;
    public bool scaling;

    private Vector2 scaleStartMouse;
    private RECT startRect;
    private IntPtr hWnd;
    private float aspectRatio;

  
    [DllImport("user32.dll")]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr hCursor);

    // Cursor IDs
    private const int IDC_ARROW = 32512;
    private const int IDC_SIZEWE = 32644;
    private const int IDC_SIZENS = 32645;
    private const int IDC_SIZENWSE = 32642;
    private const int IDC_SIZENESW = 32643;
    private const int IDC_SIZEALL = 32646;

    private IntPtr hCursor;

    void Start()
    {
        hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

        hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
    }


    public void SetArrowCursor(int cursorInt)
    {
        
        switch (cursorInt)
        {
            case 0:
                hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
                break;
            case 1:
                hCursor = LoadCursor(IntPtr.Zero, IDC_SIZENWSE);
                break;
            case 2:
                hCursor = LoadCursor(IntPtr.Zero, IDC_SIZEALL);
                break;
            default:
                break;
        }
        
        SetCursor(hCursor);
    }



    void Update()
    {

        SetCursor(hCursor);

#if !UNITY_EDITOR
        Vector2 mousePos = Input.mousePosition;

        // --- SCALING ONLY ---
        if (scaling)
        {
            Vector2 delta = mousePos - scaleStartMouse;

            int newWidth  = Math.Max(100, (startRect.right - startRect.left) + (int)delta.x);
            int newHeight = (int)(newWidth / aspectRatio);

            SetWindowPos(hWnd, HWND_TOPMOST,
                startRect.left,
                startRect.top,
                newWidth,
                newHeight,
                SWP_NOZORDER | SWP_NOACTIVATE);
        }
#endif
    }

    public void StartDrag()
    {
#if !UNITY_EDITOR
        // Let OS handle drag entirely for snapping behavior
        ReleaseCapture();
        SendMessage(hWnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
#endif
    }

    public void StartScale()
    {
#if !UNITY_EDITOR
        scaling = true;
        scaleStartMouse = Input.mousePosition;
        GetWindowRect(hWnd, out startRect);

        int width  = startRect.right - startRect.left;
        int height = startRect.bottom - startRect.top;
        aspectRatio = (float)width / height;
#endif
    }

    public void MouseUp()
    {
        dragging = false;
        scaling = false;
    }
}