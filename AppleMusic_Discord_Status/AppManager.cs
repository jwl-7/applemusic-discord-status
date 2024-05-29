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
        /// Event handler for the elapsed event of the status refresh timer. 
        /// Refreshes application status.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Args containing event data.</param>
        internal static void OnStatusRefreshTimerElapsed(object sender, ElapsedEventArgs args) {
            RefreshStatus(App.Current as App);
        }

        /// <summary>
        /// Refreshes application status and Discord status.
        /// Sends Rich Presence to Discord if Discord, Apple Music, and Apple Music Mini Player are all detected.
        /// </summary>
        internal static async void RefreshStatus(App app) {
            App.DiscordIsOpen = Process.GetProcessesByName(Constants.DiscordAppName).Length > 0;
            App.AppleMusicIsOpen = Process.GetProcessesByName(Constants.AppleMusicAppName).Length > 0;
            RefreshMiniPlayerStatus();
            MainWindow window = app.m_window as MainWindow;

            window.DispatcherQueue.TryEnqueue(() => {
                window.UpdateStatusIcons();
            });

            if (
                App.DiscordIsOpen &&
                App.AppleMusicIsOpen &&
                App.MiniPlayerIsOpen &&
                AppSettings.DisplayMusicStatusToggle
            ) {
                (string songName, string songArtist, string songAlbum, string timeLeft, bool isPlaying) = AppleMusicScraper.Scrape();

                if (!isPlaying && !AppSettings.ShowStatusOnPauseToggle) {
                    app.DiscordBot.Dispose();
                    return;
                }

                if (songName != app.currentSong) {
                    app.currentSong = songName;
                    app.currentAlbumArtwork = await ITunesAPI.GetAlbumArtworkUrl(songAlbum, songArtist);
                    app.currentSongUrl = await ITunesAPI.GetSongLink(songName, songArtist);
                }

                app.DiscordBot.UpdatePresence(
                    details: app.currentSong ?? "",
                    state: $"by {songArtist} — {songAlbum}",
                    albumArtwork: app.currentAlbumArtwork,
                    songEnd: timeLeft,
                    songUrl: app.currentSongUrl,
                    isPlaying: isPlaying
                );
            }
        }

        /// <summary>
        /// Refreshes status of Apple Music Mini Player.
        /// </summary>
        public static void RefreshMiniPlayerStatus() {
            try {
                if (!App.AppleMusicIsOpen) {
                    App.MiniPlayerIsOpen = false;
                } else {
                    nint miniPlayerHandle = AppleMusicScraper.GetMiniPlayerWindowHandle();
                    AutomationElement miniPlayerWindow = AppleMusicScraper.GetMiniPlayerWindow(miniPlayerHandle);
                    App.MiniPlayerIsOpen = miniPlayerWindow != null;
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
        public static void UpdateStatusIcons(
            FontIcon DiscordStatusIcon,
            FontIcon AppleMusicStatusIcon,
            FontIcon MiniPlayerStatusIcon
        ) {
            UpdateStatusIcon(DiscordStatusIcon, App.DiscordIsOpen);
            UpdateStatusIcon(AppleMusicStatusIcon, App.AppleMusicIsOpen);
            UpdateStatusIcon(MiniPlayerStatusIcon, App.MiniPlayerIsOpen);
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
    }
}
