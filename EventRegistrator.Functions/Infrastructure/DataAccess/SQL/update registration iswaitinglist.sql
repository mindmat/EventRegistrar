select *
from 

begin tran
update registrations
set iswaitinglist = 0
where iswaitinglist = 1
  and id not in
(
select registrationid
from seats
where iswaitinglist = 1
  and registrationid is not null
union
select registrationid_follower
from seats
where iswaitinglist = 1
  and registrationid_follower is not null
) 

select *
from seats
where registrationid = '189276E0-C8BE-4568-B392-B36082DD71FC' or registrationid_follower ='189276E0-C8BE-4568-B392-B36082DD71FC'
--select *
--from registrations reg
  
  commit