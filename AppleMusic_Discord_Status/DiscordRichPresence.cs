using DiscordRPC;
using System;
using System.Diagnostics;
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
        /// <param name="details">The details with song name.</param>
        /// <param name="state">The state with artist/album.</param>
        /// <param name="albumArtwork">Apple Music song album artwork URL.</param>
        /// <param name="songStart">Time elapsed in the song.</param>
        /// <param name="songEnd">Time left in the song.</param>
        /// <param name="songUrl">Apple Music song URL.</param>
        /// <param name="isPlaying">Whether or not the song is playing/paused.</param>
        internal static void UpdatePresence(
            string details,
            string state,
            string albumArtwork,
            int? songStart,
            int? songEnd,
            string songUrl,
            bool isPlaying
        ) {
            if (!App.DiscordClientIsInitialized) {
                InitializeDiscordClient();
            }

            if (App.DiscordClientIsInitialized) {
                Debug.WriteLine(state);
                RichPresence presence = new() {
                    Details = details.PadRight(2, '\0'),
                    State = Truncate(state),
                    Timestamps = isPlaying ? GetTimestamps(songStart, songEnd) : null,
                    Assets = new Assets() {
                        LargeImageKey = albumArtwork ?? Constants.DiscordDefaultArtwork,
                        SmallImageKey = isPlaying ? Constants.DiscordPlayingIcon : Constants.DiscordPausedIcon,
                        SmallImageText = Constants.DiscordSmallImageText
                    },
                    Type = ActivityType.Listening,
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
        /// Gets the timestamps for displaying the song time progress.
        /// </summary>
        /// <param name="songStart">Start time (time elapsed) of the song.</param>
        /// <param name="songEnd">End time (time left) of the song.</param>
        /// <returns></returns>
        public static Timestamps GetTimestamps(int? songStart, int? songEnd) {
            if (songStart == null || songEnd == null) return null;

            return new Timestamps() {
                Start = DateTime.UtcNow - new TimeSpan(0, 0, (int)songStart),
                End = DateTime.UtcNow + new TimeSpan(0, 0, (int)songEnd)
            };
        }

        /// <summary>
        /// Parses time string scraped from Apple Music into seconds.
        /// </summary>
        /// <param name="timeString">Time string in (-)H:MM:SS format.</param>
        /// <returns>Time in seconds.</returns>
        public static int ParseTimeString(string timeString) {
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

        /// <summary>
        /// Truncates string to 128 bytes.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>Truncated string.</returns>
        public static string Truncate(string input) {
            Encoding utf8 = Encoding.UTF8;
            byte[] inputBytes = utf8.GetBytes(input);
            string ellipsis = "…";
            int maxBytes = 128;
            int ellipsisBytes = utf8.GetByteCount(ellipsis);
            int length = 0;

            if (inputBytes.Length <= maxBytes)
                return input;

            while (utf8.GetByteCount(input[..++length]) <= (maxBytes - ellipsisBytes)) ;

            return input[..(length - 1)] + ellipsis;
        }
    }
}
