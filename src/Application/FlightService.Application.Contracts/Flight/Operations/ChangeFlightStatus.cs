using FlightService.Application.Models.Flights;

namespace FlightService.Application.Contracts.Flight.Operations;

public static class ChangeFlightStatus
{
    public readonly record struct Request(long Id, FlightStatus Status);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success : Result;

        public sealed record NotFound : Result;
    }
}