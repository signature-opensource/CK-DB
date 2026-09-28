-- SetupConfig: {}
--
-- Adds a User or another kinf of Actor to a Zone (but not a Group).
--
alter procedure CK.sZoneMemberAdd 
(
	@ActorId int,
	@ZoneId int,
	@MemberId int
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @ZoneId <= 0 throw 50000, 'Zone.InvalidId', 1;
	-- ..and if the ZoneId is actually a Group, this is an error.
	if not exists (select * from CK.tZone where ZoneId = @ZoneId) throw 50000, 'Zone.InvalidId', 1;

	-- System is, somehow, already in all groups.
    if @MemberId = 1 return 0;

	--[beginsp]


	-- The user must not be already in the zone...
	if @ZoneId <> @MemberId and not exists (select * from CK.tActorProfile where GroupId = @ZoneId and ActorId = @MemberId)
	begin
		-- ...and if this is the System Zone, only members of it can add members.
		if @ZoneId = 1 
		begin
			if not exists( select 1 from CK.tActorProfile p where p.GroupId = 1 and p.ActorId = @ActorId ) 
			begin
				;throw 50000, 'Security.ActorMustBeSytem', 1;
			end
		end

		--<PreZoneMemberAdd revert />

		insert into CK.tActorProfile( ActorId, GroupId ) values( @MemberId, @ZoneId );

		--<PostZoneMemberAdd />
	end

	--[endsp]
end
