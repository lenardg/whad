# What Have (I) Done?

A command-line utility that tracks how much time you spend in different windows and applications throughout the day.

## History

This started as an experiment using Cursor to log who I talk with over Slack. It then evolved into a full blown experiment of
testing Coding Agents, that ended in frustration. I left a single Cursor rule file in place, to remind me of the horrors of
this vibe coding experience.

In the end, the tool became quite useful so now here it is.

## Requirements

Currently WHAD runs under .NET 10. It supports Windows only at the moment.

Preliminary MacOS support is "AI coded" but requires some human coding still :) PRs are welcome.

## Features

- Automatically tracks active window titles and process names
- Can split up titles so you can for example group Visual Studio Code windows by project but not by file, or not track separately files that are changed vs not changed (often a * in the name)
- Logs time spent in each window/application
- Saves daily logs in JSON format
- Can pin a window (like a Teams meeting) so regardless of what you do it tracks tha pinned window. Stops when closed.
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
* `P` - pin the previous process. it stays pinned until unpinned or closed
* `U` - unpin
* `Ctrl-C` - close the app

#### Pinning a process

When you pin a process (by pressing `P`) WHAD will keep monitoring that process until the pinned item quits or until you unpin the process. For example, if you pin a Microsoft Teams meeting, you
can then move to other windows to check things, to take notes, etc, but WHAD will ignore these Window changes, and everything will be counted towards the pinned window. You can create other workflows
for yourself, like pinning to a Visual Studio window. Just make sure you close the window or unpin it when you stop working on the project, otherwise all your hours go towards that one window.

Quitting means simply the window is closed, not that the entire process quits. When a meeting ends, the window closes automatically and the pinning will stop.

Pinning pins the _previous_ active window as the pinned item. This is because when you switch to WHAD, it will be the active window, so it makes no sense to pin that one.

You can see what is pinned because in that case WHAD will tell you _Pinned window_ instead of _Active window_.


### Checking logs

You can check logs by specifying which day to.

```bash
whad.exe 2025-06-04
dotnet run -- 2025-06-04
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

For per process settings:

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
