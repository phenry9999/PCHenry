select * 
from SchemaVersions 
--where ScriptName like '%usp_GetNextRenewingSubscription%'
--where ScriptName like '%42341%'
order by 1 desc

/*
begin tran
declare @releaseVer varchar(25) = '193'
select min(id) from SchemaVersions where scriptname like 'DatabaseUpdator.T_Release_' + @releaseVer +'%'
delete from SchemaVersions where id >= (select min(id) from SchemaVersions where scriptname like 'DatabaseUpdator.T_Release_' + @releaseVer +'%')
select * from SchemaVersions order by 1 desc
rollback tran
--commit tran

select * from SchemaVersions order by 1 desc
*/
