using FlightService.Application.Models.Flights;

namespace FlightService.Presentation.Grpc.Util;

public static class FlightStatusMapper
{
    public static FlightStatus ToFlightStatus(Flights.FlightsService.Contracts.FlightStatus status)
    {
        return status switch
        {
            Flights.FlightsService.Contracts.FlightStatus.StateNone => throw new InvalidCastException("Type is not defined. Possible server error!"),
            Flights.FlightsService.Contracts.FlightStatus.StateScheduled => FlightStatus.Scheduled,
            Flights.FlightsService.Contracts.FlightStatus.StateArrived => FlightStatus.Arrived,
            Flights.FlightsService.Contracts.FlightStatus.StateCancelled => FlightStatus.Cancelled,
            Flights.FlightsService.Contracts.FlightStatus.StateDeparted => FlightStatus.Departed,
            Flights.FlightsService.Contracts.FlightStatus.StateDelayed => FlightStatus.Delayed,
            Flights.FlightsService.Contracts.FlightStatus.StateBoarding => FlightStatus.Boarding,
            _ => throw new InvalidCastException($"Unknown FlightStatus value: {status}"),
        };
    }

    public static Flights.FlightsService.Contracts.FlightStatus ToProtoFlightStatus(FlightStatus status)
    {
        return status switch
        {
            FlightStatus.Scheduled => Flights.FlightsService.Contracts.FlightStatus.StateScheduled,
            FlightStatus.Departed => Flights.FlightsService.Contracts.FlightStatus.StateDeparted,
            FlightStatus.Delayed => Flights.FlightsService.Contracts.FlightStatus.StateDelayed,
            FlightStatus.Cancelled => Flights.FlightsService.Contracts.FlightStatus.StateCancelled,
            FlightStatus.Arrived => Flights.FlightsService.Contracts.FlightStatus.StateArrived,
            FlightStatus.Boarding => Flights.FlightsService.Contracts.FlightStatus.StateBoarding,
            _ => throw new ArgumentOutOfRangeException($"Unknown FlightStatus value: {status}"),
        };
    }
}