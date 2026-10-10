-- SetupConfig: { "Requires": [ "CK.vUserAuthProvider" ] }
--
-- Gets whether a user can log in with another scheme than @Scheme (a row of CK.vUserAuthProvider, that every provider
-- feeds): the "last login method" rule. A user's own removal of a login method must be refused when this is 0: the user
-- would be locked out. A provider with several credentials for one scheme (passkeys) first checks its other credentials.
-- This is a safety net, not a guarantee: a row doesn't prove that the method still works.
--
create function CK.fAuthUserHasOtherScheme
(
    @UserId int,
    @Scheme varchar(64)
)
returns bit
as begin
    return case when exists( select 1 from CK.vUserAuthProvider with(nolock) where UserId = @UserId and Scheme <> @Scheme ) then 1 else 0 end;
end
