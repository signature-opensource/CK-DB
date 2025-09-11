create procedure CK.sUserUserProfileRead
(
    @ActorId int,
    @UserId int /*input*/output,
    @UserName nvarchar( 255 ) output
)
as
begin
	if @ActorId <= 0 throw 50000, 'Argument.InvalidActorId', 1;

    --<PreSelect revert />

    select @UserId = u.UserId,
           @UserName = u.UserName
    from CK.tUser u
    where u.UserId = @UserId;

    --<PostSelect />
end
