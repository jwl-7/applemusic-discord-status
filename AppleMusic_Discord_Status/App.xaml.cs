using Microsoft.UI.Xaml;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Main application class.
    /// </summary>
    public partial class App : Application {
        internal Window m_window;
        internal static AppManager AppManager { get; set; }

        /// <summary>
        /// Initializes application and AppManager instance.
        /// </summary>
        public App() {
            this.InitializeComponent();
            InitializeAppManager();
        }

        /// <summary>
        /// Initializes AppManager instance. 
        /// This function is totally necessary, so that the constructor is filled with the word "initialize."
        /// </summary>
        private static void InitializeAppManager() {
            AppManager = new AppManager();
        }

        /// <summary>
        /// Activates main window on application start.
        /// </summary>
        /// <param name="args">Launch event args..</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args) {
            m_window = new MainWindow();
            m_window.Activate();
            m_window.Closed += AppManager.OnAppExit;
        }
    }
}
