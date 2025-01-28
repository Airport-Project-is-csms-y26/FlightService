namespace FlightService.Presentation.Kafka.Producer;

public interface IKafkaProducer<TKey, TValue>
{
    Task ProduceAsync(TKey key, TValue value, CancellationToken cancellationToken);
}