-- SetupConfig: { "Requires": [ "CK.fAuthUserHasOtherScheme" ] }
--
-- The actor unlinks one of its own OpenID Connect identities.
-- @Result: 0 = NotFound (no identity for this scheme), 1 = Unlinked, 2 = LastLoginMethod (refused).
--
-- The "last login method" rule applies (CK.DB.Auth): the identity is not unlinked when the actor has no other scheme in
-- CK.vUserAuthProvider.
--
create procedure CK.sUserOidcUnlink
(
	@ActorId int,
	@SchemeSuffix varchar(64),
	@Result int output
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @SchemeSuffix is null throw 50000, 'Argument.InvalidSchemeSuffix', 1;

	--[beginsp]

	-- Locks the identities of the actor: concurrent unlinks of its last two identities are serialized.
	declare @Scheme varchar(69);
	select @Scheme = Scheme from CK.tUserOidc with(updlock, holdlock) where UserId = @ActorId and SchemeSuffix = @SchemeSuffix;

	set @Result = 0;
	if @Scheme is not null
	begin
		if CK.fAuthUserHasOtherScheme( @ActorId, @Scheme ) = 0
		begin
			set @Result = 2;
		end
		else
		begin
			--<PreUnlink revert />
			delete CK.tUserOidc where UserId = @ActorId and SchemeSuffix = @SchemeSuffix;
			set @Result = 1;
			--<PostUnlink />
		end
	end

	--[endsp]
end
