/*
delete from paymentfiles
delete from ReceivedPayments
delete from [PaymentAssignments]
update Registrations
set state = 1
*/
select *
from paymentfiles

select *
from ReceivedPayments

select *
from [dbo].[PaymentAssignments]

select *
from Registrations
where state = 2