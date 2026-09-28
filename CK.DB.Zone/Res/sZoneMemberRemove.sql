-- SetupConfig: {}
--
-- Removes a member from a Zone.
--
alter procedure CK.sZoneMemberRemove
(
	@ActorId int,
	@ZoneId int,
	@MemberId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	-- The member must be in the Zone...
	if @ZoneId <> @MemberId and exists (select * from CK.tActorProfile where GroupId = @ZoneId and ActorId = @MemberId)
	begin
		-- ...and if this is the System Zone, only members of it can remove members.
		if @ZoneId = 1 
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId ) 
			begin
				;throw 50000, 'Security.ActorMustBeSytem', 1;
			end
		end
		-- ..and if the ZoneId is actually a Group, this is an error.
		if not exists (select * from CK.tZone with(serializable) where ZoneId = @ZoneId) throw 50000, 'Zone.InvalidId', 1;

		--<PreZoneMemberRemove revert />

		-- Removes the member from all the groups of the security Zone.
		declare @GroupId int;
		declare @CGroup cursor;
		set @CGroup = cursor local fast_forward for 
			select a.GroupId
				from CK.tActorProfile a
				inner join CK.tGroup g on g.GroupId = a.GroupId
				where g.ZoneId = @ZoneId and a.ActorId = @MemberId and a.GroupId <> @ZoneId;
		open @CGroup;
		fetch from @CGroup into @GroupId;
		while @@FETCH_STATUS = 0
		begin
			exec CK.sGroupMemberRemove @ActorId, @GroupId, @MemberId;
			fetch next from @CGroup into @GroupId;
		end
		deallocate @CGroup;

		delete from CK.tActorProfile where GroupId = @ZoneId and ActorId = @MemberId;

		--<PostZoneMemberRemove />
	end

	--[endsp]
end
