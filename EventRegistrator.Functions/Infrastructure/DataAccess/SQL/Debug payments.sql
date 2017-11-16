/*
delete from paymentfiles
delete from ReceivedPayments
delete from [PaymentAssignments]
update Registrations
set state = 1
*/
select *
from paymentfiles

select diff = PMT.Amount - MAP.Summe,*
from ReceivedPayments PMT
  left join (SELECT ReceivedPaymentId, Summe = SUM(Amount)
             FROM PaymentAssignments
			 GROUP BY ReceivedPaymentId) MAP ON MAP.ReceivedPaymentId = PMt.Id
where PMT.Settled = 0
  or RecognizedEmail is null

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
 

  --and 

  /*
select *
from [dbo].[PaymentAssignments]

select *
from Registrations
where state = 2
*/
--or RespondentEmail = 'mail@manuelnaegeli.ch'

/*
mail@manuelnaegeli.ch


*/
