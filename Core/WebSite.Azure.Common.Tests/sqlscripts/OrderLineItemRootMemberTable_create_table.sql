--- OrderLineItemRootMemberTable --------------------------------------------------------------------------------
if not exists (select * from sys.tables where name='OrderLineItemRootMemberTable')
begin

create table OrderLineItemRootMemberTable (
    Id nvarchar(256) not null,
	FriendlyId nvarchar(512),
	RowId bigint IDENTITY PRIMARY KEY,
	ShardId bigint,

	Product nvarchar(256) NULL,
	Price decimal(18, 2) NULL,
	Quantity int,
	
)

end
GO
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_OrderLineItemRootMemberTable_Id')
	CREATE NONCLUSTERED INDEX IX_OrderLineItemRootMemberTable_Id ON OrderLineItemRootMemberTable (Id);
GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_OrderLineItemRootMemberTable_ProductId')
	CREATE NONCLUSTERED INDEX IX_OrderLineItemRootMemberTable_ProductId ON OrderLineItemRootMemberTable (Product);
GO