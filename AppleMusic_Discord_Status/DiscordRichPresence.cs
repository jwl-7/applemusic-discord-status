using DiscordRPC;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Provides functionality for managing Discord status via Rich Presence.
    /// </summary>
    internal class DiscordRichPresence {
        /// <summary>
        /// Initializes the Discord client and sets up event handlers.
        /// </summary>
        internal static void InitializeDiscordClient() {
            App.DiscordClient = new DiscordRpcClient(Constants.DiscordToken);

            App.DiscordClient.OnReady += (sender, e) => {
                Debug.WriteLine("Discord client is ready");
                App.DiscordClientIsInitialized = true;
            };

            App.DiscordClient.OnConnectionFailed += (sender, e) => {
                Debug.WriteLine("Discord connection failed");
                App.DiscordClientIsInitialized = false;
            };

            App.DiscordClient.OnError += (sender, e) => {
                Debug.WriteLine($"Discord error: {e.Message}");
                App.DiscordClientIsInitialized = false;
            };

            App.DiscordClient.OnClose += (sender, e) => {
                Debug.WriteLine("Discord connection closed");
                App.DiscordClientIsInitialized = false;
            };

            App.DiscordClient.Initialize();
        }

        /// <summary>
        /// Updates Discord Status via Rich Presence with Apple Music song info.
        /// </summary>
        internal static void UpdatePresence() {
            if (!App.DiscordClientIsInitialized) {
                InitializeDiscordClient();
            }

            if (App.DiscordClientIsInitialized) {
                RichPresence presence = new() {
                    Details = SanitizeText(App.CurrentTrack.Song ?? ""),
                    State = SanitizeText($"by {App.CurrentTrack.Artist} — {App.CurrentTrack.Album}"),
                    Timestamps = App.CurrentTrack.IsPlaying ? GetTimestamps(App.CurrentTrack.CurrentTime, App.CurrentTrack.RemainingTime) : null,
                    Assets = new Assets() {
                        LargeImageKey = SanitizeImageKey(App.CurrentTrack.ArtworkUrl, Constants.DiscordDefaultArtwork),
                        SmallImageKey = App.CurrentTrack.IsPlaying ? Constants.DiscordPlayingIcon : Constants.DiscordPausedIcon,
                        SmallImageText = Constants.DiscordSmallImageText
                    },
                    Type = ActivityType.Listening,
                    Buttons = [
                        new() {
                            Label = Constants.DiscordButtonLabel,
                            Url = App.CurrentTrack.SongUrl ?? Constants.AppleMusicUrl
                        }
                    ]
                };

                App.DiscordClient.SetPresence(presence);
            } else {
                Debug.WriteLine("Discord client is not initialized.");
            }
        }

        /// <summary>
        /// Disposes of the Discord RPC client and resets the initialization state.
        /// </summary>
        internal static void Dispose() {
            App.DiscordClientIsInitialized = false;
            App.DiscordClient?.Dispose();
        }

        /// <summary>
        /// Gets the timestamps for displaying the song time progress.
        /// </summary>
        /// <param name="currentTime">Start time (time elapsed) of the song.</param>
        /// <param name="remainingTime">End time (time left) of the song.</param>
        /// <returns></returns>
        internal static Timestamps? GetTimestamps(int? currentTime, int? remainingTime) {
            if (currentTime is null || remainingTime is null) return null;

            return new Timestamps() {
                Start = DateTime.UtcNow - new TimeSpan(0, 0, (int)currentTime),
                End = DateTime.UtcNow + new TimeSpan(0, 0, (int)remainingTime)
            };
        }

        /// <summary>
        /// Sanitizes text string for DiscordRPC.
        /// Max text = 128 characters.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>Formatted string.</returns>
        internal static string SanitizeText(string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                return "  ";
            }

            if (input.Length < 2) {
                return input.PadRight(2, ' ');
            }

            if (input.Length <= Constants.DiscordMaxTextLength) {
                return input;
            }

            int targetLength = Constants.DiscordMaxTextLength - Constants.Ellipsis.Length;
            return input[..targetLength] + Constants.Ellipsis;
        }

        /// <summary>
        /// Sanitizes image string for DiscordRPC.
        /// Max image string = 256 characters
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>Formatted string.</returns>
        internal static string SanitizeImageKey(string? input, string defaultImage) {
            if (string.IsNullOrWhiteSpace(input)) {
                return defaultImage;
            }

            if (input.Length > Constants.DiscordMaxKeyLength) {
                return defaultImage;
            }

            return input;
        }
    }
}
