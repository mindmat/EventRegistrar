using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Seats;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Infrastructure.DataAccess
{
    public class EventRegistratorDbContext : DbContext
    {
        private readonly ConnectionString _connectionString;

        public EventRegistratorDbContext(ConnectionString connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString.ToString(), builder =>
             {
                 builder.EnableRetryOnFailure();
             });
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new RegistrableMap());
            builder.ApplyConfiguration(new SeatMap());
        }
    }
}