

--- OrderAggregateRootTable --------------------------------------------------------------------------------
if not exists (select * from sys.tables where name='OrderAggregateRootTable')
begin

create table OrderAggregateRootTable (
    Id nvarchar(256) CONSTRAINT OrderAggregateRootTable_Id UNIQUE NONCLUSTERED not null,
	FriendlyId nvarchar(512),
	RowId bigint IDENTITY PRIMARY KEY,
	ShardId bigint,

	Json nvarchar(MAX),
	JsonHash bigint,
	ClrType nvarchar(256),

	LastUpdated datetime2,
	CustomerId nvarchar(256) NOT NULL,
	OrderCredit  decimal(18, 2) NULL,
)

end

GO

IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_OrderAggregateRootTable_CustomerId')
	CREATE NONCLUSTERED INDEX IX_OrderAggregateRootTable_CustomerId ON OrderAggregateRootTable (CustomerId);
GO


