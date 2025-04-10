using Confluent.Kafka;
using asset_allocation_api.Config;

namespace asset_allocation_api.Service.Producer
{
    /// <summary>
    ///     Wraps a Confluent.Kafka.IProducer instance, and allows for basic
    ///     configuration of this via IConfiguration.
    ///
    ///     KafkaClientHandle does not provide any way for messages to be produced
    ///     directly. Instead, it is a dependency of KafkaDependentProducer. You
    ///     can create more than one instance of KafkaDependentProducer (with
    ///     possibly differing K and V generic types) that leverages the same
    ///     underlying _kafkaProducer instance exposed by the Handle property of this
    ///     class. This is more efficient than creating separate
    ///     Confluent.Kafka.IProducer instances for each Message type you wish to
    ///     produce.
    /// </summary>
    public class KafkaClientHandle : IDisposable
    {
        private readonly IProducer<byte[], byte[]> kafkaProducer;

        public KafkaClientHandle(IConfiguration config)
        {
            ProducerConfig conf = new();
            config.GetSection("Kafka:ProducerSettings").Bind(conf);
            conf.BootstrapServers = AssetAllocationConfig.kafkaBootstrapServers;
            conf.Acks = Acks.Leader;
            conf.MaxInFlight = 1000000;
            conf.Partitioner = Partitioner.ConsistentRandom;
            conf.AllowAutoCreateTopics = true;
            kafkaProducer = new ProducerBuilder<byte[], byte[]>(conf).Build();
        }

        public Handle Handle { get => kafkaProducer.Handle; }

        public void Dispose()
        {
            // Block until all outstanding produce requests have completed (with or
            // without error).
            kafkaProducer.Flush();
            kafkaProducer.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}