namespace AgainDj.Domain.Model;

/// <summary>
/// A single console action. A flat, discriminated shape (rather than a class
/// hierarchy) keeps the wire contract trivially (de)serializable and easy for
/// the TypeScript client to switch on. Use the factory methods to build them.
/// </summary>
public sealed record DjAction
{
    public required DjActionType Type { get; init; }

    public required Actor Actor { get; init; }

    public DeckId? Deck { get; init; }

    public EqBand? Band { get; init; }

    public double? Value { get; init; }

    public string? TrackId { get; init; }

    public int? PadId { get; init; }

    /// <summary>Optional human-readable rationale (e.g. why the AI did this).</summary>
    public string? Note { get; init; }

    public DateTimeOffset At { get; init; } = DateTimeOffset.UtcNow;

    public static DjAction LoadTrack(DeckId deck, string trackId, Actor actor, string? note = null) =>
        new() { Type = DjActionType.LoadTrack, Actor = actor, Deck = deck, TrackId = trackId, Note = note };

    public static DjAction Play(DeckId deck, Actor actor) =>
        new() { Type = DjActionType.Play, Actor = actor, Deck = deck };

    public static DjAction Pause(DeckId deck, Actor actor) =>
        new() { Type = DjActionType.Pause, Actor = actor, Deck = deck };

    public static DjAction Cue(DeckId deck, double positionSeconds, Actor actor) =>
        new() { Type = DjActionType.Cue, Actor = actor, Deck = deck, Value = positionSeconds };

    public static DjAction SetTempo(DeckId deck, double bpm, Actor actor) =>
        new() { Type = DjActionType.SetTempo, Actor = actor, Deck = deck, Value = bpm };

    public static DjAction Nudge(DeckId deck, double deltaBpm, Actor actor) =>
        new() { Type = DjActionType.Nudge, Actor = actor, Deck = deck, Value = deltaBpm };

    public static DjAction SetVolume(DeckId deck, double value, Actor actor) =>
        new() { Type = DjActionType.SetVolume, Actor = actor, Deck = deck, Value = value };

    public static DjAction SetEq(DeckId deck, EqBand band, double value, Actor actor) =>
        new() { Type = DjActionType.SetEq, Actor = actor, Deck = deck, Band = band, Value = value };

    public static DjAction SetCrossfader(double value, Actor actor) =>
        new() { Type = DjActionType.SetCrossfader, Actor = actor, Value = value };

    public static DjAction TriggerSample(int padId, Actor actor) =>
        new() { Type = DjActionType.TriggerSample, Actor = actor, PadId = padId };

    public static DjAction Sync(DeckId deck, Actor actor) =>
        new() { Type = DjActionType.Sync, Actor = actor, Deck = deck };
}
