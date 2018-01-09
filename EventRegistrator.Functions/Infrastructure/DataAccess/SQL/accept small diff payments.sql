begin tran

select Diff = Price - map.Summe, Price, map.Summe, reg.state, *
from Registrations reg
  inner join (SELECT RegistrationId, Summe = SUM(Amount)
              FROM PaymentAssignments
		 	 GROUP BY RegistrationId) MAP ON MAP.RegistrationId = reg.id
where Price - map.Summe < 13 
  and reg.state = 1


update reg
set state = 2
--select *
from Registrations reg
  inner join (SELECT RegistrationId, Summe = SUM(Amount)
              FROM PaymentAssignments
		 	 GROUP BY RegistrationId) MAP ON MAP.RegistrationId = reg.id
where Price - map.Summe <13
   and reg.state = 1

select Diff = Price - map.Summe, Price, map.Summe, reg.state, *
from Registrations reg
  inner join (SELECT RegistrationId, Summe = SUM(Amount)
              FROM PaymentAssignments
		 	 GROUP BY RegistrationId) MAP ON MAP.RegistrationId = reg.id
where Price - map.Summe < 13
   and reg.state = 1

--rollback
--commit