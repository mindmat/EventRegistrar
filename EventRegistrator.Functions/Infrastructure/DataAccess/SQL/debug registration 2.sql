declare @registrationids table
(
  id uniqueidentifier
);

insert into @registrationids 
select id
from Registrations
where 1=1
  and state <> 4
  --and RespondentEmail in( 'jonas_huber11@hotmail.com','rebecca.goodman@yahoo.co.uk')
  --and RespondentEmail in('helen.wipfli@gmail.com')
  --and RespondentFirstName like 'andrea.b.empoli@gmail.com%'
  and RespondentEmail like '%FLORIANA.SALERNO%'
  --and id in('bf87454c-c35a-4ba8-b23d-242f331e9fd9')
  --and ExternalIdentifier = '2_ABaOnueyyMvCyfoRdeowyfgi8sDCvVVs5WObREYP3d4Q3jmbk00V4V4lvnyMmp4'
  --or id = 'D5BD659A-E5B8-49F8-B458-0558F6E5CA72'
  
select *
from Registrations
where id in (select id from @registrationids)
-- or id = '184821E5-418B-4856-BB11-8349951A317F'

select*
from Seats seat
  inner join Registrables rbl on rbl.Id = seat.RegistrableId
where (RegistrationId in (select id from @registrationids) or RegistrationId_Follower in (select id from @registrationids))
  --and rbl.MaximumDoubleSeats is not null

select withhold, *
from Mails mail
  inner join Registrations reg on mail.Recipients like '%'+ reg.RespondentEmail+'%'
where reg.Id in (select id from @registrationids)
order by Created desc

select *
from PaymentAssignments map
  inner join ReceivedPayments pmt on pmt.Id = map.ReceivedPaymentId
where map.RegistrationId in (select id from @registrationids)


select *
from seats
where RegistrableId = '2DBF19B6-6DBD-4050-AC4B-683B9BBA9A98'
  and IsCancelled = 0
order by isWaitingList, FirstPartnerJoined



