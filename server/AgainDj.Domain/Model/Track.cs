namespace AgainDj.Domain.Model;

/// <summary>An immutable track in the crate. Original, licence-safe metadata only.</summary>
public sealed record Track
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required string Artist { get; init; }

    public required double Bpm { get; init; }

    public required CamelotKey Key { get; init; }

    /// <summary>Perceived energy, 0..1.</summary>
    public required double Energy { get; init; }

    public IReadOnlyList<string> Moods { get; init; } = [];

    /// <summary>
    /// Optional audio URL. When null the client synthesizes a groove from
    /// <see cref="Bpm"/>/<see cref="Key"/>, so no copyrighted audio is bundled.
    /// </summary>
    public string? Src { get; init; }

    /// <summary>Loop/track length in seconds.</summary>
    public double DurationSeconds { get; init; } = 180;
}
