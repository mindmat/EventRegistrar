select reg.respondentemail, *
from seats stl
  inner join registrations reg on reg.id in (stl.registrationid, stl.registrationid_follower)
  inner join registrables rbl on rbl.id = stl.registrableid
  --inner join seats stf on stl.partneremail = stf.partneremail and stl.id <> stf.id
--and stf.registrableid = stl.registrableid
where rbl.maximumdoubleseats is not null
  and stl.partneremail is not null
  and (stl.registrationid is null or stl.registrationid_follower is null)
  and reg.State <> 4
order by registrableid
