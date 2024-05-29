using DiscordRPC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;


namespace AppleMusic_Discord_Status.Test {
    [TestClass]
    public class DiscordRichPresenceTests {
        [TestMethod]
        public void UpdatePresence_CallsSetPresence_WhenInitialized() {
            Mock<DiscordRpcClient> mockClient = new("fake_token");
            mockClient.Setup(c => c.Initialize());
            mockClient.Setup(c => c.SetPresence(It.IsAny<RichPresence>()));

            Mock<DiscordRichPresence> mockDiscordPresence = new() { CallBase = true };
            mockDiscordPresence.Setup(d => d.client).Returns(mockClient.Object);
            mockDiscordPresence.Object.isInitialized = true;
            mockDiscordPresence.Object.UpdatePresence("details", "state", "albumArtwork", "songEnd", "songUrl", true);

            mockClient.Verify(c => c.SetPresence(It.IsAny<RichPresence>()), Times.Once);
        }

        [TestMethod]
        public async Task InitializeClient_SetsIsInitializedToTrue_WhenReady() {
            Mock<DiscordRpcClient> mockClient = new("fake_token");
            Mock<DiscordRichPresence> mockDiscordPresence = new() { CallBase = true };

            mockDiscordPresence.Setup(d => d.client).Returns(mockClient.Object);
            mockClient.Raise(c => c.OnReady += null, EventArgs.Empty);

            Assert.IsTrue(mockDiscordPresence.Object.isInitialized);
        }

        [TestMethod]
        public void ParseTime_ValidTimeSeconds() {
            string oneDigit = "-:1";
            string twoDigits = "-:12";
            string paddedDigit = "-:01";

            int seconds = DiscordRichPresence.ParseTimeRemaining(oneDigit);
            Assert.AreEqual(1, seconds);

            seconds = DiscordRichPresence.ParseTimeRemaining(twoDigits);
            Assert.AreEqual(12, seconds);

            seconds = DiscordRichPresence.ParseTimeRemaining(paddedDigit);
            Assert.AreEqual(1, seconds);
        }

        [TestMethod]
        public void ParseTime_ValidTimeMinutes() {
            string threeDigits = "-1:23";
            string fourDigits = "-12:34";
            string paddedDigits = "-01:23";

            int seconds = DiscordRichPresence.ParseTimeRemaining(threeDigits);
            Assert.AreEqual(83, seconds);

            seconds = DiscordRichPresence.ParseTimeRemaining(fourDigits);
            Assert.AreEqual(754, seconds);

            seconds = DiscordRichPresence.ParseTimeRemaining(paddedDigits);
            Assert.AreEqual(83, seconds);
        }

        [TestMethod]
        public void ParseTime_ValidTimeHours() {
            string fiveDigits = "-1:23:45";
            string sixDigits = "-12:34:45";
            string paddedDigits = "-01:02:03";

            int seconds = DiscordRichPresence.ParseTimeRemaining(fiveDigits);
            Assert.AreEqual(5025, seconds);

            seconds = DiscordRichPresence.ParseTimeRemaining(sixDigits);
            Assert.AreEqual(45285, seconds);

            seconds = DiscordRichPresence.ParseTimeRemaining(paddedDigits);
            Assert.AreEqual(3723, seconds);
        }

        [TestMethod]
        public void ParseTime_EmptyTime() {
            int seconds = DiscordRichPresence.ParseTimeRemaining("");
            Assert.AreEqual(0, seconds);
        }

        [TestMethod]
        public void ParseTime_InvalidTime() {
            int seconds = DiscordRichPresence.ParseTimeRemaining("::-9:9");
            Assert.AreEqual(0, seconds);

            seconds = DiscordRichPresence.ParseTimeRemaining("99:(9asdf);");
            Assert.AreEqual(0, seconds);    
        }
    }
}
