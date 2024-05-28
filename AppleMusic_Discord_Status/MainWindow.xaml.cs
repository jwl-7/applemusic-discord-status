using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Main Window class.
    /// </summary>
    public sealed partial class MainWindow : Window {
        /// <summary>
        /// Initializes main UI window.
        /// </summary>
        public MainWindow() {
            this.InitializeComponent();
            this.InitializeWindow();
            this.InitializeToggleSwitches();
            this.UpdateStatusIcons();
        }

        /// <summary>
        /// Initializes application window with customizations.
        /// </summary>
        public void InitializeWindow() {
            OverlappedPresenter appWindowPresenter = this.AppWindow.Presenter as OverlappedPresenter;

            appWindowPresenter.IsResizable = false;
            appWindowPresenter.IsMaximizable = false;

            this.AppWindow.Resize(new SizeInt32(Constants.AppWindowWidth, Constants.AppWindowHeight));
            this.ExtendsContentIntoTitleBar = true;
        }

        /// <summary>
        /// Initializes toggle switches with saved states.
        /// </summary>
        private void InitializeToggleSwitches() {
            this.DisplayMusicToggleSwitch.IsOn = AppManager.DisplayMusicStatusToggleState;
            this.ShowStatusOnPauseToggleSwitch.IsOn = AppManager.ShowStatusOnPauseToggleState;
            this.LaunchAtStartupToggleSwitch.IsOn = AppManager.LaunchAtStartupToggleState;

            this.DisplayMusicToggleSwitch.Toggled += (sender, args) => {
                AppManager.DisplayMusicStatusToggleState = this.DisplayMusicToggleSwitch.IsOn;
            };
            this.ShowStatusOnPauseToggleSwitch.Toggled += (sender, args) => {
                AppManager.ShowStatusOnPauseToggleState = this.ShowStatusOnPauseToggleSwitch.IsOn;
            };
            this.LaunchAtStartupToggleSwitch.Toggled += (sender, args) => {
                AppManager.LaunchAtStartupToggleState = this.LaunchAtStartupToggleSwitch.IsOn;
            };
        }

        /// <summary>
        /// Updates application status icons.
        /// </summary>
        public void UpdateStatusIcons() {
            App.AppManager.UpdateStatusIcons(this.DiscordStatusIcon, this.AppleMusicStatusIcon, this.MiniPlayerStatusIcon);
        }
    }
}
