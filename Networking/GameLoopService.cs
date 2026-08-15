using BoxingGame.Domain.Match;

namespace BoxingGame.Networking;

public class GameLoopService : BackgroundService
{
    private const int TargetFps = 60;
    private readonly MatchRegistry _registry;
    private readonly ILogger<GameLoopService> _logger;

    public GameLoopService(MatchRegistry registry, ILogger<GameLoopService> logger)
    {
        _registry = registry;
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMilliseconds(1000.0 / TargetFps);
        var last     = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var dt  = now - last;
            last    = now;

            try
            {
                foreach (var match in _registry.AllMatches())
                    match.Tick(dt);

                _registry.RemoveFinished();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in game loop tick");
            }

            var elapsed = DateTime.UtcNow - now;
            var delay   = interval - elapsed;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);
        }
    }
}
