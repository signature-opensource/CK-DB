The authentication database layer: the authentication providers of the users (`CK.tAuthProvider`, `CK.vUserAuthProvider`), the login procedure and the provider contracts that the provider packages implement.

Main types: `IAuthenticationDatabaseService`, `IBasicAuthenticationProvider`, `IGenericAuthenticationProvider` and the
CK.AuthenticationInfo services `SqlActorInfoProvider`, `SqlLoginProvider` and `SqlAutoBindingAccountService`.

See the [README](https://github.com/signature-opensource/CK-DB/blob/stable/CK.DB.Auth/README.md).
