using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;

namespace AgainDj.Infrastructure.Audio;

/// <summary>
/// Maintains running (EWMA) estimates of the live output from streamed feature
/// frames: loudness, tempo, and a chroma-derived key estimate.
/// </summary>
public sealed class RunningAudioAnalyzer : IAudioAnalyzer
{
    private const double Alpha = 0.1;

    // pitch class (C=0 .. B=11) -> Camelot number for the minor key at that root.
    private static readonly int[] PitchClassToCamelotMinor = [5, 12, 7, 2, 9, 4, 11, 6, 1, 8, 3, 10];

    private readonly Lock _gate = new();
    private readonly double[] _chroma = new double[12];
    private double _rms;
    private double _tempo;

    public double CurrentRms
    {
        get { lock (_gate) { return _rms; } }
    }

    public double CurrentTempo
    {
        get { lock (_gate) { return _tempo; } }
    }

    public CamelotKey EstimatedKey
    {
        get
        {
            lock (_gate)
            {
                var max = 0.0;
                var index = -1;
                for (var i = 0; i < 12; i++)
                {
                    if (_chroma[i] > max)
                    {
                        max = _chroma[i];
                        index = i;
                    }
                }

                return index < 0 ? new CamelotKey(0, '?') : new CamelotKey(PitchClassToCamelotMinor[index], 'A');
            }
        }
    }

    public void Push(AudioFeatureFrame frame)
    {
        lock (_gate)
        {
            _rms = Ewma(_rms, Math.Clamp(frame.Rms, 0, 1), Alpha);
            if (frame.Tempo is > 40 and < 220)
            {
                _tempo = Ewma(_tempo, frame.Tempo, Alpha);
            }

            if (frame.Chroma.Count == 12)
            {
                for (var i = 0; i < 12; i++)
                {
                    _chroma[i] = Ewma(_chroma[i], frame.Chroma[i], Alpha);
                }
            }
        }
    }

    private static double Ewma(double current, double sample, double alpha) =>
        ((1 - alpha) * current) + (alpha * sample);
}
