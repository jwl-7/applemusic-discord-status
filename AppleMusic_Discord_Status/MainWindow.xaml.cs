using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Diagnostics;
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
            this.AppWindow.SetIcon(Constants.AppIcon);
            this.Title = "Apple Music — Discord Status";
            this.ExtendsContentIntoTitleBar = true;
        }

        /// <summary>
        /// Initializes toggle switches with saved states.
        /// </summary>
        private void InitializeToggleSwitches() {
            this.DisplayMusicToggleSwitch.IsOn = AppSettings.DisplayMusicStatusToggle;
            this.ShowStatusOnPauseToggleSwitch.IsOn = AppSettings.ShowStatusOnPauseToggle;
            this.LaunchAtStartupToggleSwitch.IsOn = AppSettings.LaunchAtStartupToggle;

            this.DisplayMusicToggleSwitch.Toggled += (sender, args) => {
                AppSettings.DisplayMusicStatusToggle = this.DisplayMusicToggleSwitch.IsOn;
            };
            this.ShowStatusOnPauseToggleSwitch.Toggled += (sender, args) => {
                AppSettings.ShowStatusOnPauseToggle = this.ShowStatusOnPauseToggleSwitch.IsOn;
            };
            this.LaunchAtStartupToggleSwitch.Toggled += (sender, args) => {
                AppSettings.LaunchAtStartupToggle = this.LaunchAtStartupToggleSwitch.IsOn;
            };
        }

        /// <summary>
        /// Updates application status icons.
        /// </summary>
        public void UpdateStatusIcons() {
            AppManager.UpdateStatusIcons(this.DiscordStatusIcon, this.AppleMusicStatusIcon, this.MiniPlayerStatusIcon);
        }
    }
}
