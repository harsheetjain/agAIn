using AgainDj.Domain.Model;

namespace AgainDj.Api.Contracts;

/// <summary>Strongly-typed SignalR client contract pushed from server to console.</summary>
public interface IDjClient
{
    /// <summary>A single control action (by the AI or a human) to animate on the console.</summary>
    Task OnAction(DjAction action);

    /// <summary>The authoritative console state.</summary>
    Task OnState(MixerState state);

    /// <summary>The current learned style.</summary>
    Task OnStyle(StyleSnapshot style);
}
