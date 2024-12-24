
namespace WebApplication1
{
    public class UptimeCounterService : IHostedService
    {
        private readonly Stats stats;
        Timer? _timer;

        public UptimeCounterService(Stats stats)
        {
            this.stats = stats;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(UpdateUptime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            return Task.CompletedTask;
        }

        private void UpdateUptime(object? state)
        {
            stats.CurrentTime = DateTime.UtcNow;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
