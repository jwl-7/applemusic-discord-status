using DiscordRPC;
using System;
using System.Diagnostics;


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
        /// <param name="details">The details with song name.</param>
        /// <param name="state">The state with artist/album.</param>
        /// <param name="albumArtwork">Apple Music song album artwork URL.</param>
        /// <param name="songEnd">Time left in the song.</param>
        /// <param name="songUrl">Apple Music song URL.</param>
        /// <param name="isPlaying">Whether or not the song is playing/paused.</param>
        internal static void UpdatePresence(
            string details,
            string state,
            string albumArtwork,
            string songEnd,
            string songUrl,
            bool isPlaying
        ) {
            if (!App.DiscordClientIsInitialized) {
                InitializeDiscordClient();
            }

            if (App.DiscordClientIsInitialized) {
                RichPresence presence = new() {
                    Details = details.PadRight(2, '\0'),
                    State = state[..Math.Min(state.Length, 256)],
                    Timestamps = isPlaying ? Timestamps.FromTimeSpan(ParseTimeRemaining(songEnd)) : null,
                    Assets = new Assets() {
                        LargeImageKey = albumArtwork ?? Constants.DiscordDefaultArtwork,
                        SmallImageKey = isPlaying ? Constants.DiscordPlayingIcon : Constants.DiscordPausedIcon,
                        SmallImageText = Constants.DiscordSmallImageText
                    },
                    Buttons = [
                        new() {
                            Label = Constants.DiscordButtonLabel,
                            Url = songUrl ?? Constants.AppleMusicUrl
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
        /// Parses time string representing time left in the song into seconds.
        /// </summary>
        /// <param name="timeString">Time string representing the remaining time of the song.</param>
        /// <returns>The total remaining seconds parsed from the time string.</returns>
        public static int ParseTimeRemaining(string timeString) {
            if (string.IsNullOrWhiteSpace(timeString)) {
                return 0;
            }

            // Remove the '-' at the beginning
            timeString = timeString.TrimStart('-');
            string[] timeParts = timeString.Split(':');
            int hours = 0, minutes = 0, seconds;

            try {
                if (timeParts.Length == 1) {
                    // Format: SS
                    if (!int.TryParse(timeParts[0], out seconds) || seconds < 0 || seconds > 59) {
                        throw new FormatException();
                    }
                } else if (timeParts.Length == 2) {
                    // Format: M:SS or :SS
                    if (timeParts[0] == string.Empty) {
                        // Format: :SS
                        if (!int.TryParse(timeParts[1], out seconds) || seconds < 0 || seconds > 59) {
                            throw new FormatException();
                        }
                    } else {
                        // Format: M:SS
                        if (!int.TryParse(timeParts[0], out minutes) || minutes < 0 || minutes > 59 ||
                            !int.TryParse(timeParts[1], out seconds) || seconds < 0 || seconds > 59) {
                            throw new FormatException();
                        }
                    }
                } else if (timeParts.Length == 3) {
                    // Format: H:MM:SS
                    if (!int.TryParse(timeParts[0], out hours) || hours < 0 ||
                        !int.TryParse(timeParts[1], out minutes) || minutes < 0 || minutes > 59 ||
                        !int.TryParse(timeParts[2], out seconds) || seconds < 0 || seconds > 59) {
                        throw new FormatException();
                    }
                } else {
                    throw new FormatException();
                }
            } catch (FormatException) {
                Debug.WriteLine("Invalid time string format: " + timeString);
                return 0;
            }

            int totalSeconds = hours * 3600 + minutes * 60 + seconds;
            Debug.WriteLine("Parsed time in seconds: " + totalSeconds);
            return totalSeconds;
        }

    }
}
