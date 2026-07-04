using AgainDj.Domain.Model;

namespace AgainDj.Domain.Abstractions;

/// <summary>The crate of available tracks.</summary>
public interface ITrackLibrary
{
    IReadOnlyList<Track> All { get; }

    Track? Get(string id);
}
