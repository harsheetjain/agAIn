namespace AgainDj.Domain.Model;

/// <summary>Three-band EQ, each 0..1 with 0.5 as unity (flat).</summary>
public sealed record EqSettings(double Low = 0.5, double Mid = 0.5, double High = 0.5)
{
    public EqSettings With(EqBand band, double value) => band switch
    {
        EqBand.Low => this with { Low = value },
        EqBand.Mid => this with { Mid = value },
        EqBand.High => this with { High = value },
        _ => this,
    };
}

/// <summary>Immutable snapshot of a single deck.</summary>
public sealed record DeckState
{
    public required DeckId Id { get; init; }

    public Track? Track { get; init; }

    public bool IsPlaying { get; init; }

    public double PositionSeconds { get; init; }

    /// <summary>Current playback tempo (bpm); may be pitched away from the track's native bpm.</summary>
    public double Tempo { get; init; }

    /// <summary>Channel fader, 0..1.</summary>
    public double Volume { get; init; } = 1.0;

    public EqSettings Eq { get; init; } = new();

    public double ProgressFraction =>
        Track is { DurationSeconds: > 0 } t ? Math.Clamp(PositionSeconds / t.DurationSeconds, 0, 1) : 0;

    /// <summary>Seconds until the track ends (0 when nothing is loaded).</summary>
    public double SecondsRemaining =>
        Track is { } t ? Math.Max(0, t.DurationSeconds - PositionSeconds) : 0;

    public static DeckState Empty(DeckId id) => new() { Id = id, Tempo = 124 };
}
