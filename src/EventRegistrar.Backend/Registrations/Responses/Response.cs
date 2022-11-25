using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms.Questions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations.Responses;

public class Response : Entity
{
    public Guid? QuestionId { get; set; }
    public Question? Question { get; set; }
    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    public Guid? QuestionOptionId { get; set; }
    public QuestionOption? QuestionOption { get; set; }

    public string ResponseString { get; set; } = null!;
}

public class ResponseMap : EntityMap<Response>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Response> builder)
    {
        builder.ToTable("Responses");

        builder.HasOne(rsp => rsp.Question)
               .WithMany(qst => qst.Responses)
               .HasForeignKey(rsp => rsp.QuestionId);

        builder.HasOne(rsp => rsp.Registration)
               .WithMany(reg => reg.Responses)
               .HasForeignKey(rsp => rsp.RegistrationId);

        builder.HasOne(rsp => rsp.QuestionOption)
               .WithMany(qso => qso.Responses)
               .HasForeignKey(rsp => rsp.QuestionOptionId);
    }
}