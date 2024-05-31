using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using static AppleMusic_Discord_Status.AppSettings;


namespace AppleMusic_Discord_Status.Test {
    [TestClass]
    public class AppSettingsTests {

        [TestInitialize]
        public void TestInitialize() {
            Cleanup();
        }

        [TestCleanup]
        public void TestCleanup() {
            Cleanup();
        }

        private static void Cleanup() {
            if (Directory.Exists(Constants.AppDataSettingsFolder)) {
                Directory.Delete(Constants.AppDataSettingsFolder, true);
            }
            if (File.Exists(Constants.AppShortcutPath)) {
                File.Delete(Constants.AppShortcutPath);
            }

            Assert.IsFalse(Directory.Exists(Constants.AppDataSettingsFolder));
            Assert.IsFalse(File.Exists(Constants.AppShortcutPath));
        }

        [TestMethod]
        public void InitializeSettings_CreatesSettings() {
            AppSettings.InitializeSettings();

            Assert.IsTrue(Directory.Exists(Constants.AppDataSettingsFolder));
            Assert.IsTrue(File.Exists(Constants.AppDataSettingsPath));
        }

        [TestMethod]
        public void InitializeDefaultSettings_SetsDefaultSettings() {
            Directory.CreateDirectory(Constants.AppDataSettingsFolder);
            AppSettings.InitializeDefaultSettings();
            AppSettings.SettingsObject settingsObject = AppSettings.GetSettingsObject();

            Assert.AreEqual(Constants.AppDefaultSettings.DisplayMusicStatus, settingsObject.DisplayMusicStatus);
            Assert.AreEqual(Constants.AppDefaultSettings.ShowStatusOnPause, settingsObject.ShowStatusOnPause);
            Assert.AreEqual(Constants.AppDefaultSettings.LaunchAtStartup, settingsObject.LaunchAtStartup);
        }

        [TestMethod]
        public void GetSettingsObject_GetsSettings() {
            AppSettings.InitializeSettings();
            Assert.IsNotNull(AppSettings.GetSettingsObject());
        }

        [TestMethod]
        public void UpdateSettings_UpdatesSettings() {
            AppSettings.InitializeSettings();
            AppSettings.SettingsObject settingsObject = AppSettings.GetSettingsObject();

            settingsObject.DisplayMusicStatus = !settingsObject.DisplayMusicStatus;
            Assert.AreNotEqual(AppSettings.GetSettingsObject().DisplayMusicStatus, settingsObject.DisplayMusicStatus);

            AppSettings.UpdateSettings(settingsObject);
            Assert.AreEqual(AppSettings.GetSettingsObject().DisplayMusicStatus, settingsObject.DisplayMusicStatus);
        }

        [TestMethod]
        public void AddStartupShortcut_CreatesShortcut() {
            AppSettings.AddStartupShortcut();
            Assert.IsTrue(File.Exists(Constants.AppShortcutPath));
        }

        [TestMethod]
        public void RemoveStartupShortcut_DeletesShortcut() {
            AppSettings.AddStartupShortcut();
            Assert.IsTrue(File.Exists(Constants.AppShortcutPath));

            AppSettings.RemoveStartupShortcut();
            Assert.IsFalse(File.Exists(Constants.AppShortcutPath));
        }
    }
}
