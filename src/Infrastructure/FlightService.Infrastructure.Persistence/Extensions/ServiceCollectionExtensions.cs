using FlightService.Application.Abstractions.Persistence.Repositories;
using FlightService.Application.Models.Flights;
using FlightService.Infrastructure.Persistence.Migrations;
using FlightService.Infrastructure.Persistence.Repositories;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using PassengerService.Infrastructure.Persistence.Options;

namespace FlightService.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMigration(
        this IServiceCollection collection)
    {
        collection
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(serviceProvider =>
                    serviceProvider
                        .GetRequiredService<IOptions<PostgresOptions>>()
                        .Value
                        .GetConnectionString())
                .ScanIn(typeof(InitialMigration).Assembly));

        collection.AddSingleton<NpgsqlDataSource>(provider =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(provider
                .GetRequiredService<IOptions<PostgresOptions>>()
                .Value
                .GetConnectionString());

            dataSourceBuilder.MapEnum<FlightStatus>(pgName: "flight_state");

            return dataSourceBuilder.Build();
        });

        return collection;
    }

    public static IServiceCollection AddInfrastructureDataAccess(this IServiceCollection collection)
    {
        collection.AddScoped<IFlightRepository, FlightRepository>();

        return collection;
    }
}