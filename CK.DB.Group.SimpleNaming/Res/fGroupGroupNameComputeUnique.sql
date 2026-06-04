-- SetupConfig: {}
-- The @GroupId parameter must be set to the group identifier in case of a
-- rename (to handle the case of a rename with the same name). 
-- Set it to -1 to compute a group name for a new group.
-- When no unique name can be computed null is returned.
create Function CK.fGroupGroupNameComputeUnique
	(
		@GroupId	int,
		@GroupName	nvarchar(128),
		@PatternBefore nvarchar(10) = ' (',
		@PaternAfter nvarchar(10) = ')'
	)
returns nvarchar(128) -- with SCHEMABINDING
as 
begin
	if not exists( select '?' 
						from CK.tGroup g
						where g.GroupId <> @GroupId and g.GroupName = @GroupName ) 
	begin
		return @GroupName;
	end
    declare @MaxSize int = 128 - 2 /* @num width (0-99) */ - len( @PatternBefore ) - len( @PaternAfter );
	if len( @GroupName ) > @MaxSize set @GroupName = left( @GroupName, @MaxSize );
	set @GroupName = @GroupName + @PatternBefore;
	declare @proposed nvarchar(128);
	declare @num int = 1;
	while @num <= 99 
	begin
		set @proposed = @GroupName + cast(@num as nvarchar(4)) + @PaternAfter;
		if not exists( select '?'
							from CK.tGroup g
							where g.GroupId <> @GroupId and g.GroupName = @proposed ) 
		begin
			return @proposed;
		end
		set @num = @num+1;
	end
	return null;
end
