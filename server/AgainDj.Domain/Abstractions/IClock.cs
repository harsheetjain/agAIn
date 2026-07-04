using AgainDj.Domain.Model;

namespace AgainDj.Domain.Abstractions;

/// <summary>Abstraction over the system clock, for deterministic tests.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>Persists the learned style so it survives restarts.</summary>
public interface IStyleStore
{
    StyleSnapshot? Load();

    void Save(StyleSnapshot snapshot);
}
