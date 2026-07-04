namespace AgainDj.Domain.Model;

/// <summary>
/// The learned "style" of the DJ, updated online from what the user plays. Kept
/// intentionally interpretable — it captures how to mix (energy arc, tempo,
/// transition pacing, harmonic preference, mood affinity), not the audio itself.
/// </summary>
public sealed record StyleSnapshot
{
    /// <summary>Preferred energy target, 0..1 (EWMA of played energy).</summary>
    public double EnergyTarget { get; init; } = 0.6;

    /// <summary>Preferred average tempo (bpm).</summary>
    public double TempoCenter { get; init; } = 124;

    /// <summary>Mean seconds between transitions.</summary>
    public double MeanTransitionSeconds { get; init; } = 30;

    /// <summary>Preference for harmonic (key-compatible) transitions, 0..1.</summary>
    public double HarmonicAffinity { get; init; } = 0.7;

    /// <summary>Learned mood-tag weights (higher = played more).</summary>
    public IReadOnlyDictionary<string, double> MoodWeights { get; init; } =
        new Dictionary<string, double>();

    /// <summary>Number of training observations folded in so far.</summary>
    public long Samples { get; init; }

    public static StyleSnapshot Default { get; } = new();
}
