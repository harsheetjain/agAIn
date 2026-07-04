using System.Text.Json;
using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;

namespace AgainDj.Infrastructure.Learning;

/// <summary>Persists the learned style to a JSON file via an atomic write.</summary>
public sealed class JsonStyleStore : IStyleStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path;
    private readonly Lock _gate = new();

    public JsonStyleStore(string path)
    {
        _path = path;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public StyleSnapshot? Load()
    {
        lock (_gate)
        {
            if (!File.Exists(_path))
            {
                return null;
            }

            try
            {
                var dto = JsonSerializer.Deserialize<StyleDto>(File.ReadAllText(_path), Options);
                return dto?.ToSnapshot();
            }
            catch (JsonException)
            {
                return null; // corrupt file — start fresh
            }
        }
    }

    public void Save(StyleSnapshot snapshot)
    {
        lock (_gate)
        {
            var temp = _path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(StyleDto.From(snapshot), Options));
            File.Move(temp, _path, overwrite: true);
        }
    }

    // Concrete DTO so System.Text.Json can round-trip the dictionary
    // (StyleSnapshot exposes IReadOnlyDictionary, which it cannot construct).
    private sealed record StyleDto
    {
        public double EnergyTarget { get; init; }

        public double TempoCenter { get; init; }

        public double MeanTransitionSeconds { get; init; }

        public double HarmonicAffinity { get; init; }

        public Dictionary<string, double> MoodWeights { get; init; } = new();

        public long Samples { get; init; }

        public static StyleDto From(StyleSnapshot s) => new()
        {
            EnergyTarget = s.EnergyTarget,
            TempoCenter = s.TempoCenter,
            MeanTransitionSeconds = s.MeanTransitionSeconds,
            HarmonicAffinity = s.HarmonicAffinity,
            MoodWeights = new Dictionary<string, double>(s.MoodWeights),
            Samples = s.Samples,
        };

        public StyleSnapshot ToSnapshot() => new()
        {
            EnergyTarget = EnergyTarget,
            TempoCenter = TempoCenter,
            MeanTransitionSeconds = MeanTransitionSeconds,
            HarmonicAffinity = HarmonicAffinity,
            MoodWeights = MoodWeights,
            Samples = Samples,
        };
    }
}
