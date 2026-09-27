-- SetupConfig: { "Requires": [ "CK.sGroupMemberRemove" ] }
--
-- Removes a User or other kind of Actor from all the Groups it belongs to.
--
create procedure CK.sMemberRemoveFromAllGroups
(
	@ActorId int,
	@MemberId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	declare @GroupId int;
	declare @CGroup cursor;
	set @CGroup = cursor local fast_forward for 
		select GroupId from CK.tActorProfile where ActorId = @MemberId and GroupId <> @MemberId;
	open @CGroup
	fetch from @CGroup into @GroupId
	while @@FETCH_STATUS = 0
	begin
		exec CK.sGroupMemberRemove @ActorId, @GroupId, @MemberId;
		fetch next from @CGroup into @GroupId;
	end
	deallocate @CGroup;

	--[endsp]
end
