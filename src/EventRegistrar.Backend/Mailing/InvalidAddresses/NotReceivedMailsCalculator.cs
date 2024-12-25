using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.MenuNodes;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class NotReceivedMailsCalculator(IQueryable<MailToRegistration> mails,
                                        EnumTranslator enumTranslator)
    : IReadModelCalculator
{
    public string QueryName => nameof(NotReceivedMailsQuery);
    public bool IsDateDependent => true;

    public async Task<(object ReadModel, MenuNodeCalculation? MenuNode)> Calculate(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        const int maxCount = 100;
        var successStates = new MailState?[] { MailState.Delivered, MailState.Open, MailState.Click };
        var problems = await mails.Where(ml => ml.Registration!.EventId == eventId
                                            && !ml.Mail!.Discarded
                                            && !ml.Mail!.Withhold
                                            && ml.Registration.State != RegistrationState.Cancelled
                                            && !successStates.Contains(ml.State)
                                            && ml.Email != null)
                                  .GroupBy(ml => ml.Email)
                                  .Select(grp => new ProblematicEmail
                                                 {
                                                     RegistrationId = grp.First().RegistrationId,
                                                     ParticipantFirstName = grp.First().Registration!.RespondentFirstName,
                                                     ParticipantLastName = grp.First().Registration!.RespondentLastName,
                                                     Email = grp.Key!,
                                                     Severity = MailDeliverySeverity.NoneSucceeded,
                                                     Mails = grp.OrderByDescending(mtr => mtr.Mail!.Created)
                                                                .Select(mtr => new NotReceivedMail
                                                                               {
                                                                                   MailId = mtr.MailId,
                                                                                   RegistrationId = mtr.RegistrationId,
                                                                                   State = mtr.State,
                                                                                   StateText = enumTranslator.Translate(mtr.State),
                                                                                   Created = mtr.Mail!.Created,
                                                                                   Recipient = mtr.Email,
                                                                                   Sent = mtr.Mail.Sent,
                                                                                   Subject = mtr.Mail.Subject
                                                                               })
                                                 })
                                  .OrderBy(pml => pml.Email)
                                  .Take(maxCount + 1)
                                  .ToListAsync(cancellationToken);

        var succeededMails = await mails.Where(mtr => problems.Select(pml => pml.RegistrationId).Contains(mtr.RegistrationId)
                                                   && mtr.Registration!.EventId == eventId
                                                   && !mtr.Mail!.Discarded
                                                   && !mtr.Mail!.Withhold
                                                   && mtr.Registration.State != RegistrationState.Cancelled
                                                   && mtr.Email != null
                                                   && successStates.Contains(mtr.State))
                                        .GroupBy(mtr => mtr.RegistrationId)
                                        .Select(grp => new
                                                       {
                                                           RegistrationId = grp.Key,
                                                           Mails = grp.Select(mtr => new NotReceivedMail
                                                                                     {
                                                                                         MailId = mtr.MailId,
                                                                                         RegistrationId = mtr.RegistrationId,
                                                                                         State = mtr.State,
                                                                                         StateText = enumTranslator.Translate(mtr.State),
                                                                                         Created = mtr.Mail!.Created,
                                                                                         Recipient = mtr.Email,
                                                                                         Sent = mtr.Mail.Sent,
                                                                                         Subject = mtr.Mail.Subject
                                                                                     })
                                                       })
                                        .ToListAsync(cancellationToken);

        foreach (var problematicEmail in problems)
        {
            var succeeded = succeededMails.FirstOrDefault(mls => mls.RegistrationId == problematicEmail.RegistrationId);
            if (succeeded != null)
            {
                problematicEmail.Mails = problematicEmail.Mails
                                                         .Concat(succeeded.Mails)
                                                         .OrderByDescending(mtr => mtr.Sent ?? mtr.Created)
                                                         .ToList();
                problematicEmail.Severity = successStates.Contains(problematicEmail.Mails.First().State)
                                                ? MailDeliverySeverity.LastSucceeded
                                                : MailDeliverySeverity.SomeSucceeded;
            }
        }

        var node = CalculateNode(problems);
        return (problems, node);
    }

    private MenuNodeCalculation CalculateNode(List<ProblematicEmail> problems)
    {
        var noneSucceededCount = problems.Count(prm => prm.Severity == MailDeliverySeverity.NoneSucceeded);
        var someSucceededCount = problems.Count - noneSucceededCount;
        var node = new MenuNodeCalculation
                   {
                       Key = MenuNodeKey.MailTracking,
                       Style = MenuNodeStyle.None,
                       Content = $"{noneSucceededCount} | {someSucceededCount}"
                   };
        if (noneSucceededCount > 0)
        {
            node.Style = MenuNodeStyle.Important;
        }
        else if (someSucceededCount > 0)
        {
            node.Style = MenuNodeStyle.ToDo;
        }

        return node;
    }
}

public class ProblematicEmail
{
    public string Email { get; set; } = null!;
    public Guid RegistrationId { get; set; }
    public IEnumerable<NotReceivedMail> Mails { get; set; } = null!;
    public string? ParticipantFirstName { get; set; }
    public string? ParticipantLastName { get; set; }
    public MailDeliverySeverity Severity { get; set; }
}

public enum MailDeliverySeverity
{
    NoneSucceeded = 1,
    SomeSucceeded = 2,
    LastSucceeded = 3
}

public class NotReceivedMail
{
    public Guid MailId { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset? Sent { get; set; }
    public string? Recipient { get; set; }
    public Guid RegistrationId { get; set; }
    public MailState? State { get; set; }
    public string? Subject { get; set; }
    public string? StateText { get; set; }
}