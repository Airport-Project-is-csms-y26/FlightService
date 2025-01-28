using FlightService.Application.Contracts.Flight;
using FlightService.Application.Contracts.Flight.Operations;
using FlightService.Application.Models.Flights;
using Tasks.Kafka.Contracts;

namespace FlightService.Presentation.Kafka.Consumer.Handlers;

public class AllTasksDoneHandler : IKafkaMessageHandler<TaskProcessingKey, TaskProcessingValue>
{
    private readonly IFlightService _flightService;

    public AllTasksDoneHandler(IFlightService flightService)
    {
        _flightService = flightService;
    }

    public async Task HandleAsync(IEnumerable<IKafkaConsumerMessage<TaskProcessingKey, TaskProcessingValue>> messages, CancellationToken cancellationToken)
    {
        foreach (IKafkaConsumerMessage<TaskProcessingKey, TaskProcessingValue> message in messages)
        {
            var query = new ChangeFlightStatus.Request(
                message.Value.FlightId,
                FlightStatus.Boarding);
            await _flightService.ChangeStatus(query, cancellationToken);
        }
    }
}