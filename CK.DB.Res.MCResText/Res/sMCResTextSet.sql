-- SetupConfig: {}
--
-- Sets a string value for a resource in a given culture.
-- When Value is null, it is removed.
--
create procedure CK.sMCResTextSet
(
	@ResId int,
	@CultureId int,
	@Value nvarchar(max)
)
as
begin
	set nocount on;
	if @ResId <= 0 throw 50000, 'Res.InvalidResId', 1;
	if @CultureId <= 0 throw 50000, 'Culture.InvalidCultureId', 1;
	merge CK.tMCResText as target
		using ( select ResId = @ResId, CultureId = @CultureId )
		as source on source.ResId = target.ResId and source.CultureId = target.CultureId
		when matched and @Value is null then delete
		when matched then update set Value = @Value
		when not matched by target and @Value is not null then insert( ResId, CultureId, Value ) values( source.ResId, source.CultureId, @Value );
	return 0;
end
