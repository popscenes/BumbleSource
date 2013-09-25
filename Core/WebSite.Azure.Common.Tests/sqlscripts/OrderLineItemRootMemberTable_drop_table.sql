
if exists (select * from sys.tables where name='OrderLineItemRootMemberTable')
begin
	truncate table OrderLineItemRootMemberTable
	drop table OrderLineItemRootMemberTable
end
GO


