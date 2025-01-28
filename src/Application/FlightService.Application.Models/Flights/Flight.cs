namespace FlightService.Application.Models.Flights;

public record Flight(
    long Id,
    string From,
    string To,
    long PlaneNumber,
    FlightStatus Status,
    DateTimeOffset DepartureTime);