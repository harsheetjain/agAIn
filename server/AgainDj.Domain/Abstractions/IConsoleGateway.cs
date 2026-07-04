using AgainDj.Domain.Model;

namespace AgainDj.Domain.Abstractions;

/// <summary>
/// Output port to connected consoles. The API implements this over SignalR so
/// the domain/application layers stay transport-agnostic.
/// </summary>
public interface IConsoleGateway
{
    Task BroadcastActionAsync(DjAction action, CancellationToken ct = default);

    Task BroadcastStateAsync(MixerState state, CancellationToken ct = default);

    Task BroadcastStyleAsync(StyleSnapshot style, CancellationToken ct = default);
}
