using FlightService.Application.Contracts.Flight;
using FlightService.Application.Flights;
using Microsoft.Extensions.DependencyInjection;

namespace FlightService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection.AddScoped<IFlightService, MyFlightsService>();
        return collection;
    }
}