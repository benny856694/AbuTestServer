
namespace WebApplication1
{
    public class UptimeCounterService(Stats stats, ILogger<UptimeCounterService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            while (stoppingToken.IsCancellationRequested is false)
            {
                var now = DateTime.Now;
                logger.LogInformation("Current time: {time}", now);
                stats.CurrentTime = now;
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
