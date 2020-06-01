using System;
using System.Collections.Generic;

using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.Questions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class RegistrationForm : Entity
    {
        public Guid? EventId { get; set; }
        public Event? Event { get; set; }

        public string ExternalIdentifier { get; set; } = null!;
        public string? Language { get; set; }

        public string? ProcessConfigurationJson { get; set; }
        //public Guid? QuestionId_FirstName { get; set; }
        //public Guid? QuestionId_LastName { get; set; }
        //public Guid? QuestionId_Phone { get; set; }
        //public Guid? QuestionId_Remarks { get; set; }
        public ICollection<Question>? Questions { get; set; }
        public State State { get; set; }
        public string? Title { get; set; }
        public FormType? Type { get; set; }
    }

    public class RegistrationFormMap : EntityTypeConfiguration<RegistrationForm>
    {
        public override void Configure(EntityTypeBuilder<RegistrationForm> builder)
        {
            base.Configure(builder);
            builder.ToTable("RegistrationForms");

            builder.HasOne(frm => frm.Event)
                   .WithMany()
                   .HasForeignKey(frm => frm.EventId);
        }
    }
}