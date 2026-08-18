using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using Windows.Graphics;

namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Main Window class.
    /// </summary>
    public sealed partial class MainWindow : Window {
        private bool _isExiting = false;

        /// <summary>
        /// Initializes main UI window.
        /// </summary>
        public MainWindow() {
            this.InitializeComponent();
            this.InitializeWindow();
            this.InitializeToggleSwitches();
            this.UpdateStatusIcons();
            this.Closed += MainWindow_Closed;
        }

        /// <summary>
        /// Displays the window and brings it to the foreground.
        /// </summary>
        [RelayCommand]
        private void ShowWindow() {
            this.AppWindow.Show();
            this.Activate();
        }

        /// <summary>
        /// Closes app window and disposes of tray icon.
        /// </summary>
        [RelayCommand]
        private void ExitApp() {
            Debug.WriteLine("DEBUG: Exit command fired!");
            _isExiting = true;
            TrayIcon.Dispose();
            this.Close();
        }

        /// <summary>
        /// Intercepts window close event to hide app to system tray.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainWindow_Closed(object sender, WindowEventArgs args) {
            if (!_isExiting) {
                args.Handled = true;
                this.AppWindow.Hide();
            }
        }

        /// <summary>
        /// Initializes application window with customizations.
        /// </summary>
        internal void InitializeWindow() {
            OverlappedPresenter appWindowPresenter = this.AppWindow.Presenter as OverlappedPresenter;

            appWindowPresenter.IsResizable = false;
            appWindowPresenter.IsMaximizable = false;

            this.AppWindow.Resize(new SizeInt32(Constants.AppWindowWidth, Constants.AppWindowHeight));
            this.AppWindow.SetIcon(Constants.AppIcon);
            this.Title = "Apple Music � Discord Status";
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
        internal void UpdateStatusIcons() {
            AppManager.UpdateStatusIcons(this.DiscordStatusIcon, this.AppleMusicStatusIcon);
        }
    }
}
