using System;
using System.Diagnostics;
using System.IO;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Application constants. 
    /// </summary>
    internal class Constants {
        internal const int AppWindowHeight = 400;
        internal const int AppWindowWidth = 500;
        internal const int AppRefreshRate = 5000;
        internal const string AppIcon = "ms-appx:///Assets/Apple_Music_icon.ico";
        internal const string AppName = "AppleMusic_Discord_Status";
        internal const string AppSettingsName = "settings.json";
        internal const string AppShortcutName = "AppleMusic_Discord_Status.lnk";
        internal const string AppleMusicAppName = "AppleMusic";
        internal const string AppleMusicUrl = "https://music.apple.com/";
        internal const string Cancel = "\uE711";
        internal const string CheckMark = "\uE73E";
        internal const string DiscordAppName = "Discord";
        internal const string DiscordButtonLabel = "Listen on Apple Music";
        internal const string DiscordDefaultArtwork = "no_artwork";
        internal const string DiscordPausedIcon = "apple_music_pause_icon";
        internal const string DiscordPlayingIcon = "apple_music_icon";
        internal const string DiscordSmallImageText = "Apple Music";
        internal const string DiscordToken = "1240579407635157042";
        internal const string ITunesApiUrl = "https://itunes.apple.com/search?term=";
        internal const string WindowsScriptHostShellObjectGUID = "72C24DD5-D70A-438B-8A42-98424B88AFB8";
        internal static string AppExePath = Process.GetCurrentProcess().MainModule?.FileName;
        internal static string WindowsAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        internal static string WindowsStartupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        internal static string AppDataSettingsFolder = Path.Join(WindowsAppDataFolder, AppName);
        internal static string AppDataSettingsPath = Path.Join(AppDataSettingsFolder, AppSettingsName);
        internal static string AppShortcutPath = Path.Join(WindowsStartupFolder, AppShortcutName);
        internal static AppSettings.SettingsObject AppDefaultSettings = new() {
            DisplayMusicStatus = false,
            ShowStatusOnPause = false,
            LaunchAtStartup = false
        };
    }
}
