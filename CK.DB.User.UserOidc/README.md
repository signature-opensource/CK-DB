# CK.DB.User.UserOidc

## 1. Package Role

This package provides a complete implementation for user authentication via the **OpenID Connect (OIDC)** protocol. It is designed to be generic and can handle multiple OIDC providers simultaneously.

It integrates with the `CK.DB.Auth` framework to be automatically recognized as one or more authentication providers.

## 2. Key Concepts

### 2.1. `SchemeSuffix`: Handling Multiple OIDC Providers

Unlike the `UserGoogle` package, which handles only one provider, `UserOidc` can manage an infinite number of them (e.g., one for logging in with *Auth0*, another with *Okta*, etc.).

This is achieved through the `SchemeSuffix` column. Each OIDC provider is identified by a unique string. The full name of the authentication provider (the "scheme") will be `Oidc.<SchemeSuffix>`. If the suffix is empty, the scheme is simply `Oidc`.

### 2.2. `Sub`: The User Identifier

In the OIDC world, the `sub` (subject) is the unique and immutable identifier of a user **within a given provider**. The combination `(SchemeSuffix, Sub)` therefore uniquely identifies an external user.

## 3. Database Objects

### 3.1. `CK.tUserOidc` Table

This is the central table of the package.

**Schema:**
```sql
create table CK.tUserOidc
(
    UserId int not null,
    -- Suffix identifying the OIDC provider.
    SchemeSuffix varchar(64) collate Latin1_General_100_CI_AS not null,
    -- Computed column for the full scheme name (e.g., 'Oidc.Auth0').
	Scheme as case len(SchemeSuffix) when 0 then 'Oidc' else 'Oidc.' + SchemeSuffix end,
    -- User identifier from the provider.
	Sub nvarchar(64) collate Latin1_General_100_BIN2 not null,
	LastLoginTime datetime2(2) not null,

	constraint PK_CK_UserOidc primary key (UserId, SchemeSuffix),
	constraint FK_CK_UserOidc_UserId foreign key (UserId) references CK.tUser(UserId),
	constraint UK_CK_UserOidc_OidcSub unique( SchemeSuffix, Sub )
);
```
-   The primary key is `(UserId, SchemeSuffix)`, which means a user can only be associated once with a given OIDC provider.
-   A unique constraint on `(SchemeSuffix, Sub)` ensures that an external account can only be linked to a single `UserId`.

### 3.2. `CK.sUserOidcUCL` Stored Procedure

This is the single procedure for all operations (`UCL` = Update/Create/Login), similar to `UserGoogle`'s, but with `@SchemeSuffix` and `@Sub` parameters.

### 3.3. Transformer on `CK.vUserAuthProvider`

The package registers itself with `CK.DB.Auth` via a TQL transformer:

```sql
-- File: Res/vUserAuthProvider.tql
create transformer on CK.vUserAuthProvider
as
begin
	inject "
	union all
	select UserId, Scheme, LastLoginTime from CK.tUserOidc where UserId > 0
	" after first part {select};
end
```
It uses the computed `Scheme` column to declare all configured OIDC providers.

## 4. C# API

### 4.1. `UserOidcTable` Class

This class implements `IGenericAuthenticationProvider<IUserOidcInfo>` and exposes methods for interacting with the database, such as `CreateOrUpdateOidcUserAsync`.

### 4.2. `IUserOidcInfo` Interface

This `IPoco` defines the data contract for an OIDC user.

```csharp
// File: IUserOidcInfo.cs
public interface IUserOidcInfo : IPoco
{
    string SchemeSuffix { get; set; }
    string Sub { get; set; }
}
```
-   It serves as the "payload" for authentication operations.
-   This interface can be extended by other packages to add provider-specific information (by adding columns to `tUserOidc` via transformers).

## 5. Dependencies

-   **`CK.DB.Auth`**: Provides the base authentication framework.