using FlightService.Application.Contracts.Flight.Operations;

namespace FlightService.Application.Contracts.Flight;

public interface IFlightService
{
    Task<CreateFlight.Result> CreateFlight(
        CreateFlight.Request query,
        CancellationToken cancellationToken);

    Task<ChangeFlightStatus.Result> ChangeStatus(ChangeFlightStatus.Request query, CancellationToken cancellationToken);

    IAsyncEnumerable<Models.Flights.Flight> GetFlights(GetFlights query, CancellationToken cancellationToken);
}