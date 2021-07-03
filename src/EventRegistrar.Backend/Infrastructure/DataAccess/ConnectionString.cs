namespace EventRegistrar.Backend.Infrastructure.DataAccess
{
    public class ConnectionString
    {
        private readonly string _connectionString;

        public ConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override string ToString()
        {
            return _connectionString;
        }
    }
}