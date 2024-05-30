

using System.Diagnostics;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Provides settings config for the application.
    /// </summary>
    internal class AppSettings {
        /// <summary>
        /// Settings object for use with JSON serialization/deserialization.
        /// </summary>
        internal class SettingsObject {
            internal bool DisplayMusicStatus { get; set; }
            internal bool ShowStatusOnPause { get; set; }
            internal bool LaunchAtStartup { get; set; }
        }

        /// <summary>
        /// Display Music Status toggle setting.
        /// </summary>
        public static bool DisplayMusicStatusToggle {
            get {
                return GetSettingsObject().DisplayMusicStatus;
            }
            set {
                SettingsObject settingsObject = GetSettingsObject();
                settingsObject.DisplayMusicStatus = value;
                UpdateSettings(settingsObject);
            }
        }

        /// <summary>
        /// Show Status on Pause toggle setting.
        /// </summary>
        internal static bool ShowStatusOnPauseToggle {
            get {
                return GetSettingsObject().ShowStatusOnPause;
            }
            set {
                SettingsObject settingsObject = GetSettingsObject();
                settingsObject.ShowStatusOnPause = value;
                UpdateSettings(settingsObject);
            }
        }

        /// <summary>
        /// Launch at Startup toggle setting.
        /// </summary>
        internal static bool LaunchAtStartupToggle {
            get {
                return GetSettingsObject().LaunchAtStartup;
            }
            set {
                SettingsObject settingsObject = GetSettingsObject();
                settingsObject.LaunchAtStartup = value;
                UpdateSettings(settingsObject);

                if (value) AddStartupShortcut();
                else RemoveStartupShortcut();
            }
        }
        /// <summary>
        /// Creates AppleMusic_Discord_Status/settings.json in AppData folder.
        /// </summary>
        internal static void InitializeSettings() {
            Directory.CreateDirectory(Constants.AppDataSettingsFolder);
            if (!File.Exists(Constants.AppDataSettingsPath)) InitializeDefaultSettings();
        }

        /// <summary>
        /// Writes default settings to settings.json.
        /// </summary>
        internal static void InitializeDefaultSettings() {
            string DefaultSettingsJSON = JsonSerializer.Serialize(Constants.AppDefaultSettings);
            File.WriteAllText(Constants.AppDataSettingsPath, DefaultSettingsJSON);
        }

        /// <summary>
        /// Gets settings object from settings.json.
        /// </summary>
        /// <returns>Settings object.</returns>
        internal static SettingsObject GetSettingsObject() {
            string JSON = File.ReadAllText(Constants.AppDataSettingsPath);
            return JsonSerializer.Deserialize<SettingsObject>(JSON);
        }

        /// <summary>
        /// Update settings.json with settings object.
        /// </summary>
        /// <param name="settingsObject">Object that includes all settings.</param>
        internal static void UpdateSettings(SettingsObject settingsObject) {
            string JSON = JsonSerializer.Serialize(settingsObject);
            File.WriteAllText(Constants.AppDataSettingsPath, JSON);
        }

        /// <summary>
        /// Adds application shortcut to Windows startup folder, so that it launches at startup.
        /// </summary>
        internal static void AddStartupShortcut() {
            Type t = Type.GetTypeFromCLSID(new Guid(Constants.WindowsScriptHostShellObjectGUID));
            dynamic shell = Activator.CreateInstance(t);

            if (shell == null) return;

            try {
                dynamic lnk = shell.CreateShortcut(Constants.AppShortcutPath);

                if (lnk == null) {
                    Marshal.FinalReleaseComObject(shell);
                    return;
                }

                try {
                    lnk.TargetPath = Constants.AppExePath;
                    lnk.IconLocation = $"{Constants.AppExePath}, 0";
                    lnk.Save();
                } catch (UnauthorizedAccessException) {
                    Debug.WriteLine("Unauthorized access to save shortcut.");
                } finally {
                    Marshal.FinalReleaseComObject(lnk);
                }

            } finally {
                Marshal.FinalReleaseComObject(shell);
            }
        }

        /// <summary>
        /// Removes application shortcut from Windows startup folder, so that it will not launch at startup.
        /// </summary>
        internal static void RemoveStartupShortcut() {
            File.Delete(Constants.AppShortcutPath);
        }
    }
}
