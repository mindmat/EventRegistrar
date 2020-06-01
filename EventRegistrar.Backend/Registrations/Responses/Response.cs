using System;

using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.Questions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.Responses
{
    public class Response : Entity
    {
        public Guid? QuestionId { get; set; }
        public Question? Question { get; set; }

        public Guid RegistrationId { get; set; }
        public Registration? Registration { get; set; }

        public Guid? QuestionOptionId { get; set; }
        public string ResponseString { get; set; } = null!;
        //public QuestionOptionToRegistrableMapping RegistrableMappings { get; set; }
    }

    public class ResponseMap : EntityTypeConfiguration<Response>
    {
        public override void Configure(EntityTypeBuilder<Response> builder)
        {
            base.Configure(builder);
            builder.ToTable("Responses");

            builder.HasOne(rsp => rsp.Question)
                   .WithMany()
                   .HasForeignKey(rsp => rsp.QuestionId);
            builder.HasOne(rsp => rsp.Registration)
                   .WithMany(reg => reg!.Responses)
                   .HasForeignKey(rsp => rsp.RegistrationId);
        }
    }
}