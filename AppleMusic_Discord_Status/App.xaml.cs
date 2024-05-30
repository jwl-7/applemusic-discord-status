using DiscordRPC;
using Microsoft.UI.Xaml;
using System.Timers;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Main application class.
    /// </summary>
    public partial class App : Application {
        internal Window MainWindow;
        internal static Timer AppTimer;
        internal static DiscordRpcClient DiscordClient;
        internal static string CurrentSong;
        internal static string CurrentAlbumArtwork;
        internal static string CurrentSongUrl;
        internal static bool DiscordClientIsInitialized;
        internal static bool DiscordIsOpen;
        internal static bool AppleMusicIsOpen;
        internal static bool MiniPlayerIsOpen;

        /// <summary>
        /// Initializes application and AppManager instance.
        /// </summary>
        public App() {
            this.InitializeComponent();
            AppSettings.InitializeSettings();
            AppManager.InitializeTimer();
            DiscordRichPresence.InitializeDiscordClient();
        }

        /// <summary>
        /// Activates main window on application start.
        /// </summary>
        /// <param name="args">Launch event args..</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args) {
            MainWindow = new MainWindow();
            MainWindow.Activate();
            MainWindow.Closed += this.OnAppExit;
        }

        /// <summary>
        /// Disposes the Discord client connection when the application is closed.
        /// This prevents memory leaks, probably.
        /// </summary>
        /// <param name="_">Sender.</param>
        /// <param name="__">Args.</param>
        private void OnAppExit(object _, WindowEventArgs __) {
            DiscordRichPresence.Dispose();
        }
    }
}
