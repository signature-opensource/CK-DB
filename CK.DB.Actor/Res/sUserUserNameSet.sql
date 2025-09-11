-- SetupConfig: {}
-- Sets the user name. 
-- There is no guaranty that the actual value will be the same as the one requested (if auto numbering 
-- is injected for example). 
--
alter procedure CK.sUserUserNameSet
(
    @ActorId int,
    @UserId int,
    @UserName nvarchar(127) /*input*/output,
	@Success bit output
)
as begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--[beginsp]

	set @Success = 1;
	if exists( select * from CK.tUser where UserName = @UserName and UserId <> @UserId )
	begin
        declare @ClashUserName nvarchar( 255 ) = @UserName;

		select @UserName = UserName
		from CK.tUser
		where UserId = @UserId;

		set @Success = 0;
		--<UserNameSetClash />
	end
	if @Success = 1
	begin
		--<PreUserNameSet revert />

		update u 
			set u.UserName = @UserName
			from CK.tUser u   
			where u.UserId = @UserId;

		--<PostUserNameSet />
	end

	--[endsp]
end
