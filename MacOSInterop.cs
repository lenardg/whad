using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WhatHaveIDone;

#if MACOS

public static class MacOSInterop {
    // Import CoreGraphics framework
    private const string CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    // Import AppKit framework
    private const string AppKit = "/System/Library/Frameworks/AppKit.framework/AppKit";

    [DllImport(CoreGraphics)]
    private static extern IntPtr CGWindowListCopyWindowInfo(CGWindowListOption option, CGWindowID relativeToWindow);

    [DllImport(CoreGraphics)]
    private static extern IntPtr CGMainDisplayID();

    [DllImport(AppKit)]
    private static extern IntPtr NSPasteboard_generalPasteboard();

    [DllImport(AppKit)]
    private static extern void NSPasteboard_clearContents(IntPtr pasteboard);

    [DllImport(AppKit)]
    private static extern bool NSPasteboard_setString(IntPtr pasteboard, IntPtr str, IntPtr type);

    // Import required functions
    [DllImport(CoreGraphics)]
    private static extern void CFRelease(IntPtr cf);

    [DllImport(CoreGraphics)]
    private static extern IntPtr CFArrayGetValueAtIndex(IntPtr theArray, long idx);

    [DllImport(CoreGraphics)]
    private static extern long CFArrayGetCount(IntPtr theArray);

    [DllImport(CoreGraphics)]
    private static extern IntPtr CFDictionaryGetValue(IntPtr theDict, IntPtr key);

    [DllImport(CoreGraphics)]
    private static extern bool CFStringGetCString(IntPtr theString, StringBuilder buffer, long bufferSize, int encoding);

    private enum CGWindowListOption {
        OnScreenOnly = 1,
        OptionOnScreenAboveWindow = 2,
        OptionOnScreenBelowWindow = 4,
        OptionIncludingWindow = 8,
        OptionExcludeDesktopElements = 16
    }

    private enum CGWindowID : ulong {
        CGNullWindowID = 0
    }

    public static (string? title, uint processId) GetForegroundWindowInfo()
    {
        var windowListInfo = CGWindowListCopyWindowInfo(
            CGWindowListOption.OnScreenOnly | CGWindowListOption.OptionOnScreenAboveWindow,
            CGWindowID.CGNullWindowID);

        if (windowListInfo == IntPtr.Zero)
            return (null, 0);

        // Get the frontmost window (first in array)
        var windowCount = CFArrayGetCount(windowListInfo);
        if (windowCount == 0)
        {
            CFRelease(windowListInfo);
            return (null, 0);
        }

        var windowDict = CFArrayGetValueAtIndex(windowListInfo, 0);

        // Get window title
        var kCFWindowOwnerName = Marshal.StringToHGlobalUni("kCGWindowOwnerName");
        var nameRef = CFDictionaryGetValue(windowDict, kCFWindowOwnerName);
        Marshal.FreeHGlobal(kCFWindowOwnerName);

        var buffer = new StringBuilder(256);
        CFStringGetCString(nameRef, buffer, 256, 0x08000100); // kCFStringEncodingUTF8
        var title = buffer.ToString();

        // Get process ID
        var kCFWindowOwnerPID = Marshal.StringToHGlobalUni("kCGWindowOwnerPID");
        var pidRef = CFDictionaryGetValue(windowDict, kCFWindowOwnerPID);
        Marshal.FreeHGlobal(kCFWindowOwnerPID);
        var pid = (uint)Marshal.ReadInt32(pidRef);

        CFRelease(windowListInfo);

        return (title, pid);
    }

    public static string? GetProcessName(uint pid)
    {
        var buffer = new StringBuilder(256);
        var proc_pidpath = dlopen("libproc.dylib", 0);
        if (proc_pidpath != IntPtr.Zero)
        {
            var proc_name = GetDelegateForFunctionPointer<ProcName>(dlsym(proc_pidpath, "proc_name"));
            if (proc_name != null)
            {
                proc_name(pid, buffer, 256);
                dlclose(proc_pidpath);
                return buffer.ToString();
            }
            dlclose(proc_pidpath);
        }
        return null;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ProcName(uint pid, StringBuilder buffer, uint bufferSize);

    [DllImport("libdl.dylib")]
    private static extern IntPtr dlopen(string path, int mode);

    [DllImport("libdl.dylib")]
    private static extern IntPtr dlsym(IntPtr handle, string symbol);

    [DllImport("libdl.dylib")]
    private static extern int dlclose(IntPtr handle);

    private static T GetDelegateForFunctionPointer<T>(IntPtr ptr) where T : Delegate
    {
        return ptr != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<T>(ptr) : null;
    }

    public static void CopyToClipboard(string text)
    {
        var pasteboard = NSPasteboard_generalPasteboard();
        if (pasteboard == IntPtr.Zero)
            return;

        NSPasteboard_clearContents(pasteboard);

        // Convert the string to NSString (CFStringRef)
        var str = Marshal.StringToHGlobalUni(text);
        var type = Marshal.StringToHGlobalUni("public.utf8-plain-text");

        NSPasteboard_setString(pasteboard, str, type);

        Marshal.FreeHGlobal(str);
        Marshal.FreeHGlobal(type);
    }
}

#endif
