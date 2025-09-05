
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WhatHaveIDone;

// This class is used to abstract away the specific OS code
public static class SystemInterop {
#if WINDOWS
	public const string NO_ACTIVE_WINDOW = "--Windows System--";
	public const string NO_ACTIVE_PROCESS = "--Windows System--";
#elif MACOS
    public const string NO_ACTIVE_WINDOW = "--macOS System--";
    public const string NO_ACTIVE_PROCESS = "--macOS System--";
#endif

	public static void CopyToClipboard(string text) {
#if WINDOWS
		Win32Interop.CopyToClipboard(text);
#elif MACOS
            MacOSInterop.CopyToClipboard(text);
#endif
	}

#if WINDOWS
	public static WindowInfo? GetActiveWindowInfo() {
		var windowHandle = Win32Interop.GetForegroundWindow();
		if (windowHandle == IntPtr.Zero) {
			return new WindowInfo { Title = NO_ACTIVE_WINDOW, ProcessName = NO_ACTIVE_PROCESS };
		}

		return GetWindowInfo ( windowHandle );
	}

	public static WindowInfo? GetWindowInfo( IntPtr windowHandle ) {
		// Get window title
		var titleBuilder = new StringBuilder(256);
		if (Win32Interop.GetWindowText(windowHandle, titleBuilder, 256) == 0) {
			return new WindowInfo { Title = NO_ACTIVE_WINDOW, ProcessName = NO_ACTIVE_PROCESS };
		}

		// Get process name
		Win32Interop.GetWindowThreadProcessId(windowHandle, out uint processId);
		try {
			using var process = Process.GetProcessById((int)processId);
			var windowInfo = new WindowInfo {
				Title = titleBuilder.ToString(),
				ProcessName = process.ProcessName,
				WindowHandle = windowHandle,
			};
			return windowInfo;
		}
		catch (ArgumentException) {
			return null;
		}
	}
#endif

#if MACOS

    public static WindowInfo? GetActiveWindowInfo()
    {
		var (title, pid) = MacOSInterop.GetForegroundWindowInfo();
		if (title == null || pid == 0)
		{
			return new WindowInfo { Title = NO_ACTIVE_WINDOW, ProcessName = NO_ACTIVE_PROCESS };
		}

		var processName = MacOSInterop.GetProcessName(pid);
		if (processName == null)
		{
			return new WindowInfo { Title = NO_ACTIVE_WINDOW, ProcessName = NO_ACTIVE_PROCESS };
		}

		return new WindowInfo { Title = title, ProcessName = processName };
    }

#endif

}
