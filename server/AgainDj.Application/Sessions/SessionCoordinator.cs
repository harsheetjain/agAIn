using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;

namespace AgainDj.Application.Sessions;

/// <summary>
/// Owns the human/AI hand-off. A human action switches control to the human and
/// arms an idle timer; once the console is untouched for the idle timeout, the
/// AI resumes driving.
/// </summary>
public sealed class SessionCoordinator : ISessionCoordinator
{
    private readonly TimeSpan _idleTimeout;
    private readonly Lock _gate = new();

    private DateTimeOffset _lastHumanActivity = DateTimeOffset.MinValue;
    private SessionMode _mode = SessionMode.Autonomous;

    public SessionCoordinator(TimeSpan idleTimeout)
    {
        _idleTimeout = idleTimeout;
    }

    public event Action<SessionMode>? ModeChanged;

    public SessionMode Mode
    {
        get { lock (_gate) { return _mode; } }
    }

    public void NoteHumanActivity(DateTimeOffset now)
    {
        lock (_gate)
        {
            _lastHumanActivity = now;
        }

        SetMode(SessionMode.Human);
    }

    public void ReleaseToAi(DateTimeOffset now)
    {
        lock (_gate)
        {
            _lastHumanActivity = DateTimeOffset.MinValue;
        }

        SetMode(SessionMode.Autonomous);
    }

    public bool ShouldAiDrive(DateTimeOffset now)
    {
        bool aiDrives;
        lock (_gate)
        {
            aiDrives = now - _lastHumanActivity >= _idleTimeout;
        }

        SetMode(aiDrives ? SessionMode.Autonomous : SessionMode.Human);
        return aiDrives;
    }

    private void SetMode(SessionMode target)
    {
        bool changed;
        lock (_gate)
        {
            changed = _mode != target;
            _mode = target;
        }

        if (changed)
        {
            ModeChanged?.Invoke(target);
        }
    }
}
