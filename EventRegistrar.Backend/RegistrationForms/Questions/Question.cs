using System;
using System.Collections.Generic;

using EventRegistrar.Backend.Infrastructure.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions
{
    public class Question : Entity
    {
        public Guid RegistrationFormId { get; set; }
        public RegistrationForm? RegistrationForm { get; set; }

        public int ExternalId { get; set; }
        public int Index { get; set; }
        public ICollection<QuestionOption>? QuestionOptions { get; set; }
        public string? TemplateKey { get; set; }

        public QuestionType Type { get; set; }
        public string Title { get; set; } = null!;
        public string? Section { get; set; }
    }

    public class QuestionMap : EntityTypeConfiguration<Question>
    {
        public override void Configure(EntityTypeBuilder<Question> builder)
        {
            base.Configure(builder);
            builder.ToTable("Questions");

            builder.HasOne(que => que.RegistrationForm)
                   .WithMany(frm => frm!.Questions)
                   .HasForeignKey(que => que.RegistrationFormId);
        }
    }
}