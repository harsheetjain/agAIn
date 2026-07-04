using AgainDj.Application.Sessions;

namespace AgainDj.Api.Hosting;

/// <summary>Drives the autonomous DJ loop on a fixed cadence.</summary>
public sealed class AutonomousLoopService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(180);

    private readonly MixSession _session;
    private readonly ILogger<AutonomousLoopService> _logger;

    public AutonomousLoopService(MixSession session, ILogger<AutonomousLoopService> logger)
    {
        _session = session;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await _session.TickAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Autonomous tick failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
    }
}
