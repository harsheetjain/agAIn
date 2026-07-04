namespace AgainDj.Domain.Model;

/// <summary>A compact numeric snapshot of the mix at the moment of an event (training features).</summary>
public sealed record MixerSnapshot
{
    public double EnergyA { get; init; }

    public double EnergyB { get; init; }

    public double Crossfader { get; init; }

    public double TempoA { get; init; }

    public double TempoB { get; init; }

    public double SetPositionSeconds { get; init; }
}

/// <summary>A human action captured with the mix context it happened in — the training signal.</summary>
public sealed record SessionEvent
{
    public required DjAction Action { get; init; }

    public required MixerSnapshot Context { get; init; }
}
