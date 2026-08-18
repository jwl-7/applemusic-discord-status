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
                AppleMusicMetadata metadata = await AppleMusicScraper.Scrape();

                if (metadata is null) {
                    DiscordRichPresence.Dispose();
                    return;
                }

                if (!metadata.IsPlaying && !AppSettings.ShowStatusOnPauseToggle) {
                    DiscordRichPresence.Dispose();
                    return;
                }

                if (metadata.Song != App.CurrentTrack.Song) {
                    ITunesMetadata metadataApi = await ITunesAPI.GetTrackMetadata(metadata.Song, metadata.Artist, metadata.Album);
                    App.CurrentTrack.Duration = metadataApi?.Duration;
                    App.CurrentTrack.SongUrl = metadataApi?.SongUrl;
                    App.CurrentTrack.AlbumUrl = metadataApi?.AlbumUrl;
                    App.CurrentTrack.ArtworkUrl = metadataApi?.ArtworkUrl;
                }

                App.CurrentTrack.Song = metadata.Song;
                App.CurrentTrack.Artist = metadata.Artist;
                App.CurrentTrack.Album = metadata.Album;
                App.CurrentTrack.CurrentTime = metadata.CurrentTime;
                App.CurrentTrack.RemainingTime = metadata.RemainingTime;
                App.CurrentTrack.IsPlaying = metadata.IsPlaying;

                DiscordRichPresence.UpdatePresence();
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
