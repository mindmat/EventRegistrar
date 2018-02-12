select top 100 withhold, *
from mails
where withhold = 1
order by Created desc

select top 100 withhold, *
from mails
where DATEDIFF(hh, Created, GETDATE()) < 30
order by Created desc
