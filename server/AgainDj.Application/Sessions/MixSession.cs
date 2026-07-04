using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Mixing;
using AgainDj.Domain.Model;

namespace AgainDj.Application.Sessions;

/// <summary>
/// The live runtime for one console. It holds the authoritative
/// <see cref="MixerState"/>, advances playback, runs the autonomous DJ loop when
/// idle, applies human actions (handing control back to the human), and feeds
/// the "listen → sample → train" loop. All state mutation is serialized through
/// a single async mutex so the SignalR hub and the background loop can't race.
/// </summary>
public sealed class MixSession
{
    private readonly IMixingPolicy _policy;
    private readonly IStyleTrainer _trainer;
    private readonly IConsoleGateway _gateway;
    private readonly ISessionCoordinator _coordinator;
    private readonly ITrackLibrary _library;
    private readonly IAudioAnalyzer _analyzer;
    private readonly IClock _clock;
    private readonly SemaphoreSlim _mutex = new(1, 1);

    private MixerState _state;
    private DateTimeOffset _lastTick;
    private DateTimeOffset _setStart;
    private DateTimeOffset _nextDecisionAt;
    private long _ticks;

    public MixSession(
        IMixingPolicy policy,
        IStyleTrainer trainer,
        IConsoleGateway gateway,
        ISessionCoordinator coordinator,
        ITrackLibrary library,
        IAudioAnalyzer analyzer,
        IClock clock)
    {
        _policy = policy;
        _trainer = trainer;
        _gateway = gateway;
        _coordinator = coordinator;
        _library = library;
        _analyzer = analyzer;
        _clock = clock;

        _state = MixerState.Initial(DefaultPads());
        var now = clock.UtcNow;
        _lastTick = now;
        _setStart = now;
        _nextDecisionAt = now;
    }

    public MixerState State => _state;

    public StyleSnapshot Style => _trainer.Snapshot;

    private double SetPositionSeconds => (_clock.UtcNow - _setStart).TotalSeconds;

    /// <summary>Push the current state + style to a (re)connecting console.</summary>
    public async Task PublishInitialAsync(CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            await _gateway.BroadcastStateAsync(_state, ct);
            await _gateway.BroadcastStyleAsync(_trainer.Snapshot, ct);
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>One loop iteration: advance playback and, when idle, let the AI act.</summary>
    public async Task TickAsync(CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            var now = _clock.UtcNow;
            var elapsed = now - _lastTick;
            _lastTick = now;
            if (elapsed > TimeSpan.Zero)
            {
                _state = MixerReducer.Advance(_state, elapsed);
            }

            var aiDrives = _coordinator.ShouldAiDrive(now);
            var mode = aiDrives ? SessionMode.Autonomous : SessionMode.Human;
            if (_state.Mode != mode)
            {
                _state = _state with { Mode = mode };
            }

            if (aiDrives && now >= _nextDecisionAt)
            {
                var decision = _policy.Decide(BuildContext());
                foreach (var action in decision.Actions)
                {
                    _state = MixerReducer.Apply(_state, action, _library);
                    await _gateway.BroadcastActionAsync(action, ct);
                }

                _nextDecisionAt = now + decision.NextDecisionDelay;
            }

            await _gateway.BroadcastStateAsync(_state, ct);
            if (_ticks++ % 10 == 0)
            {
                await _gateway.BroadcastStyleAsync(_trainer.Snapshot, ct);
            }
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>Apply an action the human performed on the console.</summary>
    public async Task ApplyHumanActionAsync(DjAction action, CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            _coordinator.NoteHumanActivity(_clock.UtcNow);
            var human = action with { Actor = Actor.Human };
            _state = MixerReducer.Apply(_state, human, _library);
            if (_state.Mode != SessionMode.Human)
            {
                _state = _state with { Mode = SessionMode.Human };
            }

            _trainer.ObserveEvent(new SessionEvent { Action = human, Context = SnapshotForTraining() });

            await _gateway.BroadcastActionAsync(human, ct);
            await _gateway.BroadcastStateAsync(_state, ct);
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>Hand control back to the AI immediately.</summary>
    public async Task ReleaseToAiAsync(CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        try
        {
            var now = _clock.UtcNow;
            _coordinator.ReleaseToAi(now);
            _nextDecisionAt = now;
            _state = _state with { Mode = SessionMode.Autonomous };
            await _gateway.BroadcastStateAsync(_state, ct);
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <summary>Feed one audio feature frame from the live listening loop into training.</summary>
    public void IngestFeatureFrame(AudioFeatureFrame frame)
    {
        _analyzer.Push(frame);
        _trainer.ObserveFeatureFrame(frame);
    }

    public async Task ResetStyleAsync(CancellationToken ct = default)
    {
        _trainer.Reset();
        await _gateway.BroadcastStyleAsync(_trainer.Snapshot, ct);
    }

    private MixContext BuildContext() => new()
    {
        State = _state,
        Library = _library.All,
        Style = _trainer.Snapshot,
        SetPositionSeconds = SetPositionSeconds,
        RecentRms = _analyzer.CurrentRms,
    };

    private MixerSnapshot SnapshotForTraining() => new()
    {
        EnergyA = _state.DeckA.Track?.Energy ?? 0,
        EnergyB = _state.DeckB.Track?.Energy ?? 0,
        Crossfader = _state.Crossfader,
        TempoA = _state.DeckA.Tempo,
        TempoB = _state.DeckB.Tempo,
        SetPositionSeconds = SetPositionSeconds,
    };

    private static IReadOnlyList<SamplerPad> DefaultPads() =>
    [
        new(1, "Kick"),
        new(2, "Clap"),
        new(3, "Riser"),
        new(4, "Vox"),
        new(5, "Perc"),
        new(6, "FX"),
        new(7, "Sub"),
        new(8, "Snare"),
    ];
}
