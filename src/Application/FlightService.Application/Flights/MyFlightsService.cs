using FlightService.Application.Abstractions.Persistence.Queries;
using FlightService.Application.Abstractions.Persistence.Repositories;
using FlightService.Application.Contracts.Flight;
using FlightService.Application.Contracts.Flight.Operations;
using FlightService.Application.Models.Flights;
using FlightService.Presentation.Kafka.Producer;
using Google.Protobuf.WellKnownTypes;
using System.Transactions;
using Tasks.Kafka.Contracts;

namespace FlightService.Application.Flights;

public class MyFlightsService : IFlightService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IKafkaProducer<FlightCreationKey, FlightCreationValue> _flightCreationProducer;

    private readonly IKafkaProducer<PassengerNotificationsKey, PassengerNotificationsValue>
        _passengerNotificationsProducer;

    public MyFlightsService(
        IFlightRepository flightRepository,
        IKafkaProducer<FlightCreationKey, FlightCreationValue> flightCreationProducer,
        IKafkaProducer<PassengerNotificationsKey, PassengerNotificationsValue> passengerNotificationsProducer)
    {
        _flightRepository = flightRepository;
        _flightCreationProducer = flightCreationProducer;
        _passengerNotificationsProducer = passengerNotificationsProducer;
    }

    public async Task<CreateFlight.Result> CreateFlight(CreateFlight.Request query, CancellationToken cancellationToken)
    {
        if (query.DepartureTime < DateTimeOffset.Now)
        {
            return new CreateFlight.Result.InvalidDepartureTime();
        }

        if (query.PlaneNumber < 0)
        {
            return new CreateFlight.Result.InvalidPlaneNumber();
        }

        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var createFlightQuery = new CreateFlightQuery(
            query.From,
            query.To,
            query.PlaneNumber,
            query.DepartureTime);
        long flightId = await _flightRepository.CreateFlight(createFlightQuery, cancellationToken);

        var key = new FlightCreationKey() { FlightId = flightId };
        var value = new FlightCreationValue
        {
            FlightId = flightId,
            PlaneNumber = query.PlaneNumber,
            DepartTime = Timestamp.FromDateTimeOffset(query.DepartureTime),
        };
        await _flightCreationProducer.ProduceAsync(key, value, cancellationToken);

        transaction.Complete();
        return new CreateFlight.Result.Success();
    }

    public async Task<ChangeFlightStatus.Result> ChangeStatus(
        ChangeFlightStatus.Request query,
        CancellationToken cancellationToken)
    {
        using var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Flight? flight = await _flightRepository.GetFlightById(query.Id, cancellationToken);
        if (flight == null)
        {
            return new ChangeFlightStatus.Result.NotFound();
        }

        await _flightRepository.ChangeStatus(query.Id, query.Status, cancellationToken);

        var key = new PassengerNotificationsKey
        {
            FlightId = query.Id,
        };
        string message = string.Empty;
        bool flag = false;
        if (query.Status == FlightStatus.Boarding)
        {
            message = "Boarding started";
            flag = true;
        }
        else if (query.Status == FlightStatus.Delayed)
        {
            message = "Flight delayed";
            flag = true;
        }

        if (flag)
        {
            var value = new PassengerNotificationsValue
            {
                FlightId = query.Id,
                Message = message,
                MessageTime = Timestamp.FromDateTimeOffset(DateTimeOffset.Now),
            };

            await _passengerNotificationsProducer.ProduceAsync(key, value, cancellationToken);
        }

        transaction.Complete();
        return new ChangeFlightStatus.Result.Success();
    }

    public IAsyncEnumerable<Flight> GetFlights(GetFlights query, CancellationToken cancellationToken)
    {
        var getFlightsQuery = new GetFlightsQuery(
            query.PageSize,
            query.Cursor,
            query.Ids);
        return _flightRepository.GetFlights(getFlightsQuery, cancellationToken);
    }
}