using Confluent.Kafka;
using FlightService.Presentation.Kafka.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceManagement.Presentation.Kafka.Configuration;

namespace FlightService.Presentation.Kafka.Producer;

public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>
{
    private readonly IProducer<TKey, TValue> _producer;
    private readonly string _topic;

    public KafkaProducer(string optionsKey, IServiceProvider provider)
    {
        IOptionsMonitor<ProducerOptions> producerOptionsMonitor =
            provider.GetRequiredService<IOptionsMonitor<ProducerOptions>>();
        ProducerOptions producerOptions = producerOptionsMonitor.Get(optionsKey);
        KafkaOptions kafkaOptions = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;

        ISerializer<TKey> keySerializer = provider.GetRequiredKeyedService<ISerializer<TKey>>(optionsKey);
        ISerializer<TValue> valueSerializer = provider.GetRequiredKeyedService<ISerializer<TValue>>(optionsKey);
        _topic = producerOptions.Topic;

        var config = new ProducerConfig
        {
            BootstrapServers = kafkaOptions.Host,
            SecurityProtocol = kafkaOptions.SecurityProtocol,
            SslCaPem = kafkaOptions.SslCaPem,
            SaslMechanism = kafkaOptions.SaslMechanism,
            SaslUsername = kafkaOptions.SaslUsername,
            SaslPassword = kafkaOptions.SaslPassword,
        };

        _producer = new Confluent.Kafka.ProducerBuilder<TKey, TValue>(config)
            .SetKeySerializer(keySerializer)
            .SetValueSerializer(valueSerializer)
            .Build();
    }

    public async Task ProduceAsync(TKey key, TValue value, CancellationToken cancellationToken)
    {
        try
        {
            await _producer.ProduceAsync(
                _topic,
                new Message<TKey, TValue> { Key = key, Value = value },
                cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to write message to topic {_topic}", ex);
        }
    }
}