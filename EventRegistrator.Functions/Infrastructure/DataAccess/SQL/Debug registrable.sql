select *
from seats
where RegistrableId = '2DBF19B6-6DBD-4050-AC4B-683B9BBA9A98'
order by FirstPartnerJoined


select singleleaders   = sum(CASE WHEN PartnerEmail IS NULL AND RegistrationId_Follower IS NULL THEN 1 ELSE 0 END),
       singlefollowers = sum(CASE WHEN PartnerEmail IS NULL AND RegistrationId IS NULL          THEN 1 ELSE 0 END)
from seats
where RegistrableId = '2DBF19B6-6DBD-4050-AC4B-683B9BBA9A98'
  and IsWaitingList = 0

select singleleaders   = sum(CASE WHEN PartnerEmail IS NULL AND RegistrationId_Follower IS NULL THEN 1 ELSE 0 END),
       singlefollowers = sum(CASE WHEN PartnerEmail IS NULL AND RegistrationId IS NULL          THEN 1 ELSE 0 END)
from seats
where RegistrableId = '2DBF19B6-6DBD-4050-AC4B-683B9BBA9A98'
  and IsWaitingList = 1
