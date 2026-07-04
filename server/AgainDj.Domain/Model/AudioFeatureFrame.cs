namespace AgainDj.Domain.Model;

/// <summary>
/// One frame of audio features captured from the live console output (computed
/// client-side). Feeds the style trainer. Contains only derived features —
/// never raw or copyrighted audio.
/// </summary>
public sealed record AudioFeatureFrame
{
    public required double TimestampSeconds { get; init; }

    /// <summary>RMS loudness, 0..1.</summary>
    public required double Rms { get; init; }

    /// <summary>Spectral centroid (brightness), normalized 0..1.</summary>
    public double SpectralCentroid { get; init; }

    /// <summary>Estimated instantaneous tempo (bpm); 0 when unknown.</summary>
    public double Tempo { get; init; }

    /// <summary>12-bin chroma vector (pitch-class energy); may be empty.</summary>
    public IReadOnlyList<double> Chroma { get; init; } = [];
}
