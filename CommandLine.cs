namespace WhatHaveIDone;

public class CommandLineArgs {
	public string? DayFileName { get; private set; }
	public bool DoCopyToClipboard { get; private set; } = false;
	public bool DoShowAll { get; private set; } = false;
	public bool HadErrors { get; set; } = false;

	public static CommandLineArgs Parse(string[] args) {
		var result = new CommandLineArgs();

		if (args.Length == 0) {
			return result;
		}

		for (int i = 0; i < args.Length; i++) {
			if (args[i].StartsWith("--") || args[i].StartsWith("-")) {
				if (args[i] == "--copy" || args[i] == "-c") {
					result.DoCopyToClipboard = true;
					continue;
				}
				else if (args[i] == "--all" || args[i] == "-a") {
					result.DoShowAll = true;
					continue;
				}
				else {
					result.HadErrors = true;
				}
			}

			// If not a flag, treat as filename
			if (!args[i].StartsWith("-")) {
				result.DayFileName = args[i];
			}
		}

		return result;
	}
}
