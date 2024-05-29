using DiscordRPC;
using System;
using System.Diagnostics;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Provides functionality for managing Discord status via Rich Presence.
    /// </summary>
    internal class DiscordRichPresence {
        internal DiscordRpcClient client;
        internal bool isInitialized;

        /// <summary>
        /// Initializes a new client connection to Discord.
        /// </summary>
        public DiscordRichPresence() {
            InitializeClient();
        }

        /// <summary>
        /// Initializes the Discord RPC client and sets up event handlers.
        /// </summary>
        internal void InitializeClient() {
            this.client = new(Constants.DiscordToken);

            this.client.OnReady += (sender, e) => {
                Debug.WriteLine("Discord client is ready");
                this.isInitialized = true;
            };

            this.client.OnConnectionFailed += (sender, e) => {
                Debug.WriteLine("Discord connection failed");
                this.isInitialized = false;
            };

            this.client.OnError += (sender, e) => {
                Debug.WriteLine($"Discord error: {e.Message}");
                this.isInitialized = false;
            };

            this.client.OnClose += (sender, e) => {
                Debug.WriteLine("Discord connection closed");
                this.isInitialized = false;
            };

            this.client.Initialize();
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
        public void UpdatePresence(
            string details,
            string state,
            string albumArtwork,
            string songEnd,
            string songUrl,
            bool isPlaying
        ) {
            if (!this.isInitialized) {
                this.InitializeClient();
            }

            if (this.isInitialized) {
                RichPresence presence = new() {
                    Details = details.PadRight(2, '\0'),
                    State = state,
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

                this.client.SetPresence(presence);
            } else {
                Debug.WriteLine("Discord client is not initialized.");
            }
        }

        /// <summary>
        /// Disposes of the Discord RPC client and resets the initialization state.
        /// </summary>
        public void Dispose() {
            this.isInitialized = false;
            this.client?.Dispose();
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
