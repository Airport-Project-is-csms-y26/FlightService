namespace FlightService.Application.Contracts.Flight.Operations;

public record GetFlights(
    int PageSize,
    int Cursor,
    long[] Ids);