using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;

namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Main Window class.
    /// </summary>
    public sealed partial class MainWindow : Window {
        private bool _isExitingWindow = false;
        private bool _isInitialStartup = true;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Initializes main UI window.
        /// </summary>
        public MainWindow() {
            this.InitializeComponent();
            this.InitializeWindow();
            this.InitializeToggleSwitches();
            this.UpdateStatusIcons();
            this.Activated += MainWindow_Activated;
            this.Closed += MainWindow_Closed;
        }

        /// <summary>
        /// Initializes application window with customizations.
        /// </summary>
        internal void InitializeWindow() {
            OverlappedPresenter appWindowPresenter = (OverlappedPresenter)this.AppWindow.Presenter;

            appWindowPresenter.IsResizable = false;
            appWindowPresenter.IsMaximizable = false;

            this.AppWindow.Resize(new SizeInt32(Constants.AppWindowWidth, Constants.AppWindowHeight));
            this.AppWindow.SetIcon(Constants.AppIcon);
            this.Title = "Apple Music — Discord Status";
            this.ExtendsContentIntoTitleBar = true;

            this.EnableLayeredWindow();
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
        /// Handles initial windows activation event to hide app to system tray on initial startup.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="args">Event data.</param>
        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args) {
            if (_isInitialStartup) {
                _isInitialStartup = false;
                bool isStartup = Environment.GetCommandLineArgs().Contains("--startup");
                if (isStartup) this.AppWindow.Hide();
            }
        }

        /// <summary>
        /// Intercepts window close event to hide app to system tray.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="args">Event data.</param>
        private void MainWindow_Closed(object sender, WindowEventArgs args) {
            if (!_isExitingWindow) {
                args.Handled = true;
                this.AppWindow.Hide();
            }
        }

        /// <summary>
        /// Enables Win32 extended style layered window.
        /// This helps with preventing window flashing when starting app hidden in the system tray.
        /// </summary>
        private void EnableLayeredWindow() {
            IntPtr windowHandle = WindowNative.GetWindowHandle(this);
            int exStyle = GetWindowLong(windowHandle, Constants.WinExStyle);
            int updatedStyle = exStyle | Constants.WinExStyleLayered;
            _ = SetWindowLong(windowHandle, Constants.WinExStyle, updatedStyle);
        }

        /// <summary>
        /// Displays the window and brings it to the foreground.
        /// </summary>
        [RelayCommand]
        private void ShowWindow() {
            Debug.WriteLine("Showing Window");
            this.AppWindow.Show();
            this.Activate();
        }

        /// <summary>
        /// Closes app window and disposes of tray icon.
        /// </summary>
        [RelayCommand]
        private void ExitApp() {
            Debug.WriteLine("Exiting Window");
            _isExitingWindow = true;
            TrayIcon.Dispose();
            this.Close();
        }

        /// <summary>
        /// Checks for updates and opens the update dialog.
        /// </summary>
        /// <returns>The async task.</returns>
        [RelayCommand]
        private async System.Threading.Tasks.Task CheckForUpdatesAsync() {
            try {
                var dialog = new UpdateDialog {
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            } catch {
                // Fail silently
            }
        }

        /// <summary>
        /// Updates application status icons.
        /// </summary>
        internal void UpdateStatusIcons() {
            AppManager.UpdateStatusIcons(this.DiscordStatusIcon, this.AppleMusicStatusIcon);
        }
    }
}