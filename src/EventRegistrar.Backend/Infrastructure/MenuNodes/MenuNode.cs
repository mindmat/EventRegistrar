using EventRegistrar.Backend.Events;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Infrastructure.MenuNodes;

public class MenuNodeReadModel : Entity
{
    public required Guid EventId { get; set; }
    public Event? Event { get; set; }
    public required MenuNodeKey Key { get; set; }

    public string? Content { get; set; }
    public bool Hidden { get; set; }
    public MenuNodeStyle? Style { get; set; }
}

public enum MenuNodeKey
{
    Overview = 1,
    PendingMails = 2,
    AssignPartners = 3,
    MailTracking = 4,
    Remarks = 5,
    InternalNotes = 6,
    Cancellations = 7,
    Hosting = 8,
    Participants = 9,

    BankStatements = 11,
    AssignBankStatements = 12,
    DuePayments = 13,
    PaymentDifferences = 14,
    Repayments = 15,

    Settings = 21,
    PrepareEvent = 22,
    MailTemplates = 23,
    BulkMailTemplates = 24,
    Forms = 25,
    Pricing = 26,
}

public enum MenuNodeStyle
{
    None = 0,
    Info = 1,
    ToDo = 2
}

public class MenuNodeReadModelMap : EntityMap<MenuNodeReadModel>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MenuNodeReadModel> builder)
    {
        builder.ToTable("MenuNodeReadModels");

        builder.HasOne(mnd => mnd.Event)
               .WithMany()
               .HasForeignKey(mnd => mnd.EventId);

        builder.Property(mnd => mnd.Key)
               .HasMaxLength(50);

        builder.Property(mnd => mnd.Content)
               .HasMaxLength(20);

        builder.HasIndex(mnd => new
                                {
                                    mnd.EventId,
                                    mnd.Key
                                })
               .IsUnique();
    }
}