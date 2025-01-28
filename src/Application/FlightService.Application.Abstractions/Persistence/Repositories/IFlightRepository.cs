using FlightService.Application.Abstractions.Persistence.Queries;
using FlightService.Application.Models.Flights;

namespace FlightService.Application.Abstractions.Persistence.Repositories;

public interface IFlightRepository
{
    Task<long> CreateFlight(
        CreateFlightQuery query,
        CancellationToken cancellationToken);

    Task ChangeStatus(long id, FlightStatus status, CancellationToken cancellationToken);

    Task<Flight?> GetFlightById(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<Flight> GetFlights(GetFlightsQuery query, CancellationToken cancellationToken);
}