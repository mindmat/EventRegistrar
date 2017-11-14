select RegistrationId = reg.id, Mail = RespondentEMail, Vorname=  RespondentFirstName, Nachname = rnn.responsestring, Preis = Price, *
from registrations reg
  left join responses rnn on rnn.registrationid = reg.Id and rnn.QuestionId in ('BDDABA24-5A0E-4D42-BF6C-41E742B70D60', 'B726121A-2B10-474E-AE94-C9B8049AB432')
where reg.State = 1