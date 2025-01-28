namespace FlightService.Application.Abstractions.Persistence.Queries;

public record GetFlightsQuery(
    int PageSize,
    int Cursor,
    long[] Ids);