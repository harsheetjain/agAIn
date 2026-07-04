using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;

namespace AgainDj.Domain.Mixing;

/// <summary>
/// Pure, deterministic state transitions for the console. Applying an action or
/// advancing time always returns a new <see cref="MixerState"/> with no side
/// effects, which makes the mixing logic trivial to unit-test.
/// </summary>
public static class MixerReducer
{
    /// <summary>Apply a single action to the state.</summary>
    public static MixerState Apply(MixerState state, DjAction action, ITrackLibrary library)
    {
        switch (action.Type)
        {
            case DjActionType.LoadTrack:
                if (action.Deck is { } loadDeck && action.TrackId is { } trackId)
                {
                    var track = library.Get(trackId);
                    return state.WithDeck(loadDeck, state.Deck(loadDeck) with
                    {
                        Track = track,
                        PositionSeconds = 0,
                        IsPlaying = false,
                        Tempo = track?.Bpm ?? state.Deck(loadDeck).Tempo,
                    });
                }

                return state;

            case DjActionType.Play:
                return WithDeck(state, action.Deck, d => d with { IsPlaying = true });

            case DjActionType.Pause:
                return WithDeck(state, action.Deck, d => d with { IsPlaying = false });

            case DjActionType.Cue:
                return WithDeck(state, action.Deck, d => d with { PositionSeconds = Math.Max(0, action.Value ?? 0) });

            case DjActionType.SetTempo:
                return WithDeck(state, action.Deck, d => d with { Tempo = action.Value ?? d.Tempo });

            case DjActionType.Nudge:
                return WithDeck(state, action.Deck, d => d with { Tempo = d.Tempo + (action.Value ?? 0) });

            case DjActionType.SetVolume:
                return WithDeck(state, action.Deck, d => d with { Volume = Clamp01(action.Value ?? d.Volume) });

            case DjActionType.SetEq:
                return action.Band is { } band
                    ? WithDeck(state, action.Deck, d => d with { Eq = d.Eq.With(band, Clamp01(action.Value ?? 0.5)) })
                    : state;

            case DjActionType.SetCrossfader:
                return state with { Crossfader = Clamp01(action.Value ?? state.Crossfader) };

            case DjActionType.Sync:
                if (action.Deck is { } syncDeck)
                {
                    var other = state.Deck(state.Other(syncDeck));
                    return WithDeck(state, syncDeck, d => d with { Tempo = other.Tempo > 0 ? other.Tempo : d.Tempo });
                }

                return state;

            case DjActionType.TriggerSample:
                return action.PadId is { } padId
                    ? state with { Pads = state.Pads.Select(p => p.Id == padId ? p with { Active = true } : p).ToList() }
                    : state;

            default:
                return state;
        }
    }

    /// <summary>Advance playback by <paramref name="dt"/>, looping the demo grooves and decaying pad pulses.</summary>
    public static MixerState Advance(MixerState state, TimeSpan dt)
    {
        var seconds = dt.TotalSeconds;
        var pads = state.Pads.Any(p => p.Active)
            ? state.Pads.Select(p => p.Active ? p with { Active = false } : p).ToList()
            : state.Pads;

        return state with
        {
            DeckA = AdvanceDeck(state.DeckA, seconds),
            DeckB = AdvanceDeck(state.DeckB, seconds),
            Pads = pads,
        };
    }

    private static DeckState AdvanceDeck(DeckState deck, double seconds)
    {
        if (!deck.IsPlaying || deck.Track is not { } track || track.DurationSeconds <= 0)
        {
            return deck;
        }

        var rate = track.Bpm > 0 ? deck.Tempo / track.Bpm : 1.0;
        var position = deck.PositionSeconds + (seconds * rate);
        if (position >= track.DurationSeconds)
        {
            position %= track.DurationSeconds; // loop the synthesized groove
        }

        return deck with { PositionSeconds = position };
    }

    private static MixerState WithDeck(MixerState state, DeckId? id, Func<DeckState, DeckState> update) =>
        id is { } deckId ? state.WithDeck(deckId, update(state.Deck(deckId))) : state;

    private static double Clamp01(double value) => Math.Clamp(value, 0, 1);
}
