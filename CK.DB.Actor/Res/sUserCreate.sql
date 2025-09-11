-- SetupConfig: { "Requires": [ "CK.sActorCreate" ] }
create procedure CK.sUserCreate 
(
	@ActorId int,
	@UserName nvarchar( 255 ) /*input*/output,
	@UserIdResult int output
)
as
begin
    if @ActorId is null or @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	--<PreCreate revert />

	exec CK.sActorCreate @ActorId, @UserIdResult output;
	insert into CK.tUser( UserId, UserName ) values ( @UserIdResult, @UserName );

	--<PostCreate />
	
	--[endsp]
end
