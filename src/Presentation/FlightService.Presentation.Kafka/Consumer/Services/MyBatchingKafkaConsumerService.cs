using Confluent.Kafka;
using FlightService.Presentation.Kafka.Models;
using Itmo.Dev.Platform.Common.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceManagement.Presentation.Kafka.Configuration;

namespace FlightService.Presentation.Kafka.Consumer.Services;

public class MyBatchingKafkaConsumerService<TKey, TValue> : RestartableBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly IDeserializer<TKey>? _keyDeserializer;
    private readonly IDeserializer<TValue>? _valueDeserializer;

    private readonly string _optionsKey;

    public MyBatchingKafkaConsumerService(
        string optionsKey,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory)
    {
        _optionsKey = optionsKey;
        _serviceProvider = serviceProvider;
        _scopeFactory = scopeFactory;
        _keyDeserializer = serviceProvider.GetKeyedService<IDeserializer<TKey>>(_optionsKey);
        _valueDeserializer = serviceProvider.GetKeyedService<IDeserializer<TValue>>(_optionsKey);
    }

    protected override async Task ExecuteAsync(CancellationTokenSource cts)
    {
        IOptionsMonitor<ConsumerOptions> consumerOptionsMonitor =
            _serviceProvider.GetRequiredService<IOptionsMonitor<ConsumerOptions>>();
        KafkaOptions kafkaOptions = _serviceProvider.GetRequiredService<IOptions<KafkaOptions>>().Value;
        ConsumerOptions consumerOptions = consumerOptionsMonitor.Get(_optionsKey);

        var processor = new KafkaMessageProcessor<TKey, TValue>(
            kafkaOptions,
            consumerOptions,
            _scopeFactory,
            _keyDeserializer,
            _valueDeserializer);

        Task handlingTask = processor.ProcessChannelAsync(cts.Token);
        Task readingTask = processor.ReadFromKafkaAsync(cts.Token);

        await Task.WhenAll(readingTask, handlingTask);
    }
}