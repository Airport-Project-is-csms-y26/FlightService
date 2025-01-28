using Confluent.Kafka;
using FlightService.Presentation.Kafka.Consumer;
using FlightService.Presentation.Kafka.Consumer.Handlers;
using FlightService.Presentation.Kafka.Consumer.Services;
using FlightService.Presentation.Kafka.Producer;
using FlightService.Presentation.Kafka.Tools;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasks.Kafka.Contracts;

namespace FlightService.Presentation.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection collection)
    {
        collection.AddScoped<IKafkaMessageHandler<TaskProcessingKey,
            TaskProcessingValue>, AllTasksDoneHandler>();
        return collection;
    }

    public static IServiceCollection AddKafkaConsumer<TKey, TValue>(
        this IServiceCollection collection,
        string optionsKey) where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        collection.AddKeyedSingleton<IDeserializer<TKey>, ProtobufSerializer<TKey>>(
            optionsKey);
        collection.AddKeyedSingleton<IDeserializer<TValue>, ProtobufSerializer<TValue>>(
            optionsKey);
        collection.AddHostedService(p
            => ActivatorUtilities.CreateInstance<MyBatchingKafkaConsumerService<TKey, TValue>>(p, optionsKey));
        return collection;
    }

    public static IServiceCollection AddKafkaProducer<TKey, TValue>(
        this IServiceCollection collection,
        string optionsKey) where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        collection.AddKeyedSingleton<ISerializer<TKey>, ProtobufSerializer<TKey>>(optionsKey);
        collection.AddKeyedSingleton<ISerializer<TValue>, ProtobufSerializer<TValue>>(optionsKey);
        collection.TryAddScoped<IKafkaProducer<TKey, TValue>>(
            p => ActivatorUtilities
                .CreateInstance<KafkaProducer<TKey, TValue>>(
                    p,
                    optionsKey));

        return collection;
    }
}