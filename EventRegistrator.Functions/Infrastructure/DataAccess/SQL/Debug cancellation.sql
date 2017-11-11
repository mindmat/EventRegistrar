select *
from Registrations
where id = '4DC924D5-E497-4A40-822E-C1DA30AF2777'

select *
from seats
where RegistrationId = '4DC924D5-E497-4A40-822E-C1DA30AF2777'
  or RegistrationId_follower = '4DC924D5-E497-4A40-822E-C1DA30AF2777'
order by FirstPartnerJoined