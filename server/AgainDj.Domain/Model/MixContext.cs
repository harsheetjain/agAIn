namespace AgainDj.Domain.Model;

/// <summary>Everything a mixing policy needs to make a decision.</summary>
public sealed record MixContext
{
    public required MixerState State { get; init; }

    public required IReadOnlyList<Track> Library { get; init; }

    public required StyleSnapshot Style { get; init; }

    /// <summary>Seconds since the current set started.</summary>
    public required double SetPositionSeconds { get; init; }

    /// <summary>Recent loudness from the listening loop (0..1), for energy sensing.</summary>
    public double RecentRms { get; init; }
}

/// <summary>The output of a policy: the actions to perform now, plus timing and rationale.</summary>
public sealed record DjDecision
{
    public IReadOnlyList<DjAction> Actions { get; init; } = [];

    public TransitionPlan? Transition { get; init; }

    public string? Rationale { get; init; }

    /// <summary>How long to wait before asking the policy again.</summary>
    public TimeSpan NextDecisionDelay { get; init; } = TimeSpan.FromMilliseconds(500);

    public static DjDecision Idle(TimeSpan delay, string? why = null) =>
        new() { NextDecisionDelay = delay, Rationale = why };
}
