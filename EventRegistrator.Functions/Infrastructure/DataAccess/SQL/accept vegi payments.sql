begin tran

select Diff = Price - map.Summe, Price, map.Summe, veg.id, reg.state, *
from Registrations reg
  inner join (SELECT RegistrationId, Summe = SUM(Amount)
              FROM PaymentAssignments
		 	 GROUP BY RegistrationId) MAP ON MAP.RegistrationId= reg.id
  left join (select reg.id
             from Responses rsp
               inner join Registrations reg on reg.Id = rsp.RegistrationId
               left join QuestionOptionToRegistrableMappings map on map.QuestionOptionId = rsp.QuestionOptionId
             where rsp.QuestionOptionId = '6A167341-46C4-4F63-B436-D55EDF04EDEA') veg on veg.id = reg.id
where Price - map.Summe <> 0
  and reg.state = 1


--update reg
--set state = 2
select *
from Registrations reg
  inner join (SELECT RegistrationId, Summe = SUM(Amount)
              FROM PaymentAssignments
		 	 GROUP BY RegistrationId) MAP ON MAP.RegistrationId= reg.id
  inner join (select reg.id
             from Responses rsp
               inner join Registrations reg on reg.Id = rsp.RegistrationId
               left join QuestionOptionToRegistrableMappings map on map.QuestionOptionId = rsp.QuestionOptionId
             where rsp.QuestionOptionId = '6A167341-46C4-4F63-B436-D55EDF04EDEA') veg on veg.id = reg.id
where Price - map.Summe <= 10
   --and reg.id = '3EB99FE8-55A7-422B-A3E6-A0D6E403FBCC'
   and reg.state = 1

select Diff = Price - map.Summe, Price, map.Summe, veg.id, reg.state, *
from Registrations reg
  inner join (SELECT RegistrationId, Summe = SUM(Amount)
              FROM PaymentAssignments
		 	 GROUP BY RegistrationId) MAP ON MAP.RegistrationId= reg.id
  left join (select reg.id
             from Responses rsp
               inner join Registrations reg on reg.Id = rsp.RegistrationId
               left join QuestionOptionToRegistrableMappings map on map.QuestionOptionId = rsp.QuestionOptionId
             where rsp.QuestionOptionId = '6A167341-46C4-4F63-B436-D55EDF04EDEA') veg on veg.id = reg.id
where Price - map.Summe <> 0
   and reg.state = 1

rollback
--commit