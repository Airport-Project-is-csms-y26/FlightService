namespace FlightService.Application.Contracts.Flight.Operations;

public static class CreateFlight
{
    public readonly record struct Request(
        string From,
        string To,
        long PlaneNumber,
        DateTimeOffset DepartureTime);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success : Result;

        public sealed record InvalidDepartureTime : Result;

        public sealed record InvalidPlaneNumber : Result;
    }
}