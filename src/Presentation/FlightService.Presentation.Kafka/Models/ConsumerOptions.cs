namespace FlightService.Presentation.Kafka.Models;

public class ConsumerOptions
{
    public string Topic { get; init; } = string.Empty;

    public string Group { get; init; } = string.Empty;

    public int ParallelismDegree { get; init; } = 1;

    public int ChannelSize { get; init; } = 6;

    public int BatchSize { get; init; } = 3;

    public TimeSpan BufferWaitLimit { get; init; } = TimeSpan.Zero;
}