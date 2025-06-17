using System.Text.Json;

namespace WhatHaveIDone;

public class WindowLoggerMain {
	public static async Task Main(string[] args) {
		using var cts = new CancellationTokenSource();
		Console.CancelKeyPress += (s, e) => {
			e.Cancel = true; // Prevent immediate termination
			cts.Cancel();
		};

		Console.WriteLine("What HAve (I) Done (WHAD) - v1.0.0");
		Console.WriteLine("Copyright (c) 2025 by Lenard Gunda");
		Console.WriteLine();

		Settings settings = LoarOrInitSettings();

		// Program entry point
		if (args.Length > 0) {
			if (ParseCommandLine(settings, args) == false) {
				return;
			}
		}

		await RunMonitoring(cts, settings);
	}

	private static async Task RunMonitoring(CancellationTokenSource cts, Settings settings) {
		Console.WriteLine("Press Ctrl+C to exit");

		var logger = new WindowLogger(settings);
		try {
			await logger.StartAsync(cts.Token);
		}
		catch (OperationCanceledException) {
			Console.WriteLine("\nShutting down gracefully...");
		}

		Console.WriteLine("Quitting.");
	}

	private static Settings LoarOrInitSettings() {
		Settings settings = new Settings();
		// load settings from settings.json
		if (File.Exists("settings.json")) {
			var jsonString = File.ReadAllText("settings.json");
			settings = JsonSerializer.Deserialize<Settings>(jsonString) ?? new Settings();
		}
		else {
			Console.WriteLine("No settings file found, using default settings");
		}

		return settings;
	}

	private static bool ParseCommandLine(Settings settings, string[] args) {
		try {
			var commandLineArgs = CommandLineArgs.Parse(args);

			if (commandLineArgs.HadErrors) {
				PrintUsage();
				return false;
			}

			if (commandLineArgs.DayFileName != null) {
				var requestedFilename = commandLineArgs.DayFileName;

				// Try to parse the provided filename as a date first
				if (DateTime.TryParse(commandLineArgs.DayFileName, out DateTime date)) {
					requestedFilename = $"window_times_{date:yyyy-MM-dd}.json";
				}

				var logfilename = Path.Combine(settings.LoggingFolder, requestedFilename);

				if (File.Exists(logfilename)) {
					var jsonContent = File.ReadAllText(logfilename);
					var processWindowTimes = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(jsonContent);
					if (processWindowTimes != null) {
						Console.WriteLine($"Summary for {logfilename}:");
						Console.WriteLine("----------------------------------------");
						WindowLogger.ShowProcessTimes(processWindowTimes, commandLineArgs.DoShowAll, commandLineArgs.DoCopyToClipboard);
					}
					else {
						Console.WriteLine("Error: Invalid log file format");
					}
					return false;
				}
				else {
					Console.WriteLine($"Error: File {logfilename} not found");
					return false;
				}
			}
		}
		catch (Exception ex) {
			Console.WriteLine($"Error loading log file: {ex.Message}");
			return false;
		}
		return true;
	}

	private static void PrintUsage() {
		Console.WriteLine("Usage: WindowLogger [options] [filename]");
		Console.WriteLine("Options:");
		Console.WriteLine("  --copy, -c       Copy output to clipboard");
		Console.WriteLine("  --all, -a        Show all entries, not summarizing small ones");
		Console.WriteLine("  date             Date of log to print, use system date format");
		Console.WriteLine();
		Console.WriteLine("If no date is provided, logging will start.");
		Console.WriteLine();
	}
}

