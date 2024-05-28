using System;
using System.Diagnostics;
using System.IO;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Application constants. 
    /// </summary>
    internal class Constants {
        public const int AppWindowHeight = 400;
        public const int AppWindowWidth = 500;
        public const int AppRefreshRate = 5000;
        public const string AppleMusicAppName = "AppleMusic";
        public const string AppShortcutName = "AppleMusic_Discord_Status.lnk";
        public const string DiscordAppName = "Discord";
        public const string DiscordButtonLabel = "Listen on Apple Music";
        public const string DiscordDefaultArtwork = "no_artwork";
        public const string DiscordPausedIcon = "apple_music_pause_icon";
        public const string DiscordPlayingIcon = "apple_music_icon";
        public const string DiscordSmallImageText = "Apple Music";
        public const string DiscordToken = "1240579407635157042";
        public const string ITunesApiUrl = "https://itunes.apple.com/search?term=";
        public const string WindowsScriptHostShellObjectGUID = "72C24DD5-D70A-438B-8A42-98424B88AFB8";
        public static string AppExePath = Process.GetCurrentProcess().MainModule?.FileName;
        public static string AppShortcutPath = Path.Join(WindowsStartupFolder, AppShortcutName);
        public static string WindowsStartupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    }
}
