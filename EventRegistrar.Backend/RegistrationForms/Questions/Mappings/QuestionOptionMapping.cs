using System;

using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings
{
    public class QuestionOptionMapping : Entity
    {
        public Guid QuestionOptionId { get; set; }
        public QuestionOption? QuestionOption { get; set; }

        [Obsolete]
        public Guid? QuestionId_Partner { get; set; }
        [Obsolete]
        public Guid? QuestionOptionId_Follower { get; set; }
        [Obsolete]
        public Guid? QuestionOptionId_Leader { get; set; }

        public MappingType? Type { get; set; }
        public string? Language { get; set; }

        public Guid? RegistrableId { get; set; }
        public Registrable? Registrable { get; set; }
    }

    public class QuestionOptionMappingMap : EntityTypeConfiguration<QuestionOptionMapping>
    {
        public override void Configure(EntityTypeBuilder<QuestionOptionMapping> builder)
        {
            base.Configure(builder);
            builder.ToTable("QuestionOptionMappings");

            builder.HasOne(qop => qop.Registrable)
                   .WithMany(qst => qst!.QuestionOptionMappings)
                   .HasForeignKey(qop => qop.RegistrableId);

            builder.HasOne(qop => qop.QuestionOption)
                   .WithMany(qst => qst!.Mappings)
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