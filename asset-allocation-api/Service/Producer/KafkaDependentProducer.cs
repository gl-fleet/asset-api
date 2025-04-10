﻿using Confluent.Kafka;

namespace asset_allocation_api.Service.Producer
{
    public class KafkaDependentProducer<K, V>(KafkaClientHandle handle)
    {
        private readonly IProducer<K, V> kafkaHandle = new DependentProducerBuilder<K, V>(handle.Handle).Build();

        /// <summary>
        ///     Asychronously produce a message and expose delivery information
        ///     via the returned Task. Use this method of producing if you would
        ///     like to await the result before flow of execution continues.
        /// <summary>
        public Task ProduceAsync(string topic, Message<K, V> message)
            => kafkaHandle.ProduceAsync(topic, message);

        /// <summary>
        ///     Asynchronously produce a message and expose delivery information
        ///     via the provided callback function. Use this method of producing
        ///     if you would like flow of execution to continue immediately, and
        ///     handle delivery information out-of-band.
        /// </summary>
        public void Produce(string topic, Message<K, V> message, Action<DeliveryReport<K, V>>? deliveryHandler = null)
            => kafkaHandle.Produce(topic, message, deliveryHandler);

        public void Flush(TimeSpan timeout)
            => kafkaHandle.Flush(timeout);
    }
}