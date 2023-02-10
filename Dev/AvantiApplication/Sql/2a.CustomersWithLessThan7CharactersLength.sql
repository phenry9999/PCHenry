--not sure if query is adhoc or part of a bigger ask?
--assuming this is adhoc and looking for general info\data, just show the data
select * 
from ClientFile cf
where len(cf.ClientName) > 7

--this is what I might do if I'm using this as part of a bigger ask\requirement\support task
select cf.ID, cf.ClientName
from ClientFile cf
where len(cf.ClientName) > 7
order by cf.ID