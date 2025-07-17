# CK.DB.User.UserGoogle

## 1. Package Role

This package provides a complete implementation for user authentication via their Google account. It integrates with the `CK.DB.Auth` framework to be automatically recognized as an authentication provider.

It is responsible for:
-   Storing the association between a system `UserId` and a `GoogleAccountId`.
-   Providing the logic to create, update, and authenticate a user via Google.
-   Dynamically registering itself as an available authentication provider.

## 2. Database Objects

### 2.1. `CK.tUserGoogle` Table

This is the central table of the package. It links an internal user to their Google identifier.

**Schema:**
```sql
create table CK.tUserGoogle
(
    UserId int not null,
    -- Unique identifier for the Google account (from Google's API).
    GoogleAccountId varchar(36) collate Latin1_General_100_BIN2 not null,
    LastLoginTime datetime2(2) not null,
    
    constraint PK_CK_UserGoogle primary key (UserId),
    constraint FK_CK_UserGoogle_UserId foreign key (UserId) references CK.tUser(UserId),
    constraint UK_CK_UserGoogle_GoogleAccountId unique( GoogleAccountId )
);
```
-   `UserId`: Foreign key to `CK.tUser` (from the `CK.DB.Actor` package).
-   `GoogleAccountId`: The immutable identifier provided by Google. It is unique in the system.
-   `LastLoginTime`: Timestamp of the last successful login, used by `CK.DB.Auth`.

### 2.2. `CK.sUserGoogleUCL` Stored Procedure

This is the single procedure for all create, update, and login operations (`UCL` = Update/Create/Login).

-   **In create/update mode** (`@UserId > 0`): It associates a `GoogleAccountId` with an existing `UserId`. An error is thrown if the `GoogleAccountId` is already used by another user.
-   **In login mode** (`@UserId = 0`): It finds the user corresponding to the provided `GoogleAccountId`. If the user is found, it validates the login by calling `CK.sAuthUserOnLogin` (provided by `CK.DB.Auth`) and updates `LastLoginTime`.

### 2.3. Transformer on `CK.vUserAuthProvider`

For the `CK.DB.Auth` system to recognize "Google" as a valid provider, this package uses a TQL transformer:

```sql
-- File: Res/vUserAuthProvider.tql
create transformer on CK.vUserAuthProvider
as
begin
	inject "
	union all
	select UserId, 'Google', LastLoginTime from CK.tUserGoogle where UserId > 0
	" after first part {select};
end
```
This script dynamically injects all registered Google users into the `vUserAuthProvider` view, making them available to the central authentication system.

## 3. C# API

### 3.1. `UserGoogleTable` Class

This `abstract partial` class is the C# entry point for interacting with the package.
-   It is linked to the `tUserGoogle` table via the `[SqlTable]` attribute.
-   It implements `IGenericAuthenticationProvider<IUserGoogleInfo>`, the interface required by `CK.DB.Auth`.
-   Methods like `CreateOrUpdateGoogleUserAsync` and `LoginUserAsync` are mapped to the `sUserGoogleUCL` stored procedure via the `[SqlProcedure]` attribute.

### 3.2. `IUserGoogleInfo` Interface

This `IPoco` defines the data contract for a Google user.
```csharp
public interface IUserGoogleInfo : IPoco
{
    string? GoogleAccountId { get; set; }
}
```
-   It serves as the "payload" for authentication operations.
-   This interface can be extended by other packages to add more information (see `UserGoogle.Profile`, `UserGoogle.EMailColumns`, etc.). These packages add columns to the `tUserGoogle` table via TQL transformers.

## 4. Dependencies

-   **`CK.DB.Auth`**: Provides the base authentication framework that this package integrates with.