using Confluent.Kafka;
using asset_allocation_api.Config;
using asset_allocation_api.Model;
using asset_allocation_api.Service.Implementation;
using System.Text.Json;
using asset_allocation_api.Context;

namespace asset_allocation_api.Service.Consumer
{
    public class AllocationConsumer : BackgroundService
    {
        private readonly string topic;
        private readonly IConsumer<string, string> kafkaConsumer;
        private readonly ILogger<AllocationConsumer> logger;
        private readonly SignalRHub signalRHub;

        public AllocationConsumer(IConfiguration config, ILogger<AllocationConsumer> logger, SignalRHub signalRHub)
        {
            this.logger = logger;
            this.signalRHub = signalRHub;

            ConsumerConfig consumerConfig = new();
            config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
            consumerConfig.BootstrapServers = AssetAllocationConfig.kafkaBootstrapServers;

            consumerConfig.GroupId = AssetAllocationConfig.kafkaConsumerGroupId + "-" + Environment.MachineName.Split("-").Last();
            consumerConfig.EnableAutoCommit = false;
            consumerConfig.AutoOffsetReset = AutoOffsetReset.Latest;
            kafkaConsumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            topic = AssetAllocationConfig.kafkaSocketAllocationTopic;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new Thread(() => StartConsumerLoop(stoppingToken)).Start();

            return Task.CompletedTask;
        }

        private async void StartConsumerLoop(CancellationToken cancellationToken)
        {
            kafkaConsumer.Subscribe(topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<string, string> cr = kafkaConsumer.Consume(cancellationToken);

                    logger.LogDebug("Kafka message received. Key: {}", cr.Message.Key);

                    AssetAllocation assetAlloc = JsonSerializer.Deserialize<AssetAllocation>(cr.Message.Value)!;
                    await signalRHub.SendAllocation(assetAlloc);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    logger.LogError(e, "Consume error.");
                    if (e.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unexpected error.");
                    break;
                }
            }
        }

        public override void Dispose()
        {
            kafkaConsumer.Close(); // Commit offsets and leave the group cleanly.
            kafkaConsumer.Dispose();

            GC.SuppressFinalize(this);
            base.Dispose();
        }
    }
}