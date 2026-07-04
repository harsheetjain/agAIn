using AgainDj.Api.Contracts;
using AgainDj.Application.Sessions;
using AgainDj.Domain.Model;
using Microsoft.AspNetCore.SignalR;

namespace AgainDj.Api.Hubs;

/// <summary>
/// Realtime channel between the console and the DJ engine. On connect the caller
/// receives the current state and style; thereafter it sends human actions and
/// live audio feature frames, and receives AI actions/state/style broadcasts.
/// </summary>
public sealed class DjHub : Hub<IDjClient>
{
    private readonly MixSession _session;

    public DjHub(MixSession session) => _session = session;

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.OnState(_session.State);
        await Clients.Caller.OnStyle(_session.Style);
        await base.OnConnectedAsync();
    }

    /// <summary>The human moved a control on the console.</summary>
    public Task SendAction(DjAction action) => _session.ApplyHumanActionAsync(action, Context.ConnectionAborted);

    /// <summary>A feature frame from the "listen → sample → train" loop.</summary>
    public void SendFeatureFrame(AudioFeatureFrame frame) => _session.IngestFeatureFrame(frame);

    /// <summary>Hand control back to the AI now.</summary>
    public Task ReleaseToAi() => _session.ReleaseToAiAsync(Context.ConnectionAborted);

    /// <summary>Forget the learned style.</summary>
    public Task ResetStyle() => _session.ResetStyleAsync(Context.ConnectionAborted);
}
