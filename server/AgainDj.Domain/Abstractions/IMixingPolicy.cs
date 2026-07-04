using AgainDj.Domain.Model;

namespace AgainDj.Domain.Abstractions;

/// <summary>
/// Decides what the DJ does next. The strategy is swappable — a rule-based
/// implementation, a learned one, or a hybrid that blends both.
/// </summary>
public interface IMixingPolicy
{
    /// <summary>Produce the next set of actions (and optional transition) for the current context.</summary>
    DjDecision Decide(MixContext context);

    /// <summary>Pick the best next track to cue on <paramref name="targetDeck"/>, or null if none fits.</summary>
    Track? ChooseNextTrack(MixContext context, DeckId targetDeck);

    /// <summary>Plan how to blend from one track to another.</summary>
    TransitionPlan PlanTransition(Track from, Track to, MixContext context);
}
