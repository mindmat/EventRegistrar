using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Registrations.Responses;
using EventRegistrar.Backend.Spots;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Registrations;

public class Registration : Entity
{
    public Guid EventId { get; set; }
    public Event? Event { get; set; }

    public Guid RegistrationFormId { get; set; }
    public RegistrationForm? RegistrationForm { get; set; }

    public Guid? RegistrationId_Partner { get; set; }
    public Registration? Registration_Partner { get; set; }

    public DateTime? AdmittedAt { get; set; }
    public ICollection<RegistrationCancellation>? Cancellations { get; set; }
    public string ExternalIdentifier { get; set; } = null!;
    public DateTime ExternalTimestamp { get; set; }
    public bool? FallbackToPartyPass { get; set; }
    public ICollection<ImportedMailToRegistration>? ImportedMails { get; set; }
    public ICollection<IndividualReduction>? IndividualReductions { get; set; }
    public bool IsReduced { get; set; }
    public bool? IsWaitingList { get; set; }
    public string? Language { get; set; }
    public ICollection<MailToRegistration>? Mails { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? PartnerNormalized { get; set; }
    public string? PartnerOriginal { get; set; }
    public ICollection<PaymentAssignment>? Payments { get; set; }
    public string? Phone { get; set; }
    public string? PhoneNormalized { get; set; }
    public decimal? Price { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? Remarks { get; set; }
    public bool RemarksProcessed { get; set; }
    public int ReminderLevel { get; set; }
    public string? RespondentEmail { get; set; }
    public string? RespondentFirstName { get; set; }
    public string? RespondentLastName { get; set; }
    public ICollection<Response>? Responses { get; set; }
    public ICollection<Seat>? Seats_AsFollower { get; set; }
    public ICollection<Seat>? Seats_AsLeader { get; set; }
    public ICollection<Sms>? Sms { get; set; }
    public string? SoldOutMessage { get; set; }
    public RegistrationState State { get; set; }
    public bool WillPayAtCheckin { get; set; }
}

public class RegistrationMap : EntityTypeConfiguration<Registration>
{
    public override void Configure(EntityTypeBuilder<Registration> builder)
    {
        base.Configure(builder);
        builder.ToTable("Registrations");
        builder.HasOne(reg => reg.RegistrationForm)
               .WithMany()
               .HasForeignKey(reg => reg.RegistrationFormId);
        builder.HasOne(reg => reg.Registration_Partner)
               .WithMany()
               .HasForeignKey(reg => reg.RegistrationId_Partner);

        builder.HasOne(reg => reg.Event)
               .WithMany(evt => evt.Registrations)
               .HasForeignKey(reg => reg.EventId);

        builder.Property(reg => reg.PartnerNormalized)
               .HasMaxLength(200);

        builder.Property(reg => reg.PartnerOriginal)
               .HasMaxLength(200);

        builder.Property(reg => reg.RespondentEmail)
               .HasMaxLength(200);

        builder.Property(reg => reg.RespondentFirstName)
               .HasMaxLength(100);

        builder.Property(reg => reg.RespondentLastName)
               .HasMaxLength(100);

        builder.Property(reg => reg.ExternalIdentifier)
               .HasMaxLength(100);

        builder.Property(reg => reg.Phone)
               .HasMaxLength(50);

        builder.Property(reg => reg.PhoneNormalized)
               .HasMaxLength(50);

        builder.Property(reg => reg.Language)
               .HasMaxLength(2);
    }
}