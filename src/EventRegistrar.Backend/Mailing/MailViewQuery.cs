using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Mailing.Send;

namespace EventRegistrar.Backend.Mailing;

public class MailViewQuery : IRequest<MailView>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid MailId { get; set; }
}

public class MailViewQueryHandler(IQueryable<Mail> mails,
                                  IQueryable<ImportedMail> importedMails) : IRequestHandler<MailViewQuery, MailView>
{
    public async Task<MailView> Handle(MailViewQuery query, CancellationToken cancellationToken)
    {
        return await mails.Where(mail => mail.EventId == query.EventId
                                      && mail.Id == query.MailId)
                          .Select(mail => new MailView
                                          {
                                              Id = mail.Id,
                                              From = new EmailAddress
                                                     {
                                                         Email = mail.SenderMail ?? "??",
                                                         Name = mail.SenderName ?? "??"
                                                     },
                                              RecipientsEmails = mail.Recipients,
                                              RecipientsNames = mail.Registrations!.Select(reg => $"{reg.Registration!.RespondentFirstName} {reg.Registration.RespondentLastName}")
                                                                    .StringJoin(", "),
                                              Recipients = mail.Registrations!.Select(reg => new MailRecipient
                                                                                             {
                                                                                                 RegistrationId = reg.RegistrationId,
                                                                                                 Name = $"{reg.Registration!.RespondentFirstName} {reg.Registration.RespondentLastName}"
                                                                                             }),
                                              Subject = mail.Subject,
                                              Content = mail.ContentHtml,
                                              Created = mail.Created
                                          })
                          .FirstOrDefaultAsync(cancellationToken)
            ?? await importedMails.Where(mail => mail.EventId == query.EventId
                                              && mail.Id == query.MailId)
                                  .Select(mail => new MailView
                                                  {
                                                      Id = mail.Id,
                                                      From = new EmailAddress
                                                             {
                                                                 Email = mail.SenderMail ?? "??",
                                                                 Name = mail.SenderName ?? "??"
                                                             },
                                                      RecipientsEmails = mail.Recipients,
                                                      RecipientsNames = mail.Registrations!.Select(reg => $"{reg.Registration!.RespondentFirstName} {reg.Registration.RespondentLastName}")
                                                                            .StringJoin(", "),
                                                      Recipients = mail.Registrations!.Select(reg => new MailRecipient
                                                                                                     {
                                                                                                         RegistrationId = reg.RegistrationId,
                                                                                                         Name = $"{reg.Registration!.RespondentFirstName} {reg.Registration.RespondentLastName}"
                                                                                                     }),
                                                      Subject = mail.Subject,
                                                      Content = mail.ContentHtml ?? mail.ContentPlainText,
                                                      Created = mail.Date
                                                  })
                                  .FirstAsync(cancellationToken);
    }
}

public class MailView
{
    public Guid Id { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public string? RecipientsEmails { get; set; }
    public string? RecipientsNames { get; set; }

    public EmailAddress From { get; set; } = null!;
    public DateTimeOffset Created { get; set; }
    public IEnumerable<MailRecipient>? Recipients { get; set; }
}

public class MailRecipient
{
    public Guid RegistrationId { get; set; }
    public string Name { get; set; } = null!;
}