# What Have (I) Done?

A command-line utility that tracks how much time you spend in different windows and applications throughout the day.

## History

This started as an experiment using Cursor to log who I talk with over Slack. It then evolved into a full blown experiment of
testing Coding Agents, that ended in frustration. I left a single Cursor rule file in place, to remind me of the horrors of
this vibe coding experience.

In the end, the tool became quite useful so now here it is.

## Requirements

Currently WHAD runs under .NET 8 or .NET 9. It supports Windows only at the moment. Preliminary MacOS support is "AI coded" but
requires some human coding still :) PRs are welcome.

## Features

- Automatically tracks active window titles and process names
- Logs time spent in each window/application
- Saves daily logs in JSON format
- Can display summaries with hierarchical breakdown of time spent
- Handles day changes automatically
- Configurable via settings.json
- Customizable logging folder location

## Usage

### Running the Logger

Simply run the application to start tracking window times:

```bash
whad.exe
```

or if you do not want to publish executables, you can use the __dotnet__ tool to run it:

```bash
dotnet run
```

### Summaries and details

While the app is logging, you can press the following keys:

* `S` - display a summary for the running day
* `D` - display details for the running day
* `Ctrl-C` - close the app


### Checking logs

You can check logs by specifying which day to.

```bash
whad.exe 2025-06-04
```

This will display logs for 4th of June, 2025. Date parsing uses default setting for the computer.

You can specify arguments:

- `--all` or `-a` will show all entries, not just those that are longer than 1 minute
- `--copy` or `-c` will copy results to clipboard

## Configuration

The application can be configured through a `settings.json` file. This file allows you to customize how window titles are processed for different applications and where log files are stored.

### Settings Structure

```json
{
    "LoggingFolder": "logs",
    "ProcessSettings": {
        "ProcessName": {
            "DoSeparations": true,
            "Separator": " - ",
            "KeepParts": [0, 1],
            "RemoveParts": [0],
            "EntriesByWindowTitle": true,
            "TrimCharacters": ["*"]
        }
    }
}
```

### Configuration Options

- `LoggingFolder`: The folder where daily log files will be stored (default: "logs")
- `ProcessName`: The name of the process to configure (e.g., "chrome", "slack", "code")
- `DoSeparations`: Whether to split the window title into parts using the separator
- `Separator`: The character(s) to use when splitting the window title (default: " - ")
- `KeepParts`: Array of indices for parts to keep after splitting (0-based)
- `RemoveParts`: Array of indices for parts to remove after splitting (0-based)
- `EntriesByWindowTitle`: Whether to track individual entries by window title (default: true)
- `TrimCharacters`: Array of characters to remove from the title before processing

### Log File Location

By default, the application stores log files in a `logs` folder in the same directory as the executable. You can change this location by setting the `LoggingFolder` property in `settings.json`:

```json
{
    "LoggingFolder": "C:\\MyLogs\\WindowLogger"
}
```

The application will:
- Create the specified folder if it doesn't exist
- Store all daily log files in this folder
- Use the format `window_times_YYYY-MM-DD.json` for log files

### Example Configurations

1. **Slack Configuration**
```json
"Slack": {
    "TrimCharacters": ["*"],
    "DoSeparations": true,
    "Separator": " - ",
    "KeepParts": [0, 1]
}
```
This will:
- Remove asterisks from titles
- Split titles by " - "
- Keep the first two parts (e.g., "General - Workspace" from "General - Workspace - Slack")

2. **Visual Studio Code Configuration**
```json
"Code": {
    "DoSeparations": true,
    "Separator": " - ",
    "RemoveParts": [0],
    "TrimCharacters": ["●"]
}
```
This will:
- Remove the bullet point character (when a file has changes)
- Split titles by " - "
- Remove the first part (e.g., "Program.cs" from "● Program.cs - WHAD - Visual Studio Code"). This will group the Visual Studio Code windows without taking into account the actual filename

3. **Total Commander Configuration**
```json
"TOTALCMD64": {
    "EntriesByWindowTitle": false
}
```
This will:
- Track all Total Commander windows as a single entry without separating by title

### Default Behavior

If no settings are specified for a process, the application will:
- Track individual entries by window title
- Not perform any title processing or separation
- Use the full window title as is
