-- SetupConfig: { "Requires": [ "CK.sGroupMemberRemove" ] }
--
-- Clears a Group.
--
create procedure CK.sGroupRemoveAllMembers
(
	@ActorId int,
	@GroupId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @GroupId <= 0 throw 50000, 'Group.InvalidGroup', 1;

	--[beginsp]

	declare @MemberId int;
	declare @CMember cursor;
	set @CMember = cursor local fast_forward for 
		select ActorId from CK.tActorProfile p 
			where p.GroupId = @GroupId and p.ActorId <> @GroupId;
	open @CMember;
	fetch from @CMember into @MemberId;
	while @@FETCH_STATUS = 0
	begin
		exec CK.sGroupMemberRemove @ActorId, @GroupId, @MemberId;
		fetch next from @CMember into @MemberId;
	end
	deallocate @CMember;

	--[endsp]
end
