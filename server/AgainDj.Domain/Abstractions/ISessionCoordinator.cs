using AgainDj.Domain.Model;

namespace AgainDj.Domain.Abstractions;

/// <summary>Owns the human/AI hand-off: humans take control on touch, the AI resumes after idle.</summary>
public interface ISessionCoordinator
{
    SessionMode Mode { get; }

    /// <summary>Record human interaction; switches to Human mode and resets the idle timer.</summary>
    void NoteHumanActivity(DateTimeOffset now);

    /// <summary>Explicitly hand control back to the AI immediately.</summary>
    void ReleaseToAi(DateTimeOffset now);

    /// <summary>True when the AI should be driving at the given instant.</summary>
    bool ShouldAiDrive(DateTimeOffset now);

    event Action<SessionMode>? ModeChanged;
}
