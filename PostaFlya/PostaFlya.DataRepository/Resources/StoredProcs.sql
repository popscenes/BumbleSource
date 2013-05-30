
--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.FlierSearchRecord
if not exists (select * from sys.objects where type = 'P' AND name = 'FindFliersByLocationAndTags2')
   exec('create procedure FindFliersByLocationAndTags2 as begin SET NOCOUNT ON; end')
GO
	
--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.FlierSearchRecord 
alter procedure FindFliersByLocationAndTags2
	@loc geography,
	@top int,
	@distance int,
	@sort int = 1,
	@skipPast bigint = null,
	@skipPastEventAndCreateDate nvarchar(1000) = null,
	@xpath nvarchar(1000) = null,
	@eventDate datetime2 = null
	
as
begin

--declare @loc geography,
--@top int = 5,
--@distance int = 1000,
--@sort int = 1,
--@skipPast bigint = null,
--@eventDate datetime2 = '2076-08-11',
--@xpath nvarchar(500) = null;

--set @loc = geography::STPointFromText('POINT (144.979 -37.769)', 4326);
--set @xpath = '//tags[tag="for sale"]';
--set @createDate = '2013-03-06 11:26:09.9682780 +00:00';

declare @SQL NVARCHAR(4000);
declare @ParameterDefinition NVARCHAR(4000);

select	@ParameterDefinition = '
	@locParam geography,
	@topParam int,
	@distanceParam int,
	@skipPastParam bigint = null,
	@skipPastEventAndCreateDateParam nvarchar(1000) = null,
	@xpathParam nvarchar(400) = null,
	@eventDateParam datetime2 = null
';

select	@SQL = N'
	;With records as (	
		select 
		fr.Location.STDistance(@locParam) as Metres
		,fr.[Id]
		,fr.[LocationShard]
		,fr.[FriendlyId]
		,fr.[BrowserId]
		,fr.[NumberOfClaims]
		,fr.[NumberOfComments]
		,fr.[Status]
		,fr.[EffectiveDate]
		,fr.[CreateDate]
		,fr.[LastActivity]
		,fr.[Tags]
		,fr.[Location],
		row_number() over(partition by fr.Id order by fr.Id ) as RowNum';

if @eventDate IS NOT NULL
	select @SQL = @SQL + N', ed.EventDate as EventDate, ed.SortOrder as SortOrderString';

if @eventDate IS NULL
	select @SQL = @SQL + N', fr.SortOrder as SortOrder' 

select	@SQL = @SQL + N' 
			from FlierSearchRecord fr
		';

if @eventDate IS NOT NULL
select @SQL = @SQL + N'
		left outer join FlierDateSearchRecord ed on ed.Id = fr.Id
		';

select @SQL = @SQL + N'
		where fr.Id = fr.Id and (fr.Status is null or fr.Status = 1)
		'

if @loc is not null
select @SQL = @SQL + N'
		and fr.Location.STDistance(@locParam) <= @distanceParam*1000
		';

if @skipPast IS NOT NULL and @eventDate IS NULL
select @SQL = @SQL + N'
		and fr.SortOrder < @skipPastParam
		';

if @xpath IS NOT NULL
		select @SQL = @SQL + N'
		and fr.Tags.exist(''' + @xpath + ''') > 0
		';

if @eventDate IS NOT NULL and @skipPastEventAndCreateDate IS NULL
		select @SQL = @SQL + N'
		and CAST(ed.EventDate as datetime2) >= @eventDateParam
		';

if @eventDate IS NOT NULL and @skipPastEventAndCreateDate IS NOT NULL
		select @SQL = @SQL + N'
		and ed.SortOrder > @skipPastEventAndCreateDateParam
		';
		
select @SQL = @SQL + N'
	)
	select 
	top(@topParam) * 
	from records
	where RowNum = 1
	';

if @eventDate IS NOT NULL
			select @SQL = @SQL + N' order by SortOrderString asc'

if @eventDate IS NULL
			select @SQL = @SQL + N' order by SortOrder desc' 
	
				
exec sp_executeSQL 
	@SQL,
	@ParameterDefinition,
	@locParam = @loc,
	@topParam = @top,
	@distanceParam = @distance,
	@skipPastParam = @skipPast,
	@skipPastEventAndCreateDateParam = @skipPastEventAndCreateDate,
	@xpathParam = @xpath,
	@eventDateParam = @eventDate;


end

GO

--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.BoardFlierSearchRecord
if not exists (select * from sys.objects where type = 'P' AND name = 'FindFliersByBoard2')
   exec('create procedure FindFliersByBoard2 as begin SET NOCOUNT ON; end')
GO

--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.BoardFlierSearchRecord
alter procedure FindFliersByBoard2
		@board uniqueidentifier,
		@loc geography,
		@top int,
		@distance int,
		@sort int = 1,
		@skipFlier nvarchar(255) = null,
		@skipPastEventAndCreateDate nvarchar(1000) = null,
		@xpath nvarchar(1000) = null,
		@eventDate datetime2 = null
as
begin

--declare @board uniqueidentifier, 
--@loc geography,
--@top int = 5,
--@distance int = 10,
--@sort int = 1,
--@skipFlier nvarchar(255) = null,
--@eventDate datetime2 = '2076-08-11',
--@xpath nvarchar(500) = null;

--set @loc = geography::STPointFromText('POINT (144.96327999999994 -37.814107)', 4326);
--set @xpath = '//tags[tag="for sale"]';
--set @board = '44444444-2c16-42da-bb6e-e4acc4444208'
--set @skipFlier = '467f90a8-2c16-42da-bb6e-e4acc4444208';


declare @filterSort bigint;
if @skipFlier is not null
	select @filterSort = SortOrder from BoardFlierSearchRecord where FlierId = @skipFlier;


declare @SQL nvarchar(4000);
declare @ParameterDefinition nvarchar(4000);

select	@ParameterDefinition = '
	@boardParam uniqueidentifier,
	@locParam geography,
	@topParam int,
	@distanceParam int,
	@filterSortParam bigint = null,
	@skipPastEventAndCreateDateParam nvarchar(1000) = null,
	@xpathParam nvarchar(400) = null,
	@eventDateParam datetime2 = null
';

select	@SQL = N'
	;With records as (	
		select 
		fr.Location.STDistance(@locParam) as Metres 
		,fr.[Id]
		,fr.[BoardId]
		,fr.[DateAdded]
		,fr.[BoardStatus]
		,fr.[FlierId]
		,fr.[Location]
		,fr.[NumberOfClaims]
		,fr.[EffectiveDate]
		,fr.[CreateDate]
		,fr.[Tags]
		,fr.[Status]
		';


if @eventDate IS NOT NULL
	select @SQL = @SQL + N', ed.EventDate as EventDate, ed.SortOrder as SortOrderString';

if @eventDate IS NULL
			select @SQL = @SQL + N', fr.SortOrder as SortOrder' 

select	@SQL = @SQL + N' 
			from BoardFlierSearchRecord fr
		';

if @eventDate IS NOT NULL
select @SQL = @SQL + N'
		left outer join BoardFlierDateSearchRecord ed on ed.Id = fr.Id
		';

select @SQL = @SQL + N'							
		where fr.BoardId = @boardParam AND fr.BoardStatus = 2 and (fr.Status is null or fr.Status = 1)
';

if @loc is not null
select @SQL = @SQL + N'
		and fr.Location.STDistance(@locParam) <= @distanceParam*1000
		'
if @filterSort is not null  and @eventDate IS NULL
select @SQL = @SQL + N'
		and fr.SortOrder < @filterSortParam
		';

if @eventDate IS NOT NULL and @skipPastEventAndCreateDate IS NULL
		select @SQL = @SQL + N'
		and CAST(ed.EventDate as datetime2) >= @eventDateParam
		';

if @eventDate IS NOT NULL and @skipPastEventAndCreateDate IS NOT NULL
		select @SQL = @SQL + N'
		and ed.SortOrder > @skipPastEventAndCreateDateParam
		';


if @xpath is not null
		select @SQL = @SQL + N'
		and fr.Tags.exist(''' + @xpath + ''') > 0
		';


		
select @SQL = @SQL + N'
	)
	select 
	top(@topParam) * 
	from records
';

if @eventDate IS NOT NULL
			select @SQL = @SQL + N' order by SortOrderString asc'

if @eventDate IS NULL
			select @SQL = @SQL + N' order by SortOrder desc' 

		
exec sp_executeSQL 
	@SQL,
	@ParameterDefinition,
	@boardParam = @board,
	@locParam = @loc,
	@topParam = @top,
	@distanceParam = @distance,
	@filterSortParam = @filterSort,
	@skipPastEventAndCreateDateParam = @skipPastEventAndCreateDate,
	@xpathParam = @xpath,
	@eventDateParam = @eventDate;
end

GO

--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.BoardSearchRecord
if not exists (select * from sys.objects where type = 'P' AND name = 'FindNearbyBoards')
   exec('create procedure FindNearbyBoards as begin SET NOCOUNT ON; end')
GO

--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.BoardSearchRecord
alter procedure FindNearbyBoards
		@loc geography,
		@withinmetres int
as
begin
	select *, Location.STDistance(@loc) as Metres 
	from BoardSearchRecord br
	where Location.STDistance(@loc) < @withinmetres
end

GO
