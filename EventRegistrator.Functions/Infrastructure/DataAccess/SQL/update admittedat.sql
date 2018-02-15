begin tran

select reg.ReceivedAt, tmp.Sent, *
from dbo.Registrations REG
  INNER JOIN (SELECT map.RegistrationId, sent = MIN(mail.created)
              FROM dbo.Mails mail
			    inner join dbo.MailToRegistrations map on map.MailId = mail.Id
			  where mail.Type in (1,12,11)
			  GROUP By map.RegistrationId) TMP ON TMP.RegistrationId = REG.Id
where REG.IsWaitingList = 0
  and State <> 4
  and reg.AdmittedAt is null

update reg
set AdmittedAt = TMP.sent
--select reg.ReceivedAt, tmp.Sent, *
from dbo.Registrations REG
  INNER JOIN (SELECT map.RegistrationId, sent = MIN(mail.created)
              FROM dbo.Mails mail
			    inner join dbo.MailToRegistrations map on map.MailId = mail.Id
			  where mail.Type in (1,12,11)
			  GROUP By map.RegistrationId) TMP ON TMP.RegistrationId = REG.Id
where REG.IsWaitingList = 0
  and State <> 4
  and reg.AdmittedAt is null
--and RespondentFirstName like 'theresa'

select AdmittedAt, *
from Registrations
order by 1 desc

--commit

rollback