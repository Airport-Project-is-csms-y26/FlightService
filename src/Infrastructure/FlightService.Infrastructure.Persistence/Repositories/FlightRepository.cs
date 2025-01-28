using FlightService.Application.Abstractions.Persistence.Queries;
using FlightService.Application.Abstractions.Persistence.Repositories;
using FlightService.Application.Models.Flights;
using Npgsql;
using System.Runtime.CompilerServices;

namespace FlightService.Infrastructure.Persistence.Repositories;

public class FlightRepository : IFlightRepository
{
    private readonly NpgsqlDataSource _npgsqlDataSource;

    public FlightRepository(NpgsqlDataSource dataSource)
    {
        _npgsqlDataSource = dataSource;
    }

    public async Task<long> CreateFlight(
        CreateFlightQuery query,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO flights (flight_from, flight_to, flight_plane_number, flight_status, flight_departure_time)
                           VALUES (@FlightFrom, @FlightTo, @FlightPlaneNumber, 'scheduled', @DepartureTime)
                           RETURNING flight_id;
                           """;

        await using NpgsqlConnection connection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("FlightFrom", query.From),
                new NpgsqlParameter("FlightTo", query.To),
                new NpgsqlParameter("FlightPlaneNumber", query.PlaneNumber),
                new NpgsqlParameter("DepartureTime", query.DepartureTime),
            },
        };

        await using NpgsqlDataReader dataReader = await command.ExecuteReaderAsync(cancellationToken);
        await dataReader.ReadAsync(cancellationToken);
        long id = dataReader.GetInt64(0);

        return id;
    }

    public async Task ChangeStatus(long id, FlightStatus status, CancellationToken cancellationToken)
    {
        const string sql = """
                           UPDATE flights
                           SET flight_status = @State
                           WHERE flight_id = @Id;
                           """;

        await using NpgsqlConnection connection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("State", status),
                new NpgsqlParameter("Id", id),
            },
        };
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Flight?> GetFlightById(long id, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT  flight_id,
                                   flight_from,
                                   flight_to,
                                   flight_plane_number,
                                   flight_status,
                                   flight_departure_time
                           FROM flights
                           WHERE flight_id = @Id;
                           """;

        await using NpgsqlConnection connection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("Id", id),
            },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Flight(
            reader.GetInt64(reader.GetOrdinal("flight_id")),
            reader.GetString(reader.GetOrdinal("flight_from")),
            reader.GetString(reader.GetOrdinal("flight_to")),
            reader.GetInt64(reader.GetOrdinal("flight_plane_number")),
            reader.GetFieldValue<FlightStatus>(reader.GetOrdinal("flight_status")),
            reader.GetDateTime(reader.GetOrdinal("flight_departure_time")));
    }

    public async IAsyncEnumerable<Flight> GetFlights(
        GetFlightsQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT  flight_id,
                                   flight_from,
                                   flight_to,
                                   flight_plane_number,
                                   flight_status,
                                   flight_departure_time
                           FROM flights
                           WHERE (flight_id > @Cursor)
                           AND (cardinality(@Ids) = 0 OR flight_id = any(@Ids))
                           LIMIT @Limit;
                           """;

        await using NpgsqlConnection connection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("Cursor", query.Cursor),
                new NpgsqlParameter("Limit", query.PageSize),
                new NpgsqlParameter("Ids", query.Ids),
            },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Flight(
                reader.GetInt64(reader.GetOrdinal("flight_id")),
                reader.GetString(reader.GetOrdinal("flight_from")),
                reader.GetString(reader.GetOrdinal("flight_to")),
                reader.GetInt64(reader.GetOrdinal("flight_plane_number")),
                reader.GetFieldValue<FlightStatus>(reader.GetOrdinal("flight_status")),
                reader.GetDateTime(reader.GetOrdinal("flight_departure_time")));
        }
    }
}