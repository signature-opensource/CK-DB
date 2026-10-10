# CK.DB.Auth

The authentication database layer: the users' authentication providers (`CK.tAuthProvider`, `CK.vUserAuthProvider`,
`CK.sAuthUserOnLogin`), the `IAuthenticationDatabaseService` and the provider contracts (`IBasicAuthenticationProvider`,
`IGenericAuthenticationProvider`) that the provider packages implement (UserPassword, UserGoogle, UserOidc,
UserSimpleCode...).

It uses the CK.AuthenticationInfo model (`IUserAuthInfo.Schemes` are `CK.Core.SchemeInfo`). The legacy
CK.Auth.Abstractions model is served by CK.DB.AspNet.Auth.

## CK.AuthenticationInfo services

| Service | Contract | What it does |
|---------|----------|--------------|
| [`SqlActorInfoProvider`](SqlActorInfoProvider.cs) | `IActorInfoProvider` | Reads a user (its current state) for the identity restoration. Unknown user: null. |
| [`SqlLoginProvider`](SqlLoginProvider.cs) | `ILoginProvider` | Logs in with the generic providers (with their own payload). Not the "Basic" scheme: it is the login provider of CK.DB.User.UserPassword, whose payload is the login command. |
| [`SqlAutoBindingAccountService`](SqlAutoBindingAccountService.cs) | `IAutoBindingAccountService` | Binds the scheme of a login to the logged in user (Critical level required by default). Only a provider whose payload comes from a remote assertion (`CanCreatePayload`) can be bound: a password or a weak actor name never is. |

**The "last login method" rule:** `Package.HasOtherSchemeAsync( ctx, userId, scheme )` (the SQL function
`CK.fAuthUserHasOtherScheme`) tells whether a user has another scheme than the one that would be removed, from
`CK.vUserAuthProvider` that every provider feeds. A user's own removal of a login method (a password, a binding, a
passkey...) must be refused when it is false: the user would be locked out. An administrator's removal is not subject to
it. A provider with several credentials for one scheme (passkeys) first checks its other credentials. A safety net, not
a guarantee: a scheme row doesn't prove that the method still works.

They are singletons: each call creates its own `SqlStandardCallContext` on the given monitor and disposes it. They
depend on no DI scope and no transport, so they work the same for an HTTP endpoint, an AppIdentity endpoint or in the
background. A two-step validated login (`actualLogin` false, then true) uses two independent contexts.

The users are built at `Critical` level, the highest they can reach: the session that receives them clamps it with its
dates. A weak actor (with the `"Weak"` scheme) is clamped to `Unsafe` by `ActorInfo` itself. The login failures keep the
`KnownLoginFailureCode` and their reason, and `UnregisteredUser` is flagged so that the authentication center can try
the account creation or binding.

`SqlAutoBindingAccountService` uses the generic provider of the scheme (`CreateOrUpdateUser` with an actual login).
It refuses while impersonating, for the basic scheme (a password is not bound by a login) and below the `Critical`
level unless `RequiresCriticalLevel` is set to false (the `Normal` level is then required).

`SqlLoginProvider` bridges the CK.AuthenticationInfo `ILoginProvider` to the current providers: a modernized provider
can implement `ILoginProvider` directly.
