using FluentMigrator;

namespace FlightService.Infrastructure.Persistence.Migrations;

[Migration(1, "Initial")]
public class InitialMigration : Migration
{
    public override void Up()
    {
        string sql = """
                     create type flight_state as enum ('scheduled', 'boarding', 'departed', 'delayed', 'cancelled', 'arrived');

                     CREATE TABLE Flights (
                          flight_id BIGINT primary key generated always as identity,
                          flight_from VARCHAR(255) NOT NULL,
                          flight_to VARCHAR(255) NOT NULL,
                          flight_plane_number BIGINT not null,
                          flight_status flight_state NOT NULL,
                          flight_departure_time timestamp with time zone not null
                      );
                     """;
        Execute.Sql(sql);
    }

    public override void Down()
    {
        Execute.Sql("""
                    DROP TABLE Flights;
                    DROP TYPE flight_state;
                    """);
    }
}