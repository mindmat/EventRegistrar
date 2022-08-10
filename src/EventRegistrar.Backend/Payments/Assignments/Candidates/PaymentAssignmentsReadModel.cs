using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Registrations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventRegistrar.Backend.Payments.Assignments.Candidates;

public class PaymentAssignmentsReadModel : ReadModel<PaymentAssignments>
{
    public Guid EventId { get; set; }
    public Guid PaymentId { get; set; }
}

public class PaymentAssignmentsReadModelMap : ReadModelMap<PaymentAssignmentsReadModel, PaymentAssignments>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentAssignmentsReadModel> builder)
    {
        builder.HasKey(pas => new
                              {
                                  pas.EventId,
                                  pas.PaymentId
                              })
               .IsClustered(false);
    }
}

public class PaymentAssignments
{
    public decimal OpenAmount { get; set; }
    public PaymentType Type { get; set; }
    public IEnumerable<AssignmentCandidateRegistration>? RegistrationCandidates { get; set; }
    public IEnumerable<ExistingAssignment>? ExistingAssignments { get; set; }
}

public class AssignmentCandidateRegistration
{
    public Guid RegistrationId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public decimal Price { get; set; }
    public bool IsWaitingList { get; set; }

    public bool AmountMatch { get; set; }
    public decimal AmountPaid { get; set; }
    public int MatchScore { get; set; }
    public Guid PaymentId { get; set; }
    public RegistrationState State { get; set; }
}

public class ExistingAssignment
{
    public Guid RegistrationId { get; set; }
    public Guid? PaymentAssignmentId_Existing { get; set; }
    public decimal? AssignedAmount { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public decimal Price { get; set; }
    public bool IsWaitingList { get; set; }

    public Guid PaymentId { get; set; }
}