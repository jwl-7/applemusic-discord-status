using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;


namespace AppleMusic_Discord_Status.Test {
    [TestClass]
    public class AppSettingsTests {

        [TestInitialize]
        public void SetUp() {
            if (File.Exists(Constants.AppDataSettingsPath)) {
                File.Delete(Constants.AppDataSettingsPath);
            }
            AppSettings.InitializeDefaultSettings();
        }

        [TestCleanup]
        public void TearDown() {
            if (File.Exists(Constants.AppDataSettingsPath)) {
                File.Delete(Constants.AppDataSettingsPath);
            }
        }

        [TestMethod]
        public void DisplayMusicStatusToggle_SettingAndGettingValue_WorksCorrectly() {
            AppSettings.DisplayMusicStatusToggle = true;
            bool result = AppSettings.DisplayMusicStatusToggle;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShowStatusOnPauseToggle_SettingAndGettingValue_WorksCorrectly() {
            AppSettings.ShowStatusOnPauseToggle = true;
            bool result = AppSettings.ShowStatusOnPauseToggle;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LaunchAtStartupToggle_SettingAndGettingValue_WorksCorrectly() {
            AppSettings.LaunchAtStartupToggle = true;
            bool result = AppSettings.LaunchAtStartupToggle;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void InitializeSettings_CreatesSettingsFile() {
            if (File.Exists(Constants.AppDataSettingsPath)) {
                File.Delete(Constants.AppDataSettingsPath);
            }

            AppSettings.InitializeSettings();

            Assert.IsTrue(File.Exists(Constants.AppDataSettingsPath));
        }

        [TestMethod]
        public void UpdateSettings_UpdatesSettingsFile() {
            AppSettings.SettingsObject settings = new() {
                DisplayMusicStatus = true,
                ShowStatusOnPause = true,
                LaunchAtStartup = true
            };

            AppSettings.UpdateSettings(settings);
            AppSettings.SettingsObject updatedSettings = AppSettings.GetSettingsObject();

            Assert.IsTrue(updatedSettings.DisplayMusicStatus);
            Assert.IsTrue(updatedSettings.ShowStatusOnPause);
            Assert.IsTrue(updatedSettings.LaunchAtStartup);
        }
    }
}
