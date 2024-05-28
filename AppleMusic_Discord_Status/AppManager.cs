using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Timers;
using System.Windows.Automation;
using Windows.Storage;
using System.Runtime.InteropServices;
using System.IO;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Manages Application states and Discord status updates.
    /// </summary>
    public class AppManager {
        private Timer statusRefreshTimer;
        private DiscordRichPresence DiscordBot { get; set; }
        private string currentSong;
        private string currentAlbumArtwork;
        private string currentSongUrl;
        public bool DiscordIsOpen { get; set; }
        public bool AppleMusicIsOpen { get; set; }
        public bool MiniPlayerIsOpen { get; set; }

        public static bool DisplayMusicStatusToggleState {
            get {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue("DisplayMusicStatus", out object value)) {
                    return (bool)value;
                }
                return false;
            }
            set {
                ApplicationData.Current.LocalSettings.Values["DisplayMusicStatus"] = value;
            }
        }

        public static bool ShowStatusOnPauseToggleState {
            get {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue("ShowStatusOnPause", out object value)) {
                    return (bool)value;
                }
                return false;
            }
            set {
                ApplicationData.Current.LocalSettings.Values["ShowStatusOnPause"] = value;
            }
        }

        public static bool LaunchAtStartupToggleState {
            get {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue("LaunchAtStartup", out object value)) {
                    return (bool)value;
                }
                return false;
            }
            set {
                ApplicationData.Current.LocalSettings.Values["LaunchAtStartup"] = value;
                if (value) AddStartupShortcut();
                else RemoveStartupShortcut();
            }
        }

        /// <summary>
        /// Initializes refresh timer and Discord RPC client.
        /// </summary>
        public AppManager() {
            this.InitializeTimer();
            this.InitializeDiscordBot();
        }

        /// <summary>
        /// Initializes refresh timer for updating Discord status and application states.
        /// </summary>
        private void InitializeTimer() {
            this.statusRefreshTimer = new Timer(Constants.AppRefreshRate);
            this.statusRefreshTimer.Elapsed += OnStatusRefreshTimerElapsed;
            this.statusRefreshTimer.Start();
        }

        /// <summary>
        /// Initializes Discord RPC client.
        /// </summary>
        private void InitializeDiscordBot() {
            this.DiscordBot = new DiscordRichPresence();
        }

        /// <summary>
        /// Disposes the Discord client connection when the application is closed.
        /// This prevents memory leaks, probably.
        /// </summary>
        /// <param name="_">Sender.</param>
        /// <param name="__">Args.</param>
        public void OnAppExit(object _, WindowEventArgs __) {
            this.DiscordBot.Dispose();
        }

        /// <summary>
        /// Event handler for the elapsed event of the status refresh timer. 
        /// Refreshes application status.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Args containing event data.</param>
        private void OnStatusRefreshTimerElapsed(object sender, ElapsedEventArgs args) {
            RefreshStatus();
        }

        /// <summary>
        /// Refreshes application status and Discord status.
        /// Sends Rich Presence to Discord if Discord, Apple Music, and Apple Music Mini Player are all detected.
        /// </summary>
        async public void RefreshStatus() {
            this.DiscordIsOpen = Process.GetProcessesByName(Constants.DiscordAppName).Length > 0;
            this.AppleMusicIsOpen = Process.GetProcessesByName(Constants.AppleMusicAppName).Length > 0;
            RefreshMiniPlayerStatus();
            MainWindow window = (App.Current as App)?.m_window as MainWindow;

            window.DispatcherQueue.TryEnqueue(() => {
                window.UpdateStatusIcons();
            });

            if (
                this.DiscordIsOpen &&
                this.AppleMusicIsOpen &&
                this.MiniPlayerIsOpen &&
                DisplayMusicStatusToggleState
            ) {
                (string songName, string songArtist, string songAlbum, string timeLeft, bool isPlaying) = AppleMusicScraper.Scrape();

                if (!isPlaying && !ShowStatusOnPauseToggleState) {
                    this.DiscordBot.Dispose();
                    return;
                }

                if (songName != currentSong) {
                    this.currentSong = songName;
                    this.currentAlbumArtwork = await ITunesAPI.GetAlbumArtworkUrl(songAlbum, songArtist);
                    this.currentSongUrl = await ITunesAPI.GetSongLink(songName, songArtist);
                }

                DiscordBot.UpdatePresence(
                    this.currentSong,
                    $"by {songArtist} — {songAlbum}",
                    this.currentAlbumArtwork,
                    timeLeft,
                    this.currentSongUrl,
                    isPlaying
                );
            }
        }

        /// <summary>
        /// Refreshes status of Apple Music Mini Player.
        /// </summary>
        public void RefreshMiniPlayerStatus() {
            try {
                if (!this.AppleMusicIsOpen) {
                    this.MiniPlayerIsOpen = false;
                } else {
                    nint miniPlayerHandle = AppleMusicScraper.GetMiniPlayerWindowHandle();
                    AutomationElement miniPlayerWindow = AppleMusicScraper.GetMiniPlayerWindow(miniPlayerHandle);
                    this.MiniPlayerIsOpen = miniPlayerWindow != null;
                }
            } catch (Exception exception) {
                Debug.WriteLine($"Exception in RefreshMiniPlayerStatus: {exception}");
            }
        }

        /// <summary>
        /// Updates the status icons for Discord, Apple Music, and Apple Music Mini Player.
        /// Red X = Not Detected
        /// Green Checkmark = Detected
        /// </summary>
        /// <param name="DiscordStatusIcon">Discord status FontIcon.</param>
        /// <param name="AppleMusicStatusIcon">Apple Music status FontIcon.</param>
        /// <param name="MiniPlayerStatusIcon">Apple Music Mini Player status FontIcon.</param>
        public void UpdateStatusIcons(
            FontIcon DiscordStatusIcon,
            FontIcon AppleMusicStatusIcon,
            FontIcon MiniPlayerStatusIcon
        ) {
            Debug.WriteLine($"DiscordIsOpen: {this.DiscordIsOpen}");
            Debug.WriteLine($"AppleMusicIsOpen: {this.AppleMusicIsOpen}");
            Debug.WriteLine($"MiniPlayerIsOpen: {this.MiniPlayerIsOpen}");

            UpdateStatusIcon(DiscordStatusIcon, this.DiscordIsOpen);
            UpdateStatusIcon(AppleMusicStatusIcon, this.AppleMusicIsOpen);
            UpdateStatusIcon(MiniPlayerStatusIcon, this.MiniPlayerIsOpen);
        }

        /// <summary>
        /// Updates status FontIcon with either green checkmark or red x.
        /// </summary>
        /// <param name="fontIcon">Current status icon.</param>
        /// <param name="status">Current application status.</param>
        private static void UpdateStatusIcon(FontIcon fontIcon, bool status) {
            if (status) {
                fontIcon.Glyph = "\uE73E";
                fontIcon.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
            } else {
                fontIcon.Glyph = "\uE711";
                fontIcon.Foreground = new SolidColorBrush(Microsoft.UI.Colors.IndianRed);
            }
        }

        /// <summary>
        /// Adds application shortcut to Windows startup folder, so that it launches at startup.
        /// </summary>
        private static void AddStartupShortcut() {
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
        private static void RemoveStartupShortcut() {
            File.Delete(Constants.AppShortcutPath);
        }
    }
}
