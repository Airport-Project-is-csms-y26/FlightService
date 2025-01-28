namespace PassengerService.Infrastructure.Persistence.Options;

public class PostgresOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Database { get; set; } = string.Empty;

    public string GetConnectionString()
    {
        return $"Host={Host};Port={Port};User ID={User};Password={Password};Database={Database};";
    }
}