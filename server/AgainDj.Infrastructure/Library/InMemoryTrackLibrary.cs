using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;

namespace AgainDj.Infrastructure.Library;

/// <summary>
/// The default demo crate. Titles and metadata are original and licence-safe;
/// no audio is bundled (the client synthesizes each groove from bpm/key).
/// </summary>
public sealed class InMemoryTrackLibrary : ITrackLibrary
{
    private static readonly IReadOnlyList<Track> Crate =
    [
        new() { Id = "dawn-chorus", Title = "Dawn Chorus", Artist = "agAIn", Bpm = 120, Key = new(8, 'A'), Energy = 0.55, Moods = ["emotional", "uplifting", "vocal", "house"] },
        new() { Id = "ravehold", Title = "Ravehold", Artist = "agAIn", Bpm = 126, Key = new(4, 'A'), Energy = 0.80, Moods = ["euphoric", "peak", "dancing", "build"] },
        new() { Id = "concrete", Title = "Concrete", Artist = "agAIn", Bpm = 140, Key = new(9, 'A'), Energy = 0.92, Moods = ["hard", "peak", "energy", "bass"] },
        new() { Id = "petrichor", Title = "Petrichor", Artist = "agAIn", Bpm = 110, Key = new(5, 'A'), Energy = 0.35, Moods = ["chill", "emotional", "downtempo", "ambient"] },
        new() { Id = "streetlight", Title = "Streetlight", Artist = "agAIn", Bpm = 132, Key = new(6, 'A'), Energy = 0.70, Moods = ["garage", "dancing", "bouncy", "night"] },
        new() { Id = "afterglow", Title = "Afterglow", Artist = "agAIn", Bpm = 100, Key = new(7, 'A'), Energy = 0.25, Moods = ["ambient", "calm", "reflective", "warmup"] },
    ];

    private readonly Dictionary<string, Track> _byId;

    public InMemoryTrackLibrary()
    {
        _byId = Crate.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Track> All => Crate;

    public Track? Get(string id) => _byId.GetValueOrDefault(id);
}
