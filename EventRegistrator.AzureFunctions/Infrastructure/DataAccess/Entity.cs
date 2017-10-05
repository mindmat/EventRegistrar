using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace EventRegistrator.Functions.Infrastructure.DataAccess
{
    public class Entity
    {
        public Guid Id { get; set; }
        public byte[] RowVersion { get; set; }
    }
}