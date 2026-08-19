namespace AppleMusic_Discord_Status {
    /// <summary>
    /// Internal app state to track currently playing track.
    /// </summary>
    internal class TrackData {
        internal string? Song { get; set; }
        internal string? Artist { get; set; }
        internal string? Album { get; set; }
        internal string? SongUrl { get; set; }
        internal string? AlbumUrl { get; set; }
        internal string? ArtworkUrl { get; set; }
        internal int? Duration { get; set; }
        internal int? CurrentTime { get; set; }
        internal int? RemainingTime { get; set; }
        internal bool IsPlaying { get; set; }
    }

    /// <summary>
    /// Metadata fetched from the Apple iTunes API.
    /// </summary>
    internal class ITunesMetadata {
        internal string? SongUrl { get; set; }
        internal string? AlbumUrl { get; set; }
        internal string? ArtworkUrl { get; set; }
        internal int? Duration { get; set; }
    }

    /// <summary>
    /// Metadata fetched from the Apple Music UI.
    /// </summary>
    internal class AppleMusicMetadata {
        internal string? Song { get; set; }
        internal string? Artist { get; set; }
        internal string? Album { get; set; }
        internal int? CurrentTime { get; set; }
        internal int? RemainingTime { get; set; }
        internal bool IsPlaying { get; set; }
    }
}
