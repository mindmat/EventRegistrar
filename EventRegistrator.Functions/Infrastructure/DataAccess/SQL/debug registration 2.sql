declare @registrationids table
(
  id uniqueidentifier
);

insert into @registrationids 
select id
from Registrations
where 1=1
  and RespondentEmail in( 'phzimmermann@gmx.ch', 'yifanwang2010@gmail.com')
  --and RespondentEmail in('igor.bogomolov@gmail.com', 'demange.pauline@yahoo.com')
  --and RespondentFirstName like 'andrea%'
  --and RespondentEmail like '%jan.nydegger@gmail.com%'
  --or id in('E502B0BC-B3BA-475B-B8F4-20BB10DEAF59')
  --and ExternalIdentifier = '2_ABaOnueyyMvCyfoRdeowyfgi8sDCvVVs5WObREYP3d4Q3jmbk00V4V4lvnyMmp4'
  
select *
from Registrations
where id in (select id from @registrationids)
-- or id = '184821E5-418B-4856-BB11-8349951A317F'

select*
from Seats seat
  inner join Registrables rbl on rbl.Id = seat.RegistrableId
where (RegistrationId in (select id from @registrationids) or RegistrationId_Follower in (select id from @registrationids))
  --and rbl.MaximumDoubleSeats is not null

select *
from Mails mail
  inner join Registrations reg on mail.Recipients like '%'+ reg.RespondentEmail+'%'
where reg.Id in (select id from @registrationids)
order by Created desc

select *
from PaymentAssignments map
  inner join ReceivedPayments pmt on pmt.Id = map.ReceivedPaymentId
where map.RegistrationId in (select id from @registrationids)


--select *
--from seats
--where RegistrableId = '373E0514-2F5F-4499-990A-A130B9D38142'
--order by isWaitingList, FirstPartnerJoined



--GUTSCHRIFT VON FREMDBANK  NDEAFIHHXXX AUFTRAGGEBER: TIKANOJA ESSI HELINA MAKELANKATU 95A B 22 00610  HELSINK I 171116CH02D1Y8MD MITTEILUNGEN: LEAPIN LINDY / ESSI TIKANOJA, ESSI. TIKANOJA(AT)GMAIL.COM