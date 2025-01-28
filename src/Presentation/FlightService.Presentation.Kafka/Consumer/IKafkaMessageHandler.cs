namespace FlightService.Presentation.Kafka.Consumer;

public interface IKafkaMessageHandler<in TKey, in TValue>
{
    Task HandleAsync(
        IEnumerable<IKafkaConsumerMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken);
}