/*
delete from RegistrationForm
delete from Questions
delete from QuestionOptions
delete from registrations
delete from responses
delete from seats
*/
--948f6af3-1596-4715-90ab-ce64b89a0f51
--select * from RegistrationForms
--select * from Questions order by [index]
select * from [QuestionOptions] qop right join questions qst on qst.Id = qop.QuestionId order by questionid
select * from Registrables rsb left join QuestionOptionToRegistrableMappings map on map.RegistrableId = rsb.id

select * from registrations
select qst.Title, * from Responses rsp left join Questions qst on qst.id = rsp.QuestionId order by RegistrationId

select * from Seats seat inner join Registrables rbl on rbl.Id = seat.RegistrableId

--select * from DomainEvents order by [timestamp] desc
