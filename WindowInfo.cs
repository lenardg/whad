namespace WindowLogger;

/// <summary>
/// Information about the current window
/// </summary>
public class WindowInfo {
	public string Title { get; set; } = string.Empty;
	public string ProcessName { get; set; } = string.Empty;

	public void ProcessTitle(Settings settings) {
		if (settings.ProcessSettings.ContainsKey(ProcessName)) {
			var processSettings = settings.ProcessSettings[ProcessName];

			if (processSettings.EntriesByWindowTitle == false) {
				Title = ProcessName;
				return;
			}

			if (processSettings.TrimCharacters != null && processSettings.TrimCharacters.Length > 0) {
				foreach (var trimCharacter in processSettings.TrimCharacters) {
					Title = Title.Trim(trimCharacter[0]);
				}
			}

			Title = Title.Trim();

			if (processSettings.DoSeparations) {
				var parts = Title.Split(processSettings.Separator);
				if (processSettings.KeepParts != null && processSettings.KeepParts.Length > 0) {
					List<string> keptParts = new List<string>();
					for (int partIndex = 0; partIndex < parts.Length; partIndex++) {
						if (processSettings.KeepParts.Contains(partIndex)) {
							keptParts.Add(parts[partIndex]);
						}
					}
					Title = string.Join(processSettings.Separator, keptParts);
				}
				else if (processSettings.RemoveParts != null && processSettings.RemoveParts.Length > 0) {
					List<string> keptParts = new List<string>();
					for (int partIndex = 0; partIndex < parts.Length; partIndex++) {
						if (!processSettings.RemoveParts.Contains(partIndex)) {
							keptParts.Add(parts[partIndex]);
						}
					}
					Title = string.Join(processSettings.Separator, keptParts);
				}
				else if (processSettings.GroupBySeparators != null && processSettings.GroupBySeparators.Length > 0) {
					// if any parts will match a groupby separator, we keep that part. In the end we concat the kept parts
					// together separated by the separator.
					List<string> keptParts = new List<string>();
					foreach (var separator in processSettings.GroupBySeparators) {
						foreach (var part in parts) {
							if (part == separator) {
								keptParts.Add(part);
								break;
							}
						}
					}
					if (keptParts.Count > 0) {
						Title = string.Join(processSettings.Separator, keptParts);
					}
				}
			}
		}
	}
}
