begin tran

select *
from registrations reg
where IsWaitingList = 1
  and id not in (select registrationid 
                 from Seats seat
                   inner join Registrables rbl on rbl.Id = seat.RegistrableId
                 where registrationid is not null
				   and IsWaitingList = 1
				   and rbl.HasWaitingList = 1)
  and id not in (select RegistrationId_Follower
                 from Seats seat
                   inner join Registrables rbl on rbl.Id = seat.RegistrableId
                 where RegistrationId_Follower is not null
				   and IsWaitingList = 1
				   and rbl.HasWaitingList = 1)

update reg
set IsWaitingList = 0
from registrations reg
where IsWaitingList = 1
  and id not in (select registrationid 
                 from Seats seat
                   inner join Registrables rbl on rbl.Id = seat.RegistrableId
                 where registrationid is not null
				   and IsWaitingList = 1
				   and rbl.HasWaitingList = 1)
  and id not in (select RegistrationId_Follower
                 from Seats seat
                   inner join Registrables rbl on rbl.Id = seat.RegistrableId
                 where RegistrationId_Follower is not null
				   and IsWaitingList = 1
				   and rbl.HasWaitingList = 1)
rollback

--commit


begin tran

update reg
set IsWaitingList = 1
--select *
from registrations reg
where IsWaitingList = 0
  and (id in (select registrationid 
                 from Seats seat
                   inner join Registrables rbl on rbl.Id = seat.RegistrableId
                 where registrationid is not null
				   and IsWaitingList = 1
				   and rbl.HasWaitingList = 1)
  or id in (select RegistrationId_Follower
                 from Seats seat
                   inner join Registrables rbl on rbl.Id = seat.RegistrableId
                 where RegistrationId_Follower is not null
				   and IsWaitingList = 1
				   and rbl.HasWaitingList = 1))

rollback

--commit