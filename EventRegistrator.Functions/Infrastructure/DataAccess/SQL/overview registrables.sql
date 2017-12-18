select rbl.id, rbl.Name, 
       Accepted    = SUM(CASE WHEN SEAT.IsWaitingList = 1 OR REG.IsWaitingList = 1 THEN 0 ELSE 1 END),
       WaitingList = SUM(CASE WHEN SEAT.IsWaitingList = 1 OR REG.IsWaitingList = 1 THEN 1 ELSE 0 END),
       LeaderSpotsAvailable   = case when rbl.MaximumDoubleSeats is null then null else sum(CASE WHEN SEAT.IsWaitingList = 0 and PartnerEmail is null and RegistrationId is null THEN 1 else 0 end) end,
       FollowerSpotsAvailable = case when rbl.MaximumDoubleSeats is null then null else sum(CASE WHEN SEAT.IsWaitingList = 0 and PartnerEmail is null and RegistrationId_Follower is null THEN 1 else 0 end) end
from seats seat
  INNER JOIN dbo.Registrables  RBL ON RBL.Id = seat.RegistrableId
  LEFT  JOIN dbo.Registrations REG ON REG.Id = seat.RegistrationId
where 1=1
  --and rbl.MaximumDoubleSeats is not null or rbl.MaximumSingleSeats is not null
  and seat.IsCancelled = 0

group by rbl.id, rbl.name, rbl.MaximumDoubleSeats
order by rbl.name

--select *
--from seats
--where RegistrableId = '373E0514-2F5F-4499-990A-A130B9D38142'
--order by IsWaitingList, FirstPartnerJoined

/*
Warteliste für
Lindy Hop Advanced/Advanced+ Follower
Lindy Hop Beginner Intermediate Follower
Lindy Hop Intermediate
Lindy Hop Intermediate Advanced Follower
*/
