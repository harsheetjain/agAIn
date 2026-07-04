using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;

namespace AgainDj.Infrastructure.Policies;

/// <summary>
/// A rule-based DJ brain: it opens a set, rides each track, performs subtle
/// live moves, and beat-matched/harmonically blends into the next track when the
/// current one has run its course. It reads the learned <see cref="StyleSnapshot"/>
/// (energy target, tempo centre, pacing, harmonic and mood preference) from the
/// context, so as the style trainer learns from what you play, the autonomous
/// set drifts toward your taste. Stateless: every decision is derived from the
/// current <see cref="MixContext"/>, which keeps it easy to test.
/// </summary>
public sealed class RuleMixingPolicy : IMixingPolicy
{
    private const double TransitionSeconds = 6;
    private const double CrossfadeStep = 0.06;
    private const int TransitionTickMs = 350;
    private const int MidTrackTickMs = 1200;

    private readonly Random _rng = Random.Shared;

    public DjDecision Decide(MixContext context)
    {
        var state = context.State;
        var playing = new List<DeckState>(2);
        if (state.DeckA.IsPlaying)
        {
            playing.Add(state.DeckA);
        }

        if (state.DeckB.IsPlaying)
        {
            playing.Add(state.DeckB);
        }

        return playing.Count switch
        {
            0 => ColdStart(context),
            1 => DriveSingle(context, playing[0]),
            _ => AdvanceTransition(context),
        };
    }

    public Track? ChooseNextTrack(MixContext context, DeckId targetDeck)
    {
        var loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (context.State.DeckA.Track is { } ta)
        {
            loaded.Add(ta.Id);
        }

        if (context.State.DeckB.Track is { } tb)
        {
            loaded.Add(tb.Id);
        }

        var live = context.State.Deck(context.State.LiveDeck).Track;

        Track? best = null;
        var bestScore = double.NegativeInfinity;
        foreach (var track in context.Library)
        {
            if (loaded.Contains(track.Id))
            {
                continue;
            }

            var score = ScoreCandidate(track, live, context.Style);
            if (score > bestScore ||
                (score == bestScore && (best is null || string.CompareOrdinal(track.Id, best.Id) < 0)))
            {
                best = track;
                bestScore = score;
            }
        }

        return best ?? context.Library.FirstOrDefault(t => !loaded.Contains(t.Id));
    }

    public TransitionPlan PlanTransition(Track from, Track to, MixContext context)
    {
        var energyJump = to.Energy - from.Energy;
        var tempoGap = Math.Abs(to.Bpm - from.Bpm);
        var technique = (energyJump, tempoGap) switch
        {
            ( > 0.25, _) => TransitionTechnique.EchoOut,
            (_, > 8) => TransitionTechnique.FilterFade,
            ( < -0.15, _) => TransitionTechnique.BasslineSwap,
            _ => TransitionTechnique.Crossfade,
        };

        return new TransitionPlan
        {
            FromDeck = context.State.LiveDeck,
            ToDeck = context.State.Other(context.State.LiveDeck),
            Technique = technique,
            DurationSeconds = TransitionSeconds,
            Reason = $"{technique} into {to.Title} ({to.Key}, {to.Bpm:0} BPM)",
        };
    }

    private DjDecision ColdStart(MixContext context)
    {
        var opener = ChooseNextTrack(context, DeckId.A) ?? context.Library.FirstOrDefault();
        if (opener is null)
        {
            return DjDecision.Idle(TimeSpan.FromSeconds(1), "Crate is empty");
        }

        return new DjDecision
        {
            Actions =
            [
                DjAction.SetCrossfader(0.0, Actor.Ai),
                DjAction.LoadTrack(DeckId.A, opener.Id, Actor.Ai, $"Opening with {opener.Title}"),
                DjAction.SetVolume(DeckId.A, 1.0, Actor.Ai),
                DjAction.Play(DeckId.A, Actor.Ai),
            ],
            Rationale = $"Opening the set with {opener.Title} ({opener.Bpm:0} BPM)",
            NextDecisionDelay = TimeSpan.FromMilliseconds(700),
        };
    }

    private DjDecision DriveSingle(MixContext context, DeckState live)
    {
        if (live.Track is { } current && live.PositionSeconds >= TransitionThreshold(live, context.Style))
        {
            return BeginTransition(context, live, current);
        }

        return Perform(context, live);
    }

    private DjDecision BeginTransition(MixContext context, DeckState live, Track current)
    {
        var incoming = context.State.Other(live.Id);
        var next = ChooseNextTrack(context, incoming);
        if (next is null)
        {
            return Perform(context, live);
        }

        var plan = PlanTransition(current, next, context);
        return new DjDecision
        {
            Actions =
            [
                DjAction.LoadTrack(incoming, next.Id, Actor.Ai, $"Cueing {next.Title}"),
                DjAction.Sync(incoming, Actor.Ai),                      // beat-match to the live deck
                DjAction.SetEq(incoming, EqBand.Low, 0.0, Actor.Ai),    // hold the incoming bass out
                DjAction.SetVolume(incoming, 1.0, Actor.Ai),
                DjAction.Play(incoming, Actor.Ai),
            ],
            Transition = plan,
            Rationale = plan.Reason,
            NextDecisionDelay = TimeSpan.FromMilliseconds(TransitionTickMs),
        };
    }

    private DjDecision AdvanceTransition(MixContext context)
    {
        var state = context.State;

        // The freshly cued deck has the smaller position; that's where we're heading.
        var (incoming, outgoing) = state.DeckA.PositionSeconds <= state.DeckB.PositionSeconds
            ? (state.DeckA, state.DeckB)
            : (state.DeckB, state.DeckA);

        var target = incoming.Id == DeckId.A ? 0.0 : 1.0;
        var current = state.Crossfader;

        if (Math.Abs(current - target) <= CrossfadeStep + 1e-6)
        {
            return new DjDecision
            {
                Actions =
                [
                    DjAction.SetCrossfader(target, Actor.Ai),
                    DjAction.SetEq(incoming.Id, EqBand.Low, 0.5, Actor.Ai),
                    DjAction.SetEq(outgoing.Id, EqBand.Low, 0.5, Actor.Ai),
                    DjAction.Pause(outgoing.Id, Actor.Ai),
                ],
                Rationale = $"Locked into {incoming.Track?.Title}",
                NextDecisionDelay = TimeSpan.FromMilliseconds(MidTrackTickMs),
            };
        }

        var stepped = Math.Clamp(current + (Math.Sign(target - current) * CrossfadeStep), 0, 1);
        var proximity = incoming.Id == DeckId.A ? 1 - stepped : stepped; // 0..1 toward incoming

        return new DjDecision
        {
            Actions =
            [
                DjAction.SetCrossfader(stepped, Actor.Ai),
                DjAction.SetEq(incoming.Id, EqBand.Low, Math.Clamp(proximity, 0, 1), Actor.Ai),
                DjAction.SetEq(outgoing.Id, EqBand.Low, Math.Clamp(1 - proximity, 0, 1), Actor.Ai),
            ],
            Rationale = "Blending the bassline across",
            NextDecisionDelay = TimeSpan.FromMilliseconds(TransitionTickMs),
        };
    }

    private DjDecision Perform(MixContext context, DeckState live)
    {
        var delay = TimeSpan.FromMilliseconds(MidTrackTickMs);

        // Occasionally make a subtle "alive" move so the console never looks static.
        if (_rng.NextDouble() < 0.35)
        {
            var pads = context.State.Pads;
            DjAction action = _rng.Next(3) switch
            {
                0 when pads.Count > 0 => DjAction.TriggerSample(pads[_rng.Next(pads.Count)].Id, Actor.Ai),
                1 => DjAction.SetEq(live.Id, EqBand.High, 0.5 + ((_rng.NextDouble() * 0.3) - 0.15), Actor.Ai),
                _ => DjAction.SetEq(live.Id, EqBand.Mid, 0.5 + ((_rng.NextDouble() * 0.3) - 0.15), Actor.Ai),
            };

            return new DjDecision { Actions = [action], Rationale = "Working the track", NextDecisionDelay = delay };
        }

        return DjDecision.Idle(delay, "Riding the groove");
    }

    private static double TransitionThreshold(DeckState live, StyleSnapshot style)
    {
        var duration = live.Track?.DurationSeconds ?? 180;
        return Math.Min(style.MeanTransitionSeconds, Math.Max(8, duration - TransitionSeconds - 2));
    }

    private static double ScoreCandidate(Track track, Track? live, StyleSnapshot style)
    {
        var score = 0.0;

        // Energy close to the learned target.
        score += 1.0 - Math.Abs(track.Energy - style.EnergyTarget);

        // Tempo close to the learned centre (20 BPM window).
        score += 1.0 - Math.Min(1.0, Math.Abs(track.Bpm - style.TempoCenter) / 20.0);

        if (live is { } current)
        {
            if (track.Key.IsCompatibleWith(current.Key))
            {
                score += style.HarmonicAffinity;
            }

            // Easy beat-match to the live deck (16 BPM window).
            score += 0.5 * (1.0 - Math.Min(1.0, Math.Abs(track.Bpm - current.Bpm) / 16.0));
        }

        // Learned mood affinity.
        var moodBonus = 0.0;
        foreach (var mood in track.Moods)
        {
            if (style.MoodWeights.TryGetValue(mood, out var weight))
            {
                moodBonus += weight;
            }
        }

        score += Math.Min(1.0, moodBonus);
        return score;
    }
}
