namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public class ConnectionString(string connectionString)
{
    public override string ToString()
    {
        return connectionString;
    }
}