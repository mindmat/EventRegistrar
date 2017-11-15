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

select *
from [dbo].[PaymentAssignments]

select *
from Registrations
where state = 2
--or RespondentEmail = 'mail@manuelnaegeli.ch'

/*
mail@manuelnaegeli.ch


*/
