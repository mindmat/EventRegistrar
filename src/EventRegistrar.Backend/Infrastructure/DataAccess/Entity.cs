using System.ComponentModel.DataAnnotations;

namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public class Entity
{
    [Key] public Guid Id { get; set; }

    [Timestamp] public byte[] RowVersion { get; set; }
}