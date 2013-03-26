
--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.FlierSearchRecord
if not exists (select * from sys.objects where type = 'P' AND name = 'FindFliersByLocationAndTags')
   exec('create procedure FindFliersByLocationAndTags as begin SET NOCOUNT ON; end')
GO
	
--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.FlierSearchRecord 
alter procedure FindFliersByLocationAndTags
	@loc geography,
	@top int,
	@distance int,
	@sort int = 1,
	@skipPastDate datetimeoffset = null,
	@xpath nvarchar(1000) = null
as
begin

--declare @loc geography,
--@top int = 5,
--@distance int = 10,
--@sort int = 1,
--@skipPastDate datetimeoffset = null,
--@xpath nvarchar(500) = null;

--set @loc = geography::STPointFromText('POINT (144.96327999999994 -37.814107)', 4326);
--set @xpath = '//tags[tag="for sale"]';
--set @createDate = '2013-03-06 11:26:09.9682780 +00:00';

declare @SQL NVARCHAR(4000);
declare @ParameterDefinition NVARCHAR(4000);

select	@ParameterDefinition = '
	@locParam geography,
	@topParam int,
	@distanceParam int,
	@sortParam nvarchar(200),
	@skipPastDateParam datetimeoffset = null,
	@xpathParam nvarchar(400) = null
';

select	@SQL = N'
	;With records as (	
		select 
		Location.STDistance(@locParam) as Metres, 
		*,
		row_number() over(partition by Id order by Id ) as RowNum
		from FlierSearchRecord 
		where Id = Id
		'

if @loc is not null
select @SQL = @SQL + N'
		and Location.STDistance(@locParam) <= @distanceParam*1000
		';

if @skipPastDate IS NOT NULL
select @SQL = @SQL + N'
		and CreateDate < @skipPastDateParam
		';

if @xpath IS NOT NULL
		select @SQL = @SQL + N'
		and Tags.exist(''' + @xpath + ''') > 0
		';
		
select @SQL = @SQL + N'
	)
	select 
	top(@topParam) * 
	from records
	where RowNum = 1
	order by 
' + case @Sort
			when 1 then 'CreateDate desc, PopularityRank desc, Location.STDistance(@locParam)'
			when 2 then 'EffectiveDate desc, PopularityRank desc, Location.STDistance(@locParam)'
			when 3 then 'Location.STDistance(@locParam), CreateDate desc, PopularityRank desc'
			else 'PopularityRank desc, Location.STDistance(@locParam), CreateDate desc'
		end 

		
exec sp_executeSQL 
	@SQL,
	@ParameterDefinition,
	@locParam = @loc,
	@topParam = @top,
	@distanceParam = @distance,
	@sortParam = @sort,
	@skipPastDateParam = @skipPastDate,
	@xpathParam = @xpath;
end

GO

--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.BoardFlierSearchRecord
if not exists (select * from sys.objects where type = 'P' AND name = 'FindFliersByBoard')
   exec('create procedure FindFliersByBoard as begin SET NOCOUNT ON; end')
GO

--CONTEXT=PostaFlya.DataRepository.Search.SearchRecord.BoardFlierSearchRecord
alter procedure FindFliersByBoard
		@board uniqueidentifier,
		@loc geography,
		@top int,
		@distance int,
		@sort int = 1,
		@skipFlier nvarchar(255) = null,
		@xpath nvarchar(1000) = null
as
begin

--declare @board uniqueidentifier, 
--@loc geography,
--@top int = 5,
--@distance int = 10,
--@sort int = 1,
--@skipFlier nvarchar(255) = null,
--@xpath nvarchar(500) = null;

--set @loc = geography::STPointFromText('POINT (144.96327999999994 -37.814107)', 4326);
--set @xpath = '//tags[tag="for sale"]';
--set @board = '44444444-2c16-42da-bb6e-e4acc4444208'
--set @skipFlier = '467f90a8-2c16-42da-bb6e-e4acc4444208';


declare @filterDate datetimeoffset;
if @skipFlier is not null
	select @filterDate = DateAdded from BoardFlierSearchRecord where FlierId = @skipFlier;


declare @SQL nvarchar(4000);
declare @ParameterDefinition nvarchar(4000);

select	@ParameterDefinition = '
	@boardParam uniqueidentifier,
	@locParam geography,
	@topParam int,
	@distanceParam int,
	@sortParam nvarchar(200),
	@filterDateParam datetimeoffset = null,
	@xpathParam nvarchar(400) = null
';

select	@SQL = N'
	;With records as (	
		select 
		Location.STDistance(@locParam) as Metres, *
		from BoardFlierSearchRecord fr			
		where BoardId = @boardParam AND BoardStatus = 2 
';

if @loc is not null
select @SQL = @SQL + N'
		and Location.STDistance(@locParam) <= @distanceParam*1000
		'
if @filterDate is not null
select @SQL = @SQL + N'
		and bfr.DateAdded < @filterDateParam
		';

if @xpath is not null
		select @SQL = @SQL + N'
		and Tags.exist(''' + @xpath + ''') > 0
		';
		
select @SQL = @SQL + N'
	)
	select 
	top(@topParam) * 
	from records
	order by 
' + case @Sort
			when 1 then 'DateAdded desc, PopularityRank desc, Location.STDistance(@locParam)'
			when 2 then 'EffectiveDate desc, PopularityRank desc, Location.STDistance(@locParam)'
			when 3 then 'Location.STDistance(@locParam), DateAdded desc, PopularityRank desc'
			else 'PopularityRank desc, Location.STDistance(@locParam), CreateDate desc'
		end 

		
exec sp_executeSQL 
	@SQL,
	@ParameterDefinition,
	@boardParam = @board,
	@locParam = @loc,
	@topParam = @top,
	@distanceParam = @distance,
	@sortParam = @sort,
	@filterDateParam = @filterDate,
	@xpathParam = @xpath;

end

GO