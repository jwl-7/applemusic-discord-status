using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.Timers;
using System.Windows.Automation;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Manages Application states and Discord status updates.
    /// </summary>
    internal class AppManager {
        /// <summary>
        /// Initializes refresh timer for updating Discord status and application states.
        /// </summary>
        internal static void InitializeTimer() {
            App.AppTimer = new Timer(Constants.AppRefreshRate);
            App.AppTimer.Elapsed += OnAppTimerElapsed;
            App.AppTimer.Start();
        }

        /// Event handler for the elapsed event of the status refresh timer. 
        /// Refreshes application status.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Args containing event data.</param>
        internal static void OnAppTimerElapsed(object sender, ElapsedEventArgs args) {
            RefreshStatus();
        }

        /// <summary>
        /// Refreshes application status and Discord status.
        /// Sends Rich Presence to Discord if both Discord and Apple Music are detected.
        /// </summary>
        internal static void RefreshStatus() {
            App.DiscordIsOpen = Process.GetProcessesByName(Constants.DiscordAppName).Length > 0;
            App.AppleMusicIsOpen = Process.GetProcessesByName(Constants.AppleMusicAppName).Length > 0;

            RefreshPlayerStatus();
            RefreshStatusIcons();
            RefreshDiscordRichPresence();
        }

        /// <summary>
        /// Refreshes status of Apple Music Player.
        /// </summary>
        internal static void RefreshPlayerStatus() {
            try {
                if (!App.AppleMusicIsOpen) {
                    App.PlayerIsOpen = false;
                } else {
                    nint playerHandle = AppleMusicScraper.GetPlayerWindowHandle();
                    AutomationElement playerWindow = AppleMusicScraper.GetPlayerWindow(playerHandle);
                    App.PlayerIsOpen = playerWindow != null;
                }
            } catch (Exception exception) {
                Debug.WriteLine($"Exception in RefreshPlayerStatus: {exception}");
            }
        }

        /// <summary>
        /// Refreshes Discord status via Rich Presence.
        /// </summary>
        internal static async void RefreshDiscordRichPresence() {
            if (
                App.DiscordIsOpen &&
                App.AppleMusicIsOpen &&
                AppSettings.DisplayMusicStatusToggle
            ) {
                (
                    string songName, 
                    string songArtist, 
                    string songAlbum, 
                    int? songStart,
                    int? songEnd,
                    bool isPlaying
                ) = await AppleMusicScraper.Scrape();

                if (!isPlaying && !AppSettings.ShowStatusOnPauseToggle) {
                    DiscordRichPresence.Dispose();
                    return;
                }

                if (songName != App.CurrentSong) {
                    App.CurrentSong = songName;
                    App.CurrentAlbumArtwork = await ITunesAPI.GetAlbumArtworkUrl(songAlbum, songArtist);
                    App.CurrentSongUrl = await ITunesAPI.GetSongLink(songName, songArtist);
                }

                DiscordRichPresence.UpdatePresence(
                    details: App.CurrentSong ?? "",
                    state: $"by {songArtist} — {songAlbum}",
                    albumArtwork: App.CurrentAlbumArtwork,
                    songStart: songStart,
                    songEnd: songEnd,
                    songUrl: App.CurrentSongUrl,
                    isPlaying: isPlaying
                );
            }
        }

        /// <summary>
        /// Refreshes the status icons for Discord and Apple Music.
        /// </summary>
        internal static void RefreshStatusIcons() {
            MainWindow window = (App.Current as App)?.MainWindow as MainWindow;

            window.DispatcherQueue.TryEnqueue(() => {
                window.UpdateStatusIcons();
            });
        }

        /// <summary>
        /// Updates the status icons for Discord and Apple Music.
        /// Red X = Not Detected
        /// Green Checkmark = Detected
        /// </summary>
        /// <param name="DiscordStatusIcon">Discord status FontIcon.</param>
        /// <param name="AppleMusicStatusIcon">Apple Music status FontIcon.</param>
        internal static void UpdateStatusIcons(
            FontIcon DiscordStatusIcon,
            FontIcon AppleMusicStatusIcon
        ) {
            UpdateStatusIcon(DiscordStatusIcon, App.DiscordIsOpen);
            UpdateStatusIcon(AppleMusicStatusIcon, App.AppleMusicIsOpen);
        }

        /// <summary>
        /// Updates status FontIcon with either green checkmark or red x.
        /// </summary>
        /// <param name="fontIcon">Current status icon.</param>
        /// <param name="status">Current application status.</param>
        internal static void UpdateStatusIcon(FontIcon fontIcon, bool status) {
            if (status) {
                fontIcon.Glyph = Constants.CheckMark;
                fontIcon.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
            } else {
                fontIcon.Glyph = Constants.Cancel;
                fontIcon.Foreground = new SolidColorBrush(Microsoft.UI.Colors.IndianRed);
            }
        }
    }
}
