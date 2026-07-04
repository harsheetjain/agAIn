using AgainDj.Domain.Model;

namespace AgainDj.Domain.Abstractions;

/// <summary>Read access to the current learned style.</summary>
public interface IStyleModel
{
    StyleSnapshot Snapshot { get; }
}

/// <summary>
/// The online learner behind the "listen → sample → train" loop. It folds audio
/// feature frames and human actions into an interpretable <see cref="StyleSnapshot"/>.
/// </summary>
public interface IStyleTrainer : IStyleModel
{
    /// <summary>Incorporate one audio feature frame captured from the live output.</summary>
    void ObserveFeatureFrame(AudioFeatureFrame frame);

    /// <summary>Incorporate one human action together with the mix context it occurred in.</summary>
    void ObserveEvent(SessionEvent evt);

    /// <summary>Reset the learned style back to defaults.</summary>
    void Reset();
}
