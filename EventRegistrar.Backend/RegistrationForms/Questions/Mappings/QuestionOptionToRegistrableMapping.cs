using System;

using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings
{
    public class QuestionOptionToRegistrableMapping : Entity
    {
        public Guid QuestionOptionId { get; set; }
        public QuestionOption? QuestionOption { get; set; }

        public Guid? QuestionId_Partner { get; set; }
        public Guid? QuestionOptionId_Follower { get; set; }
        public Guid? QuestionOptionId_Leader { get; set; }
        public MappingType? Type { get; set; }

        public Guid RegistrableId { get; set; }
        public Registrable? Registrable { get; set; }
    }

    public class QuestionOptionToRegistrableMappingMap : EntityTypeConfiguration<QuestionOptionToRegistrableMapping>
    {
        public override void Configure(EntityTypeBuilder<QuestionOptionToRegistrableMapping> builder)
        {
            base.Configure(builder);
            builder.ToTable("QuestionOptionToRegistrableMappings");

            builder.HasOne(qop => qop.Registrable)
                   .WithMany(qst => qst!.QuestionOptionMappings)
                   .HasForeignKey(qop => qop.RegistrableId);

            builder.HasOne(qop => qop.QuestionOption)
                   .WithMany(qst => qst!.Registrables)
                   .HasForeignKey(qop => qop.QuestionOptionId);
        }
    }

    public enum MappingType
    {
        SingleRegistrable = 1,
        //DoubleRegistrable = 2,
        DoubleRegistrableLeader = 3,
        DoubleRegistrableFollower = 4,
        Language = 5,
        Reduction = 6
    }

}