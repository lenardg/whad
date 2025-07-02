namespace WhatHaveIDone;

public class Settings
{
    public string LoggingFolder { get; set; } = "logs";

	public string[] IdleProcesses { get; set; } = [];

    public Dictionary<string, ProcessOptions> ProcessSettings { get; set; } = new();
}

public class ProcessOptions
{
    /// <summary>
    /// Should we separate the window title into parts?
    /// </summary>
    public bool DoSeparations { get; set; } = true;

    /// <summary>
    /// The separator to use for separating the window title into parts.
    /// </summary>
    public string Separator { get; set; } = " - ";

    /// <summary>
    /// After separating the window title into parts, which parts should we keep?
    /// </summary>
    public int[] KeepParts { get; set; } = Array.Empty<int>();

    /// <summary>
    /// After separating the window title into parts, which parts should we remove?
    /// </summary>
    public int[] RemoveParts { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Should we track individual entries by window title?
    /// </summary>
    public bool EntriesByWindowTitle { get; set; } = true;

    /// <summary>
    /// Trim any characters off the title before processing
    /// </summary>
    public string[] TrimCharacters { get; set; } = Array.Empty<string>();

	public string[] GroupBySeparators { get; set; } = Array.Empty<string>();
}
