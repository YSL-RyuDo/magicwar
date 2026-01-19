using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class WindowBorderRemover : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags
    );

    const int GWL_STYLE = -16;

    const int WS_POPUP = unchecked((int)0x80000000); // 완전 무테 창
    const uint SWP_FRAMECHANGED = 0x0020;
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOSIZE = 0x0001;

    void Start()
    {
        IntPtr hwnd = GetActiveWindow();

        // 타이틀바 + 닫기 버튼 + 테두리 제거
        SetWindowLong(hwnd, GWL_STYLE, WS_POPUP);

        // 변경사항 강제 적용
        SetWindowPos(
            hwnd,
            IntPtr.Zero,
            0, 0, 0, 0,
            SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE
        );
    }
}
