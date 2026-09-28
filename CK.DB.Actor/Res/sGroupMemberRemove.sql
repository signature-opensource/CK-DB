-- SetupConfig: {}
--
-- Removes a member from a Group.
--
create procedure CK.sGroupMemberRemove
(
	@ActorId int,
	@GroupId int,
	@MemberId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @GroupId <= 0 throw 50000, 'Group.InvalidId', 1;

	--[beginsp]

	if @GroupId <> @MemberId and exists (select * from CK.tActorProfile where GroupId = @GroupId and ActorId = @MemberId)
	begin
		-- If this is the System Group, only members of it can remove members.
		if @GroupId = 1 
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId ) 
			begin
				;throw 50000, 'Security.ActorMustBeSytem', 1;
			end
		end

		--<PreMemberRemove revert />

		delete from CK.tActorProfile where GroupId = @GroupId and ActorId = @MemberId;
		
		--<PostMemberRemove />
	end

	--[endsp]
end
