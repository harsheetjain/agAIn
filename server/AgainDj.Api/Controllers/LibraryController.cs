using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace AgainDj.Api.Controllers;

[ApiController]
[Route("api/tracks")]
public sealed class LibraryController : ControllerBase
{
    private readonly ITrackLibrary _library;

    public LibraryController(ITrackLibrary library) => _library = library;

    [HttpGet]
    public IReadOnlyList<Track> Get() => _library.All;
}
