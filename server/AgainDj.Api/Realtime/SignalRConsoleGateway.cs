using AgainDj.Api.Contracts;
using AgainDj.Api.Hubs;
using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;
using Microsoft.AspNetCore.SignalR;

namespace AgainDj.Api.Realtime;

/// <summary>SignalR implementation of the domain's output port.</summary>
public sealed class SignalRConsoleGateway : IConsoleGateway
{
    private readonly IHubContext<DjHub, IDjClient> _hub;

    public SignalRConsoleGateway(IHubContext<DjHub, IDjClient> hub) => _hub = hub;

    public Task BroadcastActionAsync(DjAction action, CancellationToken ct = default) =>
        _hub.Clients.All.OnAction(action);

    public Task BroadcastStateAsync(MixerState state, CancellationToken ct = default) =>
        _hub.Clients.All.OnState(state);

    public Task BroadcastStyleAsync(StyleSnapshot style, CancellationToken ct = default) =>
        _hub.Clients.All.OnStyle(style);
}
