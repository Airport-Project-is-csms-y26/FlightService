using Confluent.Kafka;
using FlightService.Presentation.Kafka.Models;
using Itmo.Dev.Platform.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ServiceManagement.Presentation.Kafka.Configuration;
using System.Threading.Channels;

namespace FlightService.Presentation.Kafka.Consumer.Services;

public class KafkaMessageProcessor<TKey, TValue>
{
    private readonly Channel<KafkaConsumerMessage<TKey, TValue>> _channel;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ConsumerOptions _consumerOptions;
    private readonly IDeserializer<TKey>? _keyDeserializer;
    private readonly IDeserializer<TValue>? _valueDeserializer;
    private readonly IServiceScopeFactory _scopeFactory;

    public KafkaMessageProcessor(
        KafkaOptions kafkaOptions,
        ConsumerOptions consumerOptions,
        IServiceScopeFactory scopeFactory,
        IDeserializer<TKey>? keyDeserializer,
        IDeserializer<TValue>? valueDeserializer)
    {
        _kafkaOptions = kafkaOptions;
        _consumerOptions = consumerOptions;
        _scopeFactory = scopeFactory;
        _keyDeserializer = keyDeserializer;
        _valueDeserializer = valueDeserializer;

        var options = new BoundedChannelOptions(consumerOptions.ChannelSize)
        {
            SingleReader = consumerOptions.ParallelismDegree is 1,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait,
        };
        _channel = Channel.CreateBounded<KafkaConsumerMessage<TKey, TValue>>(options);
    }

    public async Task ReadFromKafkaAsync(CancellationToken cancellationToken)
    {
        var consumerConfiguration = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.Host,
            SecurityProtocol = _kafkaOptions.SecurityProtocol,
            SslCaPem = _kafkaOptions.SslCaPem,
            SaslMechanism = _kafkaOptions.SaslMechanism,
            SaslUsername = _kafkaOptions.SaslUsername,
            SaslPassword = _kafkaOptions.SaslPassword,
            GroupId = _consumerOptions.Group,
            EnableAutoCommit = false,
        };

        using IConsumer<TKey, TValue>? consumer = new ConsumerBuilder<TKey, TValue>(consumerConfiguration)
            .SetKeyDeserializer(_keyDeserializer)
            .SetValueDeserializer(_valueDeserializer)
            .Build();

        consumer.Subscribe(_consumerOptions.Topic);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = new KafkaConsumerMessage<TKey, TValue>(consumer, consumer.Consume(cancellationToken));
                await _channel.Writer.WriteAsync(message, cancellationToken);
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    public async Task ProcessChannelAsync(CancellationToken cancellationToken)
    {
        await foreach (IReadOnlyList<KafkaConsumerMessage<TKey, TValue>> batch in _channel.Reader
                           .ReadAllAsync(cancellationToken)
                           .ChunkAsync(_consumerOptions.BatchSize, _consumerOptions.BufferWaitLimit)
                           .WithCancellation(cancellationToken))
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            IKafkaMessageHandler<TKey, TValue> handler = scope.ServiceProvider
                .GetRequiredService<IKafkaMessageHandler<TKey, TValue>>();

            KafkaConsumerMessage<TKey, TValue>[] validMessages = batch
                .Where(message => message.Value != null)
                .ToArray();
            await handler.HandleAsync(validMessages, cancellationToken);

            foreach (KafkaConsumerMessage<TKey, TValue> message in validMessages)
            {
                message.Commit();
            }
        }
    }
}