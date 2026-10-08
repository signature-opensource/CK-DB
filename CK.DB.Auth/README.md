# CK.DB.Auth

The authentication database layer: the users' authentication providers (`CK.tAuthProvider`, `CK.vUserAuthProvider`,
`CK.sAuthUserOnLogin`), the `IAuthenticationDatabaseService` and the provider contracts (`IBasicAuthenticationProvider`,
`IGenericAuthenticationProvider`) that the provider packages implement (UserPassword, UserGoogle, UserOidc,
UserSimpleCode...).

It uses the CK.AuthenticationInfo model (`IUserAuthInfo.Schemes` are `CK.Core.UserSchemeInfo`). The legacy
CK.Auth.Abstractions model is served by CK.DB.AspNet.Auth.

## CK.AuthenticationInfo services

| Service | Contract | What it does |
|---------|----------|--------------|
| [`SqlUserInfoProvider`](SqlUserInfoProvider.cs) | `IUserInfoProvider` | Reads a user (its current state) for the identity restoration. Unknown user: null. |
| [`SqlLoginProvider`](SqlLoginProvider.cs) | `ILoginProvider` | Logs in with every provider: the basic one with a `BasicLoginPayload`, the generic ones with their own payload. |
| [`SqlAutoBindingAccountService`](SqlAutoBindingAccountService.cs) | `IAutoBindingAccountService` | Binds the scheme of a login to the logged in user (Critical level required by default). |

They are singletons: each call creates its own `SqlStandardCallContext` on the given monitor and disposes it. They
depend on no DI scope and no transport, so they work the same for an HTTP endpoint, an AppIdentity endpoint or in the
background. A two-step validated login (`actualLogin` false, then true) uses two independent contexts.

The users are built at `Critical` level, the highest they can reach: the session that receives them clamps it with its
dates. A weak actor (with the `"Weak"` scheme) is clamped to `Unsafe` by `UserInfo` itself. The login failures keep the
`KnownLoginFailureCode` and their reason, and `UnregisteredUser` is flagged so that the authentication center can try
the account creation or binding.

`SqlAutoBindingAccountService` uses the generic provider of the scheme (`CreateOrUpdateUser` with an actual login).
It refuses while impersonating, for the basic scheme (a password is not bound by a login) and below the `Critical`
level unless `RequiresCriticalLevel` is set to false (the `Normal` level is then required).

`SqlLoginProvider` bridges the CK.AuthenticationInfo `ILoginProvider` to the current providers: a modernized provider
can implement `ILoginProvider` directly.
