declare @ID uniqueidentifier
SET @ID = '9310B016-D70F-4EB1-8F04-57412132F4A8'

select *
from Registrables
where id = @id

select *
from seats
where RegistrableId = @ID
  and IsCancelled = 0
order by IsWaitingList, FirstPartnerJoined


select singleleaders   = sum(CASE WHEN PartnerEmail IS NULL AND RegistrationId_Follower IS NULL THEN 1 ELSE 0 END),
       singlefollowers = sum(CASE WHEN PartnerEmail IS NULL AND RegistrationId IS NULL          THEN 1 ELSE 0 END)
from seats
where RegistrableId = @ID
  and IsWaitingList = 0

select singleleaders   = sum(CASE WHEN PartnerEmail IS NULL AND RegistrationId_Follower IS NULL THEN 1 ELSE 0 END),
       singlefollowers = sum(CASE WHEN PartnerEmail IS NULL AND RegistrationId IS NULL          THEN 1 ELSE 0 END)
from seats
where RegistrableId = @ID
  and IsWaitingList = 1
