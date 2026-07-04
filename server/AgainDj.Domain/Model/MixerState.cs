namespace AgainDj.Domain.Model;

/// <summary>A sampler pad; <see cref="Active"/> pulses true briefly when triggered.</summary>
public sealed record SamplerPad(int Id, string Label, bool Active = false);

/// <summary>The authoritative, immutable state of the whole console.</summary>
public sealed record MixerState
{
    public required DeckState DeckA { get; init; }

    public required DeckState DeckB { get; init; }

    /// <summary>Crossfader position: 0 = full A, 1 = full B.</summary>
    public double Crossfader { get; init; } = 0.5;

    public double MasterVolume { get; init; } = 0.85;

    public SessionMode Mode { get; init; } = SessionMode.Autonomous;

    public IReadOnlyList<SamplerPad> Pads { get; init; } = [];

    public DeckState Deck(DeckId id) => id == DeckId.A ? DeckA : DeckB;

    public DeckId Other(DeckId id) => id == DeckId.A ? DeckId.B : DeckId.A;

    public MixerState WithDeck(DeckId id, DeckState deck) =>
        id == DeckId.A ? this with { DeckA = deck } : this with { DeckB = deck };

    /// <summary>The deck currently most audible, based on crossfader and playback.</summary>
    public DeckId LiveDeck => Crossfader <= 0.5 ? DeckId.A : DeckId.B;

    public static MixerState Initial(IReadOnlyList<SamplerPad> pads) => new()
    {
        DeckA = DeckState.Empty(DeckId.A),
        DeckB = DeckState.Empty(DeckId.B),
        Pads = pads,
    };
}
