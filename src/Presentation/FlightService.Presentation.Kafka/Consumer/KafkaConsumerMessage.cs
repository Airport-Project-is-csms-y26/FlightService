using Confluent.Kafka;

namespace FlightService.Presentation.Kafka.Consumer;

public class KafkaConsumerMessage<TKey, TValue> : IKafkaConsumerMessage<TKey, TValue>
{
    private readonly IConsumer<TKey, TValue> _consumer;
    private readonly ConsumeResult<TKey, TValue> _result;

    public KafkaConsumerMessage(IConsumer<TKey, TValue> consumer, ConsumeResult<TKey, TValue> result)
    {
        _consumer = consumer;
        _result = result;

        Key = result.Message.Key;
        Value = result.Message.Value;
    }

    public TKey Key { get; }

    public TValue Value { get; }

    public void Commit()
    {
        _consumer.Commit(_result);
    }
}