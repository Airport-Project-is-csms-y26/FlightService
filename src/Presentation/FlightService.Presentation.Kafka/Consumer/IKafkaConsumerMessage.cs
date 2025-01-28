namespace FlightService.Presentation.Kafka.Consumer;

public interface IKafkaConsumerMessage<out TKey, out TValue>
{
    public TKey Key { get; }

    public TValue Value { get; }
}