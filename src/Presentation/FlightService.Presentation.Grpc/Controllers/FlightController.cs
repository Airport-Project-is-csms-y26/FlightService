using Flights.FlightsService.Contracts;
using FlightService.Application.Contracts.Flight;
using FlightService.Application.Contracts.Flight.Operations;
using FlightService.Presentation.Grpc.Util;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Diagnostics;
using Flight = FlightService.Application.Models.Flights.Flight;

namespace FlightService.Presentation.Grpc.Controllers;

public class FlightController : FlightsService.FlightsServiceBase
{
    private readonly IFlightService _flightService;

    public FlightController(IFlightService flightService)
    {
        _flightService = flightService;
    }

    public override async Task<CreateFlightResponse> Create(CreateFlightRequest request, ServerCallContext context)
    {
        CreateFlight.Result result = await _flightService.CreateFlight(
            new CreateFlight.Request(
                request.From,
                request.To,
                request.PlaneNumber,
                request.DepartureTime.ToDateTimeOffset()),
            context.CancellationToken);

        return result switch
        {
            CreateFlight.Result.InvalidDepartureTime invalidDepartureTime => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Depature time in the past")),

            CreateFlight.Result.InvalidPlaneNumber invalidPlaneNumber => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Negative plane number")),

            CreateFlight.Result.Success success => new CreateFlightResponse(),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<ChangeFlightStatusResponse> ChangeFlightStatus(
        ChangeFlightStatusRequest request,
        ServerCallContext context)
    {
        ChangeFlightStatus.Result result =
            await _flightService.ChangeStatus(
                new ChangeFlightStatus.Request(request.FlightId, FlightStatusMapper.ToFlightStatus(request.Status)),
                context.CancellationToken);

        return result switch
        {
            ChangeFlightStatus.Result.Success success => new ChangeFlightStatusResponse(),

            ChangeFlightStatus.Result.NotFound notFound => throw new RpcException(new Status(
                StatusCode.NotFound,
                "FlightNotFound")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<GetFlightsResponse> GetFlights(GetFlightsRequest request, ServerCallContext context)
    {
        var query = new GetFlights(
            request.PageSize,
            request.Cursor,
            request.FlightsIds.ToArray());

        IAsyncEnumerable<Flight> flights = _flightService.GetFlights(query, context.CancellationToken);

        var reply = new GetFlightsResponse();
        await foreach (Flight flight in flights)
        {
            reply.Flights.Add(new Flights.FlightsService.Contracts.Flight
            {
                FlightId = flight.Id,
                From = flight.From,
                To = flight.To,
                DepartureTime = Timestamp.FromDateTime(flight.DepartureTime.UtcDateTime),
                PlaneNumber = flight.PlaneNumber,
                Status = FlightStatusMapper.ToProtoFlightStatus(flight.Status),
            });
        }

        return reply;
    }
}