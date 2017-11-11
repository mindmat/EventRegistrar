/*
delete from paymentfiles
delete from ReceivedPayments
delete from [PaymentAssignments]
update Registrations
set ispaid = 0
*/
select *
from paymentfiles

select *
from ReceivedPayments

select *
from [dbo].[PaymentAssignments]

select *
from Registrations
where ispaid = 1