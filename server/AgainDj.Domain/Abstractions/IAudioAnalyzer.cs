using AgainDj.Domain.Model;

namespace AgainDj.Domain.Abstractions;

/// <summary>Aggregates streamed feature frames into running estimates of the live output.</summary>
public interface IAudioAnalyzer
{
    void Push(AudioFeatureFrame frame);

    double CurrentRms { get; }

    double CurrentTempo { get; }

    CamelotKey EstimatedKey { get; }
}
