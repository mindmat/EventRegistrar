begin tran

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