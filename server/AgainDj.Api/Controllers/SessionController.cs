using AgainDj.Application.Sessions;
using AgainDj.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace AgainDj.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class SessionController : ControllerBase
{
    private readonly MixSession _session;

    public SessionController(MixSession session) => _session = session;

    [HttpGet("state")]
    public MixerState GetState() => _session.State;

    [HttpGet("style")]
    public StyleSnapshot GetStyle() => _session.Style;
}
