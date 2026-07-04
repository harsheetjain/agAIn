using AgainDj.Domain.Mixing;
using AgainDj.Domain.Model;
using AgainDj.Infrastructure.Library;

namespace AgainDj.Tests;

public class MixerReducerTests
{
    private readonly InMemoryTrackLibrary _library = new();

    [Fact]
    public void LoadTrack_cues_track_paused_at_start_with_native_tempo()
    {
        var state = MixerState.Initial([]);

        state = MixerReducer.Apply(state, DjAction.LoadTrack(DeckId.A, "concrete", Actor.Ai), _library);

        Assert.Equal("concrete", state.DeckA.Track!.Id);
        Assert.Equal(140, state.DeckA.Tempo);
        Assert.Equal(0, state.DeckA.PositionSeconds);
        Assert.False(state.DeckA.IsPlaying);
    }

    [Fact]
    public void Play_then_Advance_moves_the_playhead()
    {
        var state = MixerState.Initial([]);
        state = MixerReducer.Apply(state, DjAction.LoadTrack(DeckId.A, "concrete", Actor.Ai), _library);
        state = MixerReducer.Apply(state, DjAction.Play(DeckId.A, Actor.Ai), _library);

        var advanced = MixerReducer.Advance(state, TimeSpan.FromSeconds(2));

        Assert.True(advanced.DeckA.PositionSeconds > 0);
    }

    [Fact]
    public void SetCrossfader_is_clamped_to_unit_range()
    {
        var state = MixerState.Initial([]);

        Assert.Equal(1.0, MixerReducer.Apply(state, DjAction.SetCrossfader(2.0, Actor.Ai), _library).Crossfader);
        Assert.Equal(0.0, MixerReducer.Apply(state, DjAction.SetCrossfader(-1.0, Actor.Ai), _library).Crossfader);
    }

    [Fact]
    public void Sync_matches_tempo_to_the_other_deck()
    {
        var state = MixerState.Initial([]);
        state = MixerReducer.Apply(state, DjAction.LoadTrack(DeckId.A, "concrete", Actor.Ai), _library); // 140
        state = MixerReducer.Apply(state, DjAction.LoadTrack(DeckId.B, "afterglow", Actor.Ai), _library); // 100

        state = MixerReducer.Apply(state, DjAction.Sync(DeckId.B, Actor.Ai), _library);

        Assert.Equal(state.DeckA.Tempo, state.DeckB.Tempo);
    }
}
