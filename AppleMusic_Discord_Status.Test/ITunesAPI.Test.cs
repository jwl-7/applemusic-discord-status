using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;


namespace AppleMusic_Discord_Status.Test {
    [TestClass]
    public class ITunesAPITests {
        [TestMethod]
        public async Task GetAlbumArtworkUrl_ReturnsValidUrl() {
            string albumName = "Spilling over Every Side";
            string artistName = "Pretty Lights";
            string artworkUrl = await ITunesAPI.GetAlbumArtworkUrl(albumName, artistName);

            Assert.IsNotNull(artworkUrl, "Artwork URL should not be null.");
            Assert.IsTrue(artworkUrl.Contains("1000x1000bb"), "Artwork URL should be for 1000x1000 image.");
        }

        [TestMethod]
        public async Task GetAlbumArtworkUrl_NonExistentAlbum() {
            string albumName = "Swirl Bridge";
            string artistName = "Pretty Lights";
            string artworkUrl = await ITunesAPI.GetAlbumArtworkUrl(albumName, artistName);

            Assert.IsNull(artworkUrl, "Artwork URL should be null.");
        }

        [TestMethod]
        public async Task GetAlbumArtworkUrl_EmptyArtistAlbum() {
            string albumName = "";
            string artistName = "";
            string artworkUrl = await ITunesAPI.GetAlbumArtworkUrl(albumName, artistName);

            Assert.IsNull(artworkUrl, "Artwork URL should be null.");
        }

        [TestMethod]
        public async Task GetAlbumArtworkUrl_HandlesBigString() {
            string albumName = new('x', 128);
            string artistName = new('x', 128);
            string artworkUrl = await ITunesAPI.GetSongLink(albumName, artistName);

            Assert.IsNull(artworkUrl, "Artwork URL should be null.");
        }

        [TestMethod]
        public async Task GetSongLink_ReturnsValidUrl() {
            string songName = "High School Art Class";
            string artistName = "Pretty Lights";
            string songUrl = await ITunesAPI.GetSongLink(songName, artistName);

            Assert.IsNotNull(songUrl, "Song URL should not be null.");
        }

        [TestMethod]
        public async Task GetSongLink_NonExistentSong() {
            string songName = "Swirl Segway";
            string artistName = "Pretty Lights";
            string songUrl = await ITunesAPI.GetSongLink(songName, artistName);

            Assert.IsNull(songUrl, "Song URL should be null.");
        }

        [TestMethod]
        public async Task GetSongLink_EmptyArtistAlbum() {
            string songName = "";
            string artistName = "";
            string songUrl = await ITunesAPI.GetSongLink(songName, artistName);

            Assert.IsNull(songUrl, "Song URL should be null.");
        }

        [TestMethod]
        public async Task GetSongLink_HandlesBigString() {
            string songName = new('x', 128);
            string artistName = new('x', 128);
            string songUrl = await ITunesAPI.GetSongLink(songName, artistName);

            Assert.IsNull(songUrl, "Song URL should be null.");
        }
    }
}
