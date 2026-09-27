-- SetupConfig: {}
--
-- Add a User or other kind of Actor (except Group) to a Group.
-- Does nothing if the member identifier is already in the Group.
--
alter procedure CK.sGroupMemberAdd
(
	@ActorId int,
	@GroupId int,
	@MemberId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @GroupId <= 0 throw 50000, 'Group.InvalidId', 1;

	-- System is, somehow, already in all groups.
    if @MemberId = 1 return 0;

    -- Any kind of Actor can be added, except Group.
    if exists(select 1 from CK.tGroup where GroupId = @MemberId)
    begin
        ;throw 50000, 'Group.GroupForbidden', 1;
    end

	--[beginsp]

	if @GroupId <> @MemberId and not exists (select * from CK.tActorProfile where GroupId = @GroupId and ActorId = @MemberId)
	begin
		-- If this is the System Group, only members of it can add new Users.
		if @GroupId = 1
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId )
			begin
				;throw 50000, 'Security.ActorMustBeSytem', 1;
			end
		end

		--<PreUserAdd revert />

		insert into CK.tActorProfile( ActorId, GroupId ) values( @MemberId, @GroupId );

		--<PostUserAdd />

	end
	--[endsp]
end
