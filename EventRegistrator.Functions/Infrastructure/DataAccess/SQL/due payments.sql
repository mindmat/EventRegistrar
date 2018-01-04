select Price, ASS.Summe, LastMail, Subject, *
from Registrations REG
  LEFT JOIN (SELECT RegistrationId, Summe = SUM(Amount)
             FROM dbo.PaymentAssignments 
		     GROUP BY RegistrationId) ASS ON ASS.RegistrationId = REG.Id
  LEFT JOIN (SELECT MAP.RegistrationID, LastMail = MAX(MAI.Created), Subject = MAX(MAI.Subject)
             FROM dbo.Mails MAI
			   INNER JOIN dbo.MailToRegistrations MAP ON MAP.MailId = MAI.ID
			 GROUP BY MAP.RegistrationId) MAIL ON MAIL.RegistrationId = REG.Id
where IsWaitingList = 0
  and State = 1
order by MAIL.LastMail

select sum(price)
from Registrations
where IsWaitingList = 0
  and State = 1
