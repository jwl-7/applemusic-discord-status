using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.Devices.AllJoyn;

namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Provides functionality for interacting with the Apple iTunes API.
    /// </summary>
    internal class ITunesAPI {
        private static readonly HttpClient client = new();

        /// <summary>
        /// Fetches track metadata from iTunes API.
        /// </summary>
        /// <param name="song">Name of the song.</param>
        /// <param name="artist">Name of the artist.</param>
        /// <param name="album">Optional name of the album for precise matching.</param>
        /// <returns>iTunesMetadata from iTunes API if found; otherwise, null.</returns>
        internal static async Task<ITunesMetadata?> GetTrackMetadata(string song, string artist, string? album = null) {
            string query = $"{song} {artist}";
            string url = $"{Constants.ITunesApiUrl}{Uri.EscapeDataString(query)}&entity=song&limit=10";

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode) {
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                if (json["resultCount"]?.ToObject<int>() > 0) {
                    JToken? result = json["results"]?[0];
                    if (result is null) return null;

                    if (!string.IsNullOrEmpty(album)) {
                        foreach (JToken item in json["results"]!) {
                            string collectionName = item["collectionName"]?.ToString().ToLower() ?? "";
                            string artistName = item["artistName"]?.ToString().ToLower() ?? "";

                            if (
                                collectionName.Equals(album, StringComparison.CurrentCultureIgnoreCase) &&
                                artistName.Equals(artist, StringComparison.CurrentCultureIgnoreCase)
                            ) {
                                result = item;
                                break;
                            }
                        }
                    }

                    int? durationSeconds = null;
                    if (result["trackTimeMillis"] != null) {
                        int durationInMillis = (int)result["trackTimeMillis"]!;
                        durationSeconds = durationInMillis / 1000;
                    }

                    string? artworkUrl = result["artworkUrl100"]?.ToString();
                    if (!string.IsNullOrEmpty(artworkUrl)) {
                        artworkUrl = artworkUrl.Replace("100x100bb", "1000x1000bb");
                    }

                    return new ITunesMetadata {
                        Duration = durationSeconds,
                        SongUrl = result["trackViewUrl"]?.ToString(),
                        AlbumUrl = result["collectionViewUrl"]?.ToString(),
                        ArtworkUrl = artworkUrl
                    };
                }
            }

            return null;
        }
    }
}
