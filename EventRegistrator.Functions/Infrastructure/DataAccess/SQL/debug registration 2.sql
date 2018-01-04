declare @registrationids table
(
  id uniqueidentifier
);

insert into @registrationids 
select id
from Registrations
where 1=1
  --and state <> 4
  --and RespondentEmail in( 'solenn.foret@gmail.com', 'zouky-hop@outlook.com')
  --and RespondentEmail in('kellett.stephen@gmail.com', 'monika.ecker@gmx.de')
  --and RespondentEmail in ( 'pavol.stefko@gmail.com','stifisax@gmx.ch','lucetinna@hotmail.com', 'w-stefan@gmx.ch')
  --and RespondentFirstName like 'passer%'
  --and RespondentEmail like '%last%'
  --and id in('bf87454c-c35a-4ba8-b23d-242f331e9fd9')
  --and ExternalIdentifier = '2_ABaOnueyyMvCyfoRdeowyfgi8sDCvVVs5WObREYP3d4Q3jmbk00V4V4lvnyMmp4'
  --or id = 'EBA99BA9-951D-49C1-9CBC-3C54B95A4BC4'
  and id in (select registrationid from Responses where ResponseString like 'leuzinger%')
  
select price, *
from Registrations
where id in (select id from @registrationids)
-- or id = '184821E5-418B-4856-BB11-8349951A317F'

select rbl.Name, *
from Seats seat
  inner join Registrables rbl on rbl.Id = seat.RegistrableId
where (RegistrationId in (select id from @registrationids) or RegistrationId_Follower in (select id from @registrationids))
  --and rbl.MaximumDoubleSeats is not null
order by isnull(seat.RegistrationId, seat.RegistrationId_Follower)

select withhold, *
from Mails mail
  inner join Registrations reg on mail.Recipients like '%'+ reg.RespondentEmail+'%'
where reg.Id in (select id from @registrationids)
order by Created desc

select *
from ReceivedPayments pmt 
  LEFT join PaymentAssignments map on pmt.Id = map.ReceivedPaymentId
where map.RegistrationId in (select id from @registrationids)
  OR PMT.RecognizedEmail in (select RespondentEmail from Registrations where id in (select id from @registrationids))

  /*
select *
from seats
where RegistrableId = '2DBF19B6-6DBD-4050-AC4B-683B9BBA9A98'
  and IsCancelled = 0
order by isWaitingList, FirstPartnerJoined
*/

