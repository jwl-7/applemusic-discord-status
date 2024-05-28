using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Provides functionality for interacting with the Apple iTunes API.
    /// </summary>
    internal class ITunesAPI {
        private static readonly HttpClient client = new();

        /// <summary>
        /// Fetches the URL for an album's artwork based on the album name and artist name.
        /// </summary>
        /// <param name="albumName">Name of the album.</param>
        /// <param name="artistName">Name of the artist.</param>
        /// <returns>The URL of the album artwork if found; otherwise, null.</returns>
        public static async Task<string> GetAlbumArtworkUrl(string albumName, string artistName) {
            string query = $"{albumName} {artistName}";
            string url = $"{Constants.ITunesApiUrl}{Uri.EscapeDataString(query)}&entity=album";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseBody);

            if (json["resultCount"].ToObject<int>() > 0) {
                foreach (JToken result in json["results"]) {
                    string collectionName = result["collectionName"].ToString().ToLower();
                    string artist = result["artistName"].ToString().ToLower();

                    if (
                        collectionName.Equals(albumName, StringComparison.CurrentCultureIgnoreCase) && 
                        artist.Equals(artistName, StringComparison.CurrentCultureIgnoreCase)
                    ) {
                        string artworkUrl = result["artworkUrl100"].ToString();
                        artworkUrl = artworkUrl.Replace("100x100bb", "1000x1000bb");
                        return artworkUrl;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Fetches the URL for a song based on the song name and artist name.
        /// </summary>
        /// <param name="songName">Name of the song.</param>
        /// <param name="artistName">Name of the artist.</param>
        /// <returns>The URL of the song if found; otherwise, null.</returns>
        public static async Task<string> GetSongLink(string songName, string artistName) {
            string query = $"{songName} {artistName}";
            string url = $"{Constants.ITunesApiUrl}{Uri.EscapeDataString(query)}&entity=song";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseBody);

            if (json["resultCount"].ToObject<int>() > 0) {
                JToken result = json["results"][0];
                string songLink = result["trackViewUrl"].ToString();
                return songLink;
            }

            return null;
        }
    }
}
