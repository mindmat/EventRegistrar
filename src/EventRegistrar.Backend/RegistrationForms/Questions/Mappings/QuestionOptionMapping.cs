﻿using EventRegistrar.Backend.Registrables;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class QuestionOptionMapping : Entity
{
    public Guid QuestionOptionId { get; set; }
    public QuestionOption? QuestionOption { get; set; }
    public Guid? RegistrableId { get; set; }
    public Registrable? Registrable { get; set; }
    public MappingType? Type { get; set; }
    public string? Language { get; set; }
}

public class QuestionOptionMappingMap : EntityMap<QuestionOptionMapping>
{
    protected override void ConfigureEntity(EntityTypeBuilder<QuestionOptionMapping> builder)
    {
        builder.ToTable("QuestionOptionMappings");

        builder.HasOne(qop => qop.Registrable)
               .WithMany(qst => qst.QuestionOptionMappings)
               .HasForeignKey(qop => qop.RegistrableId);

        builder.HasOne(qop => qop.QuestionOption)
               .WithMany(qst => qst.Mappings)
               .HasForeignKey(qop => qop.QuestionOptionId);
    }
}

public enum MappingType
{
    SingleRegistrable = 1,
    PartnerRegistrable = 2,
    PartnerRegistrableLeader = 3,
    PartnerRegistrableFollower = 4,
    Language = 5,
    RoleLeader = 7,
    RoleFollower = 8,
    CanSwitchRole = 9,
    HostingOffer = 21,
    HostingRequest = 31,
    HostingRequest_ShareOkWithPartner = 32,
    HostingRequest_ShareOkWithRandom = 33,
    HostingRequest_TravelByCar = 34
}