select rbl.id, rbl.Name, accepted = sum(case when IsWaitingList = 1 THEN 0 else 1 end), waitingList = sum(CASE WHEN IsWaitingList = 1 THEN 1 else 0 end), 
       LeaderSpotsAvailable   = case when rbl.MaximumDoubleSeats is null then null else sum(CASE WHEN IsWaitingList = 0 and PartnerEmail is null and RegistrationId is null THEN 1 else 0 end) end,
       FollowerSpotsAvailable = case when rbl.MaximumDoubleSeats is null then null else sum(CASE WHEN IsWaitingList = 0 and PartnerEmail is null and RegistrationId_Follower is null THEN 1 else 0 end) end
from seats seat
  inner join Registrables rbl on rbl.id = seat.RegistrableId
where 1=1
  --and rbl.MaximumDoubleSeats is not null or rbl.MaximumSingleSeats is not null
  and seat.IsCancelled = 0

group by rbl.id, rbl.name, rbl.MaximumDoubleSeats
order by rbl.name

select *
from seats
where RegistrableId = '373E0514-2F5F-4499-990A-A130B9D38142'
order by IsWaitingList, FirstPartnerJoined

/*
Warteliste für
Lindy Hop Advanced/Advanced+ Follower
Lindy Hop Beginner Intermediate Follower
Lindy Hop Intermediate
Lindy Hop Intermediate Advanced Follower
*/
