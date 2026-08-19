using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppleMusic_Discord_Status {
    public sealed partial class UpdateDialog : ContentDialog {
        public string StatusTitle { get; private set; } = "Checking...";
        public string LatestTag { get; private set; } = string.Empty;
        public string CurrentVersionDisplay { get; private set; } = string.Empty;
        public new string PrimaryButtonText { get; private set; } = string.Empty;
        public Visibility ShowLatest { get; private set; } = Visibility.Collapsed;

        public string StatusGlyph { get; private set; } = Constants.Cancel;
        public Brush StatusBrush { get; private set; } = new SolidColorBrush(Microsoft.UI.Colors.IndianRed);

        private bool _isUpToDate = true;

        /// <summary>
        /// Initializes new instance of UpdateDialog.
        /// </summary>
        public UpdateDialog() {
            this.InitializeComponent();
            _ = CheckVersionAsync();
        }

        /// <summary>
        /// Fetches latest release version from Github and compares to local version.
        /// </summary>
        /// <returns>The async task.</returns>
        private async Task CheckVersionAsync() {
            try {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppleMusicDiscordStatus", "1.0.0"));

                string json = await client.GetStringAsync(Constants.GithubReleaseUrl);

                using var doc = JsonDocument.Parse(json);
                LatestTag = doc.RootElement.GetProperty("tag_name").GetString() ?? "Unknown";
                string? currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);

                _isUpToDate = LatestTag == null || LatestTag.TrimStart('v') == currentVersion;

                StatusTitle = _isUpToDate ? "Current" : "Outdated";
                CurrentVersionDisplay = currentVersion ?? "Unknown";
                PrimaryButtonText = _isUpToDate ? string.Empty : "Download";
                ShowLatest = _isUpToDate ? Visibility.Collapsed : Visibility.Visible;
                StatusGlyph = _isUpToDate ? Constants.CheckMark : Constants.Cancel;
                StatusBrush = _isUpToDate
                    ? new SolidColorBrush(Microsoft.UI.Colors.Green)
                    : new SolidColorBrush(Microsoft.UI.Colors.IndianRed);

                // Force layout update if bindings don't auto-refresh post-constructor load
                this.Bindings.Update();
            } catch {
                StatusTitle = "Check Failed";
                CurrentVersionDisplay = "Unknown";
            }
        }

        /// <summary>
        /// Opens local browser to latest release Github page. 
        /// </summary>
        [RelayCommand]
        private void Download() {
            if (!_isUpToDate) {
                Process.Start(new ProcessStartInfo {
                    FileName = Constants.GithubReleaseUrl,
                    UseShellExecute = true
                });
            }
        }
    }
}