using asset_allocation_api.Service.Implementation;

namespace asset_allocation_api.Service.Background
{
    public class Scheduler(SignalRHub chatHub) : BackgroundService
    {
        private readonly SignalRHub signalRHub = chatHub;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new Thread(() => StartConsumerLoop(stoppingToken)).Start();

            return Task.CompletedTask;
        }

        private void StartConsumerLoop(CancellationToken cancellationToken)
        {
            int i = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                Task.Delay(5000, cancellationToken).Wait(cancellationToken);
                signalRHub.SendBroadcast("Broadcast №" + i++, cancellationToken).Wait(cancellationToken);
            }
        }
    }
}