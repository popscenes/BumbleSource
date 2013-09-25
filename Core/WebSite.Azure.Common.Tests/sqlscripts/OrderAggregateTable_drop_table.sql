
if exists (select * from sys.tables where name='OrderAggregateRootTable')
begin
	truncate table OrderAggregateRootTable
	drop table OrderAggregateRootTable
end
GO


