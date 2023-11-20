using EventRegistrar.Backend.Mailing.Import;

namespace EventRegistrar.Backend.Mailing;

public class MailsOfRegistrationQuery : IRequest<IEnumerable<MailDisplayItem>>, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class MailsOfRegistrationQueryHandler(IQueryable<MailToRegistration> _mails,
                                             IQueryable<ImportedMailToRegistration> _importedMails)
    : IRequestHandler<MailsOfRegistrationQuery, IEnumerable<MailDisplayItem>>
{
    public async Task<IEnumerable<MailDisplayItem>> Handle(MailsOfRegistrationQuery query,
                                                           CancellationToken cancellationToken)
    {
        var mails = await _mails
                          .Where(mtr => mtr.Registration.EventId == query.EventId
                                     && mtr.RegistrationId == query.RegistrationId
                                     && !mtr.Mail.Discarded)
                          .Select(mtr => new MailDisplayItem
                                         {
                                             Id = mtr.MailId,
                                             Withhold = mtr.Mail.Withhold,
                                             SenderName = mtr.Mail.SenderName,
                                             SenderMail = mtr.Mail.SenderMail,
                                             Recipients = mtr.Mail.Recipients,
                                             Subject = mtr.Mail.Subject,
                                             Created = mtr.Mail.Created,
                                             ContentHtml = mtr.Mail.ContentHtml,
                                             State = mtr.Mail.State,
                                             Events = mtr.Mail.Events.OrderByDescending(mev => mev.Created)
                                                         .Select(mev => new MailEventDisplayItem
                                                                        {
                                                                            When = mev.Created,
                                                                            Email = mev.EMail,
                                                                            State = mev.State,
                                                                            StateText = mev.State.ToString()
                                                                        })
                                         })
                          .ToListAsync(cancellationToken);

        var importedMails = await _importedMails.Where(mtr => mtr.Registration.EventId == query.EventId
                                                           && mtr.RegistrationId == query.RegistrationId)
                                                .Select(mtr => new MailDisplayItem
                                                               {
                                                                   Id = mtr.ImportedMailId,
                                                                   Withhold = false,
                                                                   SenderName = mtr.Mail.SenderName,
                                                                   SenderMail = mtr.Mail.SenderMail,
                                                                   Recipients = mtr.Mail.Recipients,
                                                                   Subject = mtr.Mail.Subject,
                                                                   Created = mtr.Mail.Date,
                                                                   ContentHtml = mtr.Mail.ContentHtml ?? mtr.Mail.ContentPlainText,
                                                                   State = null
                                                               })
                                                .ToListAsync(cancellationToken);

        mails.AddRange(importedMails);
        return mails.OrderByDescending(mail => mail.Created);
    }
}