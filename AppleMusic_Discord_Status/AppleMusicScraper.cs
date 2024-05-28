using System;
using System.Windows.Automation;
using System.Diagnostics;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Scrapes information from the Apple Music Mini Player using UI Automation.
    /// 
    /// Note: Information should not be scraped from the Player in the main Apple Music window,
    /// because as soon as any context menu is opened in Apple Music, the Player gets sent away
    /// to some far off dimension, and it can no longer be found within the main window.
    /// </summary>
    internal class AppleMusicScraper {
        /// <summary>
        /// Scrapes information from the Apple Music MiniPlayer.
        /// </summary>
        /// <returns>A tuple containing song name, artist, album, time left, and playing status.</returns>
        public static (
            string songName, 
            string songArtist, 
            string songAlbum, 
            string timeLeft,
            bool isPlaying
        ) Scrape() {
            try {
                nint miniPlayerHandle = GetMiniPlayerWindowHandle();
                if (miniPlayerHandle == IntPtr.Zero) return (null, null, null, null, false);

                AutomationElement miniPlayer = GetMiniPlayerWindow(miniPlayerHandle);
                if (miniPlayer == null) return (null, null, null, null, false);

                AutomationElement bridge = GetBridgeElement(miniPlayer);
                if (bridge == null) return (null, null, null, null, false);

                (string songName, string songArtist, string songAlbum) = GetSongInfo(bridge);
                if (songName == null || songArtist == null || songAlbum == null) return (null, null, null, null, false);

                string timeLeft = GetDuration(bridge);
                if (timeLeft == null) return (null, null, null, null, false);

                bool isPlaying = IsPlaying(bridge);

                return (songName, songArtist, songAlbum, timeLeft, isPlaying);
            } catch (Exception ex) {
                Debug.WriteLine($"Scraping failed: {ex.Message}");
                return (null, null, null, null, false);
            }
        }

        /// <summary>
        /// Finds the handle of the Apple Music Mini Player window.
        /// </summary>
        /// <returns>The window handle.</returns>
        public static IntPtr GetMiniPlayerWindowHandle() {
            AutomationElement miniPlayer = AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "MiniPlayer"),
                    new PropertyCondition(AutomationElement.ClassNameProperty, "WinUIDesktopWin32WindowClass")
                )
            );

            return miniPlayer != null
                ? new nint(miniPlayer.Current.NativeWindowHandle)
                : IntPtr.Zero;
        }

        /// <summary>
        /// Finds the Apple Music Mini Player window.
        /// </summary>
        /// <param name="miniPlayerWindowHandle">The window handle.</param>
        /// <returns>Mini Player element.</returns>
        public static AutomationElement GetMiniPlayerWindow(IntPtr miniPlayerWindowHandle) {
            return AutomationElement.FromHandle(miniPlayerWindowHandle);
        }

        /// <summary>
        /// Finds the bridge element within the Mini Player.
        /// </summary>
        /// <param name="miniPlayer">Mini Player element.</param>
        /// <returns>Bridge element.</returns>
        public static AutomationElement GetBridgeElement(AutomationElement miniPlayer) {
            return miniPlayer.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "Microsoft.UI.Content.DesktopChildSiteBridge")
            );
        }

        /// <summary>
        /// Finds the ScrollViewer children. These contain song information.
        /// </summary>
        /// <param name="miniPlayer">Mini Player element.</param>
        /// <returns>ScrollViewer children elements.</returns>
        public static AutomationElementCollection GetScrollViewerChildren(AutomationElement bridge) {
            return bridge.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "ScrollViewer")
            );
        }

        /// <summary>
        /// Extracts the song information from ScrollViewer children.
        /// </summary>
        /// <param name="bridge">Bridge element</param>
        /// <returns>A tuple with song name, song artist, and song album.</returns>
        public static (string songName, string songArtist, string songAlbum) GetSongInfo(AutomationElement bridge) {
            AutomationElementCollection scrollViewerChildren = GetScrollViewerChildren(bridge);
            AutomationElement firstChild = scrollViewerChildren[0];
            AutomationElement secondChild = scrollViewerChildren[1];

            string songName = firstChild.Current.Name;
            string[] songInfo = secondChild.Current.Name.Split(['—'], 2);
            string songArtist = songInfo[0].Trim();
            string songAlbum = songInfo[1].Trim();

            return (songName, songArtist, songAlbum);
        }

        /// <summary>
        /// Finds the duration of the song, which is the time remaining in the song.
        /// </summary>
        /// <param name="bridge">Bridge element.</param>
        /// <returns>Duration element.</returns>
        public static string GetDuration(AutomationElement bridge) {
            AutomationElement durationElement = bridge.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "Duration")
            );
            return durationElement.Current.Name;
        }

        /// <summary>
        /// Finds the Play/Pause element.
        /// </summary>
        /// <param name="bridge">Bridge element.</param>
        /// <returns>Whether or not the song is playing/paused.</returns>
        public static bool IsPlaying(AutomationElement bridge) {
            AutomationElement playPauseElement = bridge.FindFirst(
                TreeScope.Children,
                new PropertyCondition(
                    AutomationElement.AutomationIdProperty, "TransportControl_PlayPauseStop"
                )
            );
            return playPauseElement.Current.Name == "Pause"; 
        }
    }
}
