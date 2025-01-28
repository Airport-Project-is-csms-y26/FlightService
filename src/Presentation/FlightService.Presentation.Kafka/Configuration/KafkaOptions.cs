using Confluent.Kafka;

namespace ServiceManagement.Presentation.Kafka.Configuration;

public class KafkaOptions
{
    public string Host { get; init; } = string.Empty;

    public SecurityProtocol SecurityProtocol { get; init; }

    public string? SslCaPem { get; init; }

    public SaslMechanism? SaslMechanism { get; init; }

    public string? SaslUsername { get; init; }

    public string? SaslPassword { get; init; }
}