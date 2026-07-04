using AgainDj.Domain.Model;
using AgainDj.Infrastructure.Library;
using AgainDj.Infrastructure.Policies;

namespace AgainDj.Tests;

public class RuleMixingPolicyTests
{
    private readonly InMemoryTrackLibrary _library = new();
    private readonly RuleMixingPolicy _policy = new();

    private MixContext Context(MixerState state, double setPosition = 0) => new()
    {
        State = state,
        Library = _library.All,
        Style = StyleSnapshot.Default,
        SetPositionSeconds = setPosition,
    };

    [Fact]
    public void Cold_start_loads_and_plays_deck_A()
    {
        var decision = _policy.Decide(Context(MixerState.Initial([])));

        Assert.Contains(decision.Actions, a => a is { Type: DjActionType.LoadTrack, Deck: DeckId.A });
        Assert.Contains(decision.Actions, a => a is { Type: DjActionType.Play, Deck: DeckId.A });
    }

    [Fact]
    public void ChooseNextTrack_never_returns_a_loaded_track()
    {
        var state = MixerState.Initial([]) with
        {
            DeckA = DeckState.Empty(DeckId.A) with { Track = _library.Get("concrete"), IsPlaying = true },
            Crossfader = 0,
        };

        var next = _policy.ChooseNextTrack(Context(state, 10), DeckId.B);

        Assert.NotNull(next);
        Assert.NotEqual("concrete", next!.Id);
    }

    [Fact]
    public void Reaching_the_transition_threshold_cues_the_other_deck()
    {
        var live = DeckState.Empty(DeckId.A) with
        {
            Track = _library.Get("dawn-chorus"),
            IsPlaying = true,
            PositionSeconds = StyleSnapshot.Default.MeanTransitionSeconds + 1,
        };
        var state = MixerState.Initial([]) with { DeckA = live, Crossfader = 0 };

        var decision = _policy.Decide(Context(state, 40));

        Assert.Contains(decision.Actions, a => a is { Type: DjActionType.LoadTrack, Deck: DeckId.B });
        Assert.Contains(decision.Actions, a => a is { Type: DjActionType.Play, Deck: DeckId.B });
    }
}
