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

        [RelayCommand]
        private async System.Threading.Tasks.Task CheckForUpdatesAsync() {
            try {
                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppleMusicDiscordStatus", "1.0.6"));

                string url = "https://api.github.com/repos/jwl-7/applemusic-discord-status/releases/latest";
                string json = await client.GetStringAsync(url);

                using var doc = System.Text.Json.JsonDocument.Parse(json);
                string? latestTag = doc.RootElement.GetProperty("tag_name").GetString();
                string? currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);

                bool isUpToDate = latestTag == null || latestTag.TrimStart('v') == currentVersion;

                var outerPanel = new Microsoft.UI.Xaml.Controls.StackPanel {
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    Spacing = 12
                };

                outerPanel.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock {
                    Text = isUpToDate ? "Up to Date" : "Update Available",
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    FontSize = 20,
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center
                });

                var grid = new Microsoft.UI.Xaml.Controls.Grid {
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    RowSpacing = 6,
                    ColumnSpacing = 8
                };
                grid.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(40, Microsoft.UI.Xaml.GridUnitType.Pixel) });
                grid.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Auto) });

                int row = 0;

                if (!isUpToDate) {
                    grid.RowDefinitions.Add(new Microsoft.UI.Xaml.Controls.RowDefinition());

                    var latestLabel = new Microsoft.UI.Xaml.Controls.TextBlock {
                        Text = "Latest:",
                        FontSize = 16,
                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right
                    };
                    Microsoft.UI.Xaml.Controls.Grid.SetRow(latestLabel, row);
                    Microsoft.UI.Xaml.Controls.Grid.SetColumn(latestLabel, 0);
                    grid.Children.Add(latestLabel);

                    var latestValue = new Microsoft.UI.Xaml.Controls.TextBlock {
                        Text = latestTag ?? "Unknown",
                        FontSize = 16,
                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left
                    };
                    Microsoft.UI.Xaml.Controls.Grid.SetRow(latestValue, row);
                    Microsoft.UI.Xaml.Controls.Grid.SetColumn(latestValue, 1);
                    grid.Children.Add(latestValue);

                    row++;
                }

                grid.RowDefinitions.Add(new Microsoft.UI.Xaml.Controls.RowDefinition());

                var versionLabel = new Microsoft.UI.Xaml.Controls.TextBlock {
                    Text = "Version:",
                    FontSize = 16,
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right
                };
                Microsoft.UI.Xaml.Controls.Grid.SetRow(versionLabel, row);
                Microsoft.UI.Xaml.Controls.Grid.SetColumn(versionLabel, 0);
                grid.Children.Add(versionLabel);

                var versionValue = new Microsoft.UI.Xaml.Controls.TextBlock {
                    Text = $"{currentVersion ?? "Unknown"} {(isUpToDate ? "✅" : "❌")}",
                    FontSize = 16,
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left
                };
                Microsoft.UI.Xaml.Controls.Grid.SetRow(versionValue, row);
                Microsoft.UI.Xaml.Controls.Grid.SetColumn(versionValue, 1);
                grid.Children.Add(versionValue);

                outerPanel.Children.Add(grid);

                ContentDialog dialog = new ContentDialog {
                    Content = outerPanel,
                    CloseButtonText = "Close",
                    PrimaryButtonText = isUpToDate ? "" : "Download",
                    XamlRoot = this.Content.XamlRoot
                };

                ContentDialogResult result = await dialog.ShowAsync();

                if (!isUpToDate && result == ContentDialogResult.Primary) {
                    Process.Start(new ProcessStartInfo {
                        FileName = "https://github.com/jwl-7/applemusic-discord-status/releases/latest",
                        UseShellExecute = true
                    });
                }
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