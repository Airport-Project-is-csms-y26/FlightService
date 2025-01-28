namespace FlightService.Application.Abstractions.Persistence.Queries;

public record CreateFlightQuery(
    string From,
    string To,
    long PlaneNumber,
    DateTimeOffset DepartureTime);