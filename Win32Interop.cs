using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WhatHaveIDone;

#if WINDOWS

public static class Win32Interop {

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    public static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    public static extern bool SetClipboardData(uint uFormat, IntPtr data);

    public static void CopyToClipboard(string text)
    {
        OpenClipboard(IntPtr.Zero);
        var ptr = Marshal.StringToHGlobalUni(text);
        SetClipboardData(13, ptr);
        CloseClipboard();
        Marshal.FreeHGlobal(ptr);
    }
}

#endif

