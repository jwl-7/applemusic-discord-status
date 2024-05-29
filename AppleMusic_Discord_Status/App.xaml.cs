using Microsoft.UI.Xaml;
using System.Timers;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Main application class.
    /// </summary>
    public partial class App : Application {
        internal Window m_window;
        internal Timer statusRefreshTimer;
        internal DiscordRichPresence DiscordBot { get; set; }
        internal string currentSong;
        internal string currentAlbumArtwork;
        internal string currentSongUrl;
        internal static bool DiscordIsOpen { get; set; }
        internal static bool AppleMusicIsOpen { get; set; }
        internal static bool MiniPlayerIsOpen { get; set; }

        /// <summary>
        /// Initializes application and AppManager instance.
        /// </summary>
        public App() {
            this.InitializeComponent();
            AppSettings.InitializeSettings();
            InitializeTimer();
            InitializeDiscordBot();
        }

        /// <summary>
        /// Activates main window on application start.
        /// </summary>
        /// <param name="args">Launch event args..</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args) {
            m_window = new MainWindow();
            m_window.Activate();
            m_window.Closed += this.OnAppExit;
        }

        /// <summary>
        /// Initializes refresh timer for updating Discord status and application states.
        /// </summary>
        private void InitializeTimer() {
            this.statusRefreshTimer = new Timer(Constants.AppRefreshRate);
            this.statusRefreshTimer.Elapsed += AppManager.OnStatusRefreshTimerElapsed;
            this.statusRefreshTimer.Start();
        }

        /// <summary>
        /// Initializes Discord RPC client.
        /// </summary>
        private void InitializeDiscordBot() {
            this.DiscordBot = new DiscordRichPresence();
        }

        /// <summary>
        /// Disposes the Discord client connection when the application is closed.
        /// This prevents memory leaks, probably.
        /// </summary>
        /// <param name="_">Sender.</param>
        /// <param name="__">Args.</param>
        private void OnAppExit(object _, WindowEventArgs __) {
            this.DiscordBot.Dispose();
        }
    }
}
