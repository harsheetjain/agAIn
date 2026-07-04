using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;

namespace AgainDj.Infrastructure.Learning;

/// <summary>
/// Online, interpretable style learner — the heart of the "listen → sample →
/// train" loop. It folds live audio feature frames and the human's console
/// actions into a <see cref="StyleSnapshot"/> using exponential moving averages.
/// It learns *how* you mix (energy, tempo, pacing, harmonic and mood
/// preference); it never stores or reproduces the audio itself.
/// </summary>
public sealed class OnlineStyleTrainer : IStyleTrainer
{
    private const double FeatureAlpha = 0.02;
    private const double EventAlpha = 0.12;
    private const double PacingAlpha = 0.20;
    private const double HarmonicAlpha = 0.15;
    private const double MoodStep = 0.10;
    private const int SaveEverySamples = 40;

    private readonly ITrackLibrary _library;
    private readonly IStyleStore _store;
    private readonly Lock _gate = new();
    private readonly Dictionary<string, double> _moodWeights = new(StringComparer.OrdinalIgnoreCase);

    private double _energyTarget;
    private double _tempoCenter;
    private double _meanTransitionSeconds;
    private double _harmonicAffinity;
    private long _samples;

    private string? _lastLoadedTrackId;
    private double _lastTransitionSetPos;

    public OnlineStyleTrainer(ITrackLibrary library, IStyleStore store)
    {
        _library = library;
        _store = store;

        var seed = store.Load() ?? StyleSnapshot.Default;
        _energyTarget = seed.EnergyTarget;
        _tempoCenter = seed.TempoCenter;
        _meanTransitionSeconds = seed.MeanTransitionSeconds;
        _harmonicAffinity = seed.HarmonicAffinity;
        _samples = seed.Samples;
        foreach (var (mood, weight) in seed.MoodWeights)
        {
            _moodWeights[mood] = weight;
        }
    }

    public StyleSnapshot Snapshot
    {
        get
        {
            lock (_gate)
            {
                return SnapshotNoLock();
            }
        }
    }

    public void ObserveFeatureFrame(AudioFeatureFrame frame)
    {
        lock (_gate)
        {
            _energyTarget = Ewma(_energyTarget, Math.Clamp(frame.Rms, 0, 1), FeatureAlpha);
            if (frame.Tempo is > 40 and < 220)
            {
                _tempoCenter = Ewma(_tempoCenter, frame.Tempo, FeatureAlpha);
            }

            _samples++;
            if (_samples % SaveEverySamples == 0)
            {
                _store.Save(SnapshotNoLock());
            }
        }
    }

    public void ObserveEvent(SessionEvent evt)
    {
        lock (_gate)
        {
            if (evt.Action is { Type: DjActionType.LoadTrack, TrackId: { } id } &&
                _library.Get(id) is { } track)
            {
                _energyTarget = Ewma(_energyTarget, track.Energy, EventAlpha);
                _tempoCenter = Ewma(_tempoCenter, track.Bpm, EventAlpha);

                foreach (var mood in track.Moods)
                {
                    _moodWeights[mood] = Math.Min(3.0, _moodWeights.GetValueOrDefault(mood) + MoodStep);
                }

                if (_lastLoadedTrackId is { } previousId && _library.Get(previousId) is { } previous)
                {
                    var compatible = track.Key.IsCompatibleWith(previous.Key) ? 1.0 : 0.0;
                    _harmonicAffinity = Math.Clamp(Ewma(_harmonicAffinity, compatible, HarmonicAlpha), 0.2, 0.98);
                }

                var elapsed = evt.Context.SetPositionSeconds - _lastTransitionSetPos;
                if (elapsed is >= 5 and <= 600)
                {
                    _meanTransitionSeconds = Ewma(_meanTransitionSeconds, elapsed, PacingAlpha);
                }

                _lastTransitionSetPos = evt.Context.SetPositionSeconds;
                _lastLoadedTrackId = id;
            }

            _samples++;
            _store.Save(SnapshotNoLock());
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            var defaults = StyleSnapshot.Default;
            _energyTarget = defaults.EnergyTarget;
            _tempoCenter = defaults.TempoCenter;
            _meanTransitionSeconds = defaults.MeanTransitionSeconds;
            _harmonicAffinity = defaults.HarmonicAffinity;
            _moodWeights.Clear();
            _samples = 0;
            _lastLoadedTrackId = null;
            _lastTransitionSetPos = 0;
            _store.Save(SnapshotNoLock());
        }
    }

    private StyleSnapshot SnapshotNoLock() => new()
    {
        EnergyTarget = _energyTarget,
        TempoCenter = _tempoCenter,
        MeanTransitionSeconds = _meanTransitionSeconds,
        HarmonicAffinity = _harmonicAffinity,
        MoodWeights = new Dictionary<string, double>(_moodWeights),
        Samples = _samples,
    };

    private static double Ewma(double current, double sample, double alpha) =>
        ((1 - alpha) * current) + (alpha * sample);
}
