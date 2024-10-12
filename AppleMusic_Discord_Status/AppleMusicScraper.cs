using System;
using System.Windows.Automation;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Numerics;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Scrapes information from the Apple Music Player using UI Automation.
    /// 
    /// Note: Information should not be scraped from the Player in the main Apple Music window,
    /// because as soon as any context menu is opened in Apple Music, the Player gets sent away
    /// to some far off dimension, and it can no longer be found within the main window.
    /// </summary>
    internal class AppleMusicScraper {
        private static readonly (
            string, 
            string, 
            string,
            int?,
            int?, 
            bool
        ) DefaultReturn = (null, null, null, null, null, false);

        /// <summary>
        /// Scrapes information from the Apple Music Player.
        /// </summary>
        /// <returns>A tuple containing song name, artist, album, song start, song end, and playing status.</returns>
        public static async Task<(
            string songName, 
            string songArtist, 
            string songAlbum, 
            int? songStart, 
            int? songEnd, 
            bool isPlaying
        )> Scrape() {
            try {
                nint playerHandle = GetPlayerWindowHandle();
                if (playerHandle == IntPtr.Zero) return DefaultReturn;

                AutomationElement player = GetPlayerWindow(playerHandle);
                if (player == null) return DefaultReturn;

                AutomationElement bridge = GetBridgeElement(player);
                if (bridge == null) return DefaultReturn;

                AutomationElement inputSite = GetInputSiteElement(bridge);
                if (inputSite == null) return DefaultReturn;

                AutomationElement navView = GetNavViewElement(inputSite);
                if (navView == null) return DefaultReturn;

                AutomationElement transportBar = GetTransportBarElement(navView);
                if (transportBar == null) return DefaultReturn;

                AutomationElement lcd = GetLcdElement(transportBar);
                if (lcd == null) return DefaultReturn;

                (string songName, string songArtist, string songAlbum) = GetSongInfo(lcd);
                if (songName == null || songArtist == null || songAlbum == null) return DefaultReturn;

                string songStartTimeString = GetCurrentTime(lcd);
                string songEndTimeString = GetDuration(lcd);
                int? songStart = null;
                int? songEnd = null;

                if (!string.IsNullOrWhiteSpace(songStartTimeString) && !string.IsNullOrWhiteSpace(songEndTimeString)) {
                    songStart = ParseTimeString(songStartTimeString);
                    songEnd = ParseTimeString(songEndTimeString);
                } else {
                    // Calculate time info using slider progress and song duration
                    RangeValuePattern slider = GetSlider(lcd);

                    if (slider != null) {
                        int? songDuration = await ITunesAPI.GetSongDuration(songName, songArtist);

                        if (songDuration != null) {
                            double sliderProgress = slider.Current.Value / slider.Current.Maximum;
                            songStart = (int)(sliderProgress * songDuration);
                            songEnd = (int)((1 - sliderProgress) * songDuration);
                        }
                    }
                }

                bool isPlaying = IsPlaying(transportBar);

                return (songName, songArtist, songAlbum, songStart, songEnd, isPlaying);
            } catch (Exception ex) {
                Debug.WriteLine($"Scraping failed: {ex.Message}");
                return DefaultReturn;
            }
        }

        /// <summary>
        /// Finds the handle of the Apple Music Player window.
        /// </summary>
        /// <returns>The window handle.</returns>
        public static IntPtr GetPlayerWindowHandle() {
            AutomationElement player = AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "Apple Music"),
                    new PropertyCondition(AutomationElement.ClassNameProperty, "WinUIDesktopWin32WindowClass")
                )
            );

            return player != null
                ? new nint(player.Current.NativeWindowHandle)
                : IntPtr.Zero;
        }

        /// <summary>
        /// Finds the Apple Music Player window.
        /// </summary>
        /// <param name="playerWindowHandle">The window handle.</param>
        /// <returns>Player element.</returns>
        public static AutomationElement GetPlayerWindow(IntPtr playerWindowHandle) {
            return AutomationElement.FromHandle(playerWindowHandle);
        }

        /// <summary>
        /// Finds the bridge element within the Player.
        /// </summary>
        /// <param name="player">Player element.</param>
        /// <returns>Bridge element.</returns>
        public static AutomationElement GetBridgeElement(AutomationElement player) {
            return player.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "Microsoft.UI.Content.DesktopChildSiteBridge")
            );
        }

        /// <summary>
        /// Finds the input site element within the bridge.
        /// </summary>
        /// <param name="bridge">Bridge element.</param>
        /// <returns>Input Site Window element.</returns>
        public static AutomationElement GetInputSiteElement(AutomationElement bridge) {
            return bridge.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "InputSiteWindowClass")
            );
        }

        /// <summary>
        /// Finds the nav view element within the input site element.
        /// </summary>
        /// <param name="inputSite">Input Site element.</param>
        /// <returns>NavView element.</returns>
        public static AutomationElement GetNavViewElement(AutomationElement bridge) {
            return bridge.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "NavView")
            );
        }

        /// <summary>
        /// Finds the transport bar element within the nav view element.
        /// </summary>
        /// <param name="navView">NavView element.</param>
        /// <returns>TransportBar element.</returns>
        public static AutomationElement GetTransportBarElement(AutomationElement navView) {
            return navView.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "TransportBar")
            );
        }

        /// <summary>
        /// Finds the lcd element within the transport bar element.
        /// </summary>
        /// <param name="transportBar">TransportBar element.</param>
        /// <returns>LCD element.</returns>
        public static AutomationElement GetLcdElement(AutomationElement transportBar) {
            return transportBar.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "LCD")
            );
        }

        /// <summary>
        /// Finds the ScrollViewer children. These contain song information.
        /// </summary>
        /// <param name="lcd">LCD element.</param>
        /// <returns>ScrollViewer children elements.</returns>
        public static AutomationElementCollection GetScrollViewerChildren(AutomationElement lcd) {
            return lcd.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "ScrollViewer")
            );
        }

        /// <summary>
        /// Extracts the song information from ScrollViewer children.
        /// </summary>
        /// <param name="lcd">LCD element</param>
        /// <returns>A tuple with song name, song artist, and song album.</returns>
        public static (string songName, string songArtist, string songAlbum) GetSongInfo(AutomationElement lcd) {
            AutomationElementCollection scrollViewerChildren = GetScrollViewerChildren(lcd);
            AutomationElement firstChild = scrollViewerChildren[0];
            AutomationElement secondChild = scrollViewerChildren[1];

            string songName = firstChild.Current.Name;
            string[] songInfo = secondChild.Current.Name.Split(['—'], 2);
            string songArtist = songInfo[0].Trim();
            string songAlbum = songInfo[1].Trim();

            return (songName, songArtist, songAlbum);
        }

        /// <summary>
        /// Finds the current time (time elapsed) of the song.
        /// </summary>
        /// <param name="lcd">LCD element.</param>
        /// <returns>CurrentTime element.</returns>
        public static string GetCurrentTime(AutomationElement lcd) {
            AutomationElement currentTimeElement = lcd.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "CurrentTime")
            );
            return currentTimeElement?.Current.Name;
        }

        /// <summary>
        /// Finds the duration (time remaining) of the song.
        /// </summary>
        /// <param name="lcd">LCD element.</param>
        /// <returns>Duration element.</returns>
        public static string GetDuration(AutomationElement lcd) {
            AutomationElement durationElement = lcd.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "Duration")
            );
            return durationElement?.Current.Name;
        }

        /// <summary>
        /// Finds the slider (represents duration) element and returns its range values.
        /// </summary>
        /// <param name="lcd">LCD element.</param>
        /// <returns>Slider range values.</returns>
        public static RangeValuePattern GetSlider(AutomationElement lcd) {
            AutomationElement sliderElement = lcd.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "LCDScrubber")
            );
            return sliderElement?.GetCurrentPattern(RangeValuePattern.Pattern) as RangeValuePattern;
        }

        /// <summary>
        /// Finds the Play/Pause element.
        /// </summary>
        /// <param name="transportBar">TransportBar element.</param>
        /// <returns>Whether or not the song is playing/paused.</returns>
        public static bool IsPlaying(AutomationElement transportBar) {
            AutomationElement playPauseElement = transportBar.FindFirst(
                TreeScope.Children,
                new PropertyCondition(
                    AutomationElement.AutomationIdProperty, "TransportControl_PlayPauseStop"
                )
            );
            return playPauseElement.Current.Name == "Pause"; 
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
    }
}
