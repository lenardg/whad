using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace WhatHaveIDone;

public class WindowLogger {
	private string? currentWindowTitle;
	private string? currentProcessName;
	private readonly Stopwatch stopwatch;
	private Dictionary<string, Dictionary<string, double>> processWindowTimes;
	private DateTime todayDate;
	private const int POLLING_INTERVAL_MS = 1100;

	private readonly Settings settings;

	public static void ShowProcessTimes(Dictionary<string, Dictionary<string, double>> processWindowTimes, bool showAll = false, bool copyToClipboard = false) {
		var output = new StringBuilder();
		if (processWindowTimes != null) {
			foreach (var process in processWindowTimes) {
				var totalTime = process.Value.Values.Sum();
				if (!showAll && totalTime < 1.0) {
					continue;
				}
				var totalTimeSpan = TimeSpan.FromMinutes(totalTime);
				output.AppendLine($"\n[{process.Key}] Total: {totalTimeSpan:hh\\:mm\\:ss}");

				var hasMicroPeriods = false;
				var hasRegularPeriods = false;
				var microPeriodTotalMinutes = 0.0;
				foreach (var window in process.Value.OrderByDescending(w => w.Value)) {
					if (window.Value < 1.0) {
						hasMicroPeriods = true;
						microPeriodTotalMinutes += window.Value;
						continue;
					}
					hasRegularPeriods = true;
					var timeSpan = TimeSpan.FromMinutes(window.Value);
					output.AppendLine($"  + {timeSpan:hh\\:mm\\:ss} - {window.Key}");
				}
				if (hasMicroPeriods && hasRegularPeriods) {
					var smallTimeSpan = TimeSpan.FromMinutes(microPeriodTotalMinutes);
					output.AppendLine($"  + {smallTimeSpan:hh\\:mm\\:ss} [several micro periods, total]");
				}
			}
		}
		if (copyToClipboard) {
			SystemInterop.CopyToClipboard(output.ToString());
		}
		else {
			Console.WriteLine(output.ToString());
		}
	}

	private void ShowQuickSummary(bool showAll = false, bool showDetails = false ) {
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("\n===================================================================");
		Console.WriteLine("Summary");
		Console.WriteLine("===================================================================");
		Console.ResetColor();

		if (processWindowTimes != null) {
			var longestProcessName = processWindowTimes.Keys.Max(x => x.Length);

			var processesInDescendingOrder = processWindowTimes.OrderByDescending(v => v.Value.Values.Sum());
			foreach (var process in processesInDescendingOrder) {
				var totalTime = process.Value.Values.Sum();
				if (!showAll && totalTime < 1.0) {
					continue;
				}

				var totalTimeSpan = TimeSpan.FromMinutes(totalTime);
				Console.Write("[");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write(string.Format($"{{0,-{longestProcessName}}}", process.Key));
				Console.ResetColor();
				Console.Write("] Total: ");
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write($"{totalTimeSpan:hh\\:mm\\:ss}");
				Console.ResetColor();
				Console.WriteLine("");

				if (showDetails) {
					var hasMicroPeriods = false;
					var hasRegularPeriods = false;
					var microPeriodTotalMinutes = 0.0;
					foreach (var window in process.Value.OrderByDescending(w => w.Value)) {
						if (window.Value < 1.0) {
							hasMicroPeriods = true;
							microPeriodTotalMinutes += window.Value;
							continue;
						}

						hasRegularPeriods = true;
						var timeSpan = TimeSpan.FromMinutes(window.Value);

						Console.Write("  + ");
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.Write($"{timeSpan:hh\\:mm\\:ss}");
						Console.ResetColor();
						Console.WriteLine($" - {window.Key}");
					}

					if (hasMicroPeriods && hasRegularPeriods) {
						var smallTimeSpan = TimeSpan.FromMinutes(microPeriodTotalMinutes);
						Console.Write("  + ");
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.Write($"{smallTimeSpan:hh\\:mm\\:ss}");
						Console.ResetColor();
						Console.WriteLine($" - [several micro periods, total]");
					}
					Console.WriteLine("");
				}
			}

			var allProcessTimes = processWindowTimes
						.Sum(p => p.Value.Values.Sum());
			var allProcessTimesSpan = TimeSpan.FromMinutes(allProcessTimes);

			Console.WriteLine("");
			Console.Write(string.Format($"{{0,-{longestProcessName + 1}}}", "TOTAL"));
			Console.Write("] Total: ");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write($"{allProcessTimesSpan:hh\\:mm\\:ss}");
			Console.ResetColor();
			Console.WriteLine("");
			Console.WriteLine("===================================================================");
		}
	}

	private void HandleDayChange() {
		var today = DateTime.Now.Date;
		if (today != todayDate) {
			Console.WriteLine("\nDay changed! Saving previous day's records...");

			// Save current window time and all accumulated times for the previous day
			ShowCurrentProcessTimes();
			SaveLog(todayDate); // Save to the previous day's file

			// Reset tracking for the new day
			todayDate = today;
			processWindowTimes = LoadLog(); // This will load any existing records for the new day
			currentWindowTitle = null;
			currentProcessName = null;
			stopwatch.Reset();

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("==================================================================");
			Console.WriteLine($"Starting new day tracking for {today:yyyy-MM-dd}");
			Console.WriteLine("==================================================================");
			Console.ResetColor();

			DisplayLoadedWindowTimes();
			Console.WriteLine();
		}
	}

	public WindowLogger(Settings settings) {
		this.settings = settings;
		stopwatch = new Stopwatch();
		todayDate = DateTime.Now.Date;
		processWindowTimes = LoadLog();
		DisplayLoadedWindowTimes();
		Console.WriteLine();
	}

	private string GetFilenameForLog(DateTime date) {
		return Path.Combine(settings.LoggingFolder, $"window_times_{date:yyyy-MM-dd}.json");
	}

	private Dictionary<string, Dictionary<string, double>> LoadLog() {
		var fileName = GetFilenameForLog(todayDate);
		if (File.Exists(fileName)) {
			var jsonString = File.ReadAllText(fileName);
			return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(jsonString)
				?? new Dictionary<string, Dictionary<string, double>>();
		}
		return new Dictionary<string, Dictionary<string, double>>();
	}

	private void DisplayLoadedWindowTimes() {
		if (processWindowTimes.Count > 0) {
			Console.WriteLine("Times for today (where total time > 1 minute):");
			Console.WriteLine("----------------------------------------------");
			ShowProcessTimes(processWindowTimes);
		}
	}

	private void SaveLog(DateTime date) {
		var fileName = GetFilenameForLog(date);
		var jsonString = JsonSerializer.Serialize(processWindowTimes, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(fileName, jsonString);
	}

	private void RecordProcessTime(string processName, string windowTitle, double minutes) {
		if (!processWindowTimes.ContainsKey(processName)) {
			processWindowTimes[processName] = new Dictionary<string, double>();
		}

		if (processWindowTimes[processName].ContainsKey(windowTitle)) {
			processWindowTimes[processName][windowTitle] += minutes;
		}
		else {
			processWindowTimes[processName][windowTitle] = minutes;
		}
		SaveLog(todayDate);
	}

	private void HandleWindowChange(string? newProcessName, string? newWindowTitle) {
		ShowCurrentProcessTimes();
		currentProcessName = newProcessName;
		currentWindowTitle = newWindowTitle;
		stopwatch.Restart();
	}

	private void ShowCurrentProcessTimes() {
		if (currentProcessName != null && currentWindowTitle != null) {
			var elapsed = stopwatch.Elapsed;
			var minutes = elapsed.TotalMinutes;
			Console.Write("Active window: [");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(currentProcessName);
			Console.ResetColor();
			Console.Write("] ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(currentWindowTitle);
			Console.ResetColor();
			Console.Write(" for ");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write($"{minutes:F1}");
			Console.ResetColor();
			Console.WriteLine(" minutes");
			RecordProcessTime(currentProcessName, currentWindowTitle, minutes);
		}
	}

	private void UpdateStopwatchState(bool hasActiveWindow) {
		if (!hasActiveWindow && stopwatch.IsRunning) {
			stopwatch.Stop();
		}
		else if (hasActiveWindow && !stopwatch.IsRunning && currentWindowTitle != null) {
			stopwatch.Start();
		}
	}

	public async Task StartAsync(CancellationToken cancellationToken) {
		try {
			while (!cancellationToken.IsCancellationRequested) {
				HandleDayChange();

				var windowInfo = SystemInterop.GetActiveWindowInfo();
				windowInfo?.ProcessTitle(settings);

				var windowTitle = windowInfo?.Title;
				var processName = windowInfo?.ProcessName;
				var hasActiveWindow = !string.IsNullOrEmpty(windowTitle) && !string.IsNullOrEmpty(processName);

				UpdateStopwatchState(hasActiveWindow);

				if (hasActiveWindow && windowTitle != null && processName != null) {
					if (windowTitle != currentWindowTitle || processName != currentProcessName) {
						HandleWindowChange(processName, windowTitle);
					}
				}

				if (hasActiveWindow && windowInfo != null) {
					var currentTime = stopwatch.Elapsed.TotalMinutes;
					var totalTime = currentTime;

					// Add previously recorded time if it exists
					if (!string.IsNullOrEmpty(processName) && !string.IsNullOrEmpty(windowTitle) &&
						processWindowTimes.ContainsKey(processName) &&
						processWindowTimes[processName].ContainsKey(windowTitle)) {
						currentTime += processWindowTimes[processName][windowTitle];
						totalTime += processWindowTimes[processName].Values.Sum();
					}
					else if (!string.IsNullOrEmpty(processName) && processWindowTimes.ContainsKey(processName)) {
						totalTime += processWindowTimes[processName].Values.Sum();
					}

					var truncatedTitle = windowTitle != null && windowTitle.Length > 50
						? $"{windowTitle[..20]}...{windowTitle[^20..]}"
						: windowTitle;

					string FormatTime(double time) =>
						time >= 60
							? $"{(int)(time / 60):D2}:{(int)(time % 60):D2}:{(int)((time % 1) * 60):D2}"
							: $"{(int)time:D2}:{(int)((time % 1) * 60):D2}";

					var currentTimeFormatted = FormatTime(currentTime);
					var totalTimeFormatted = FormatTime(totalTime);

					var padding = new string(' ', Console.WindowWidth - 1);
					Console.Write(padding);
					Console.Write("\r");

					Console.Write($"Active window: [");
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write(windowInfo.ProcessName);
					Console.ResetColor();
					Console.Write("] ");
					Console.ForegroundColor = ConsoleColor.White;
					Console.Write(truncatedTitle);
					Console.ResetColor();
					Console.Write(" (Current window: ");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write(currentTimeFormatted);
					Console.ResetColor();
					Console.Write(", App total: ");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write(totalTimeFormatted);

					Console.ResetColor();
					Console.Write(") ==> Today: ");
					Console.ForegroundColor = ConsoleColor.Cyan;
					// Only sum other processes' times, since currentTime already includes the current process
					var otherProcessTimes = processWindowTimes
						.Where(p => p.Key != processName)
						.Sum(p => p.Value.Values.Sum());
					Console.Write(FormatTime(totalTime + otherProcessTimes));

					Console.ResetColor();
					Console.Write("         \r");
				}
				else {
					Console.Write("No active window     \r");
				}

				if (Console.KeyAvailable) {
					var key = Console.ReadKey();
					if (key.Key == ConsoleKey.S) {
						ShowQuickSummary();
					}
					if (key.Key == ConsoleKey.D) {
						ShowQuickSummary(showDetails: true);
					}
				}

				await Task.Delay(POLLING_INTERVAL_MS, cancellationToken);
			}
		}
		finally {
			ShowCurrentProcessTimes(); // Save the final window time before exiting
		}
	}
}



