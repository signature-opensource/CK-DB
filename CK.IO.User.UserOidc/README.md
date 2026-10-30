# CK.IO.User.UserOidc

The self-service commands of a user's OpenID Connect identities (the bindings of `CK.tUserOidc`, CK.DB.User.UserOidc):
a user sees the accounts it signs in with and unlinks the ones it doesn't want anymore. They are handled by
CK.DB.User.UserOidc (where the database is: the authentication center).

| Command | Result | Level | What it does |
|---|---|---|---|
| [`IGetOidcIdentitiesQCommand`](IGetOidcIdentitiesQCommand.cs) | `List<IUserOidcIdentity>` | Normal | Lists the identities of the current user. |
| [`IUnlinkOidcIdentityCommand`](IUnlinkOidcIdentityCommand.cs) | `ICrisBasicCommandResult` | Critical | Unlinks an identity of the current user (its `SchemeSuffix`). Refused while impersonating, and when it is the user's last way to log in (the "last login method" rule of CK.DB.Auth). |

[`IUserOidcIdentity`](IUserOidcIdentity.cs): the `SchemeSuffix` ("Google" for "Oidc.Google", empty for "Oidc"), the
`Claims` shared by the provider at the last actual login (a JSON object, "{}" when none is known: the user's own data,
possibly personal) and the `LastLoginTime`. The subject identifier is not exposed: a user has at most one identity per
scheme, the suffix designates it. The scheme and the account to display (an email, a name...) are derived from them by
the clients.

[`IncomingValidators`](IncomingValidators.cs) checks the scheme suffix and refuses the unlink while impersonating.

Binding a new identity to the current user is a remote login: `IStartLoginCommand.BindToCurrentActor`
(CK.IO.Authentication).
