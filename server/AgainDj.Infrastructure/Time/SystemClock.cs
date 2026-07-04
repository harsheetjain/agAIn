using AgainDj.Domain.Abstractions;
using AgainDj.Domain.Model;

namespace AgainDj.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
