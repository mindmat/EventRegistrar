using System;
using System.ComponentModel.DataAnnotations;

namespace EventRegistrator.Functions.Infrastructure.DataAccess
{
    public class Entity
    {
        [Key]
        public Guid Id { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}