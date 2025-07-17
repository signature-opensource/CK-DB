# CK.DB.User.UserSimpleCode

## 1. Package Role

This package provides a very simple authentication method based on a unique code (a string) associated with a user. It can be used for "magic link" scenarios, one-time passwords, or any other authentication where a simple code is exchanged.

It integrates with the `CK.DB.Auth` framework to be automatically recognized as an authentication provider named "SimpleCode".

## 2. Database Objects

### 2.1. `CK.tUserSimpleCode` Table

This is the central table of the package. It links a user to their unique code.

**Schema:**
```sql
create table CK.tUserSimpleCode
(
    UserId int not null,
    -- The unique code that identifies the user.
    SimpleCode nvarchar(128) collate Latin1_General_100_BIN2 not null,
    LastLoginTime datetime2(2) not null,

    constraint PK_CK_UserSimpleCode primary key (UserId),
    constraint FK_CK_UserSimpleCode_UserId foreign key (UserId) references CK.tUser(UserId),
    constraint UK_CK_UserSimpleCode_SimpleCode unique( SimpleCode )
);
```
-   `UserId`: Foreign key to `CK.tUser`.
-   `SimpleCode`: The unique code. A `unique` constraint ensures that a code can only be associated with a single user.
-   `LastLoginTime`: Timestamp of the last successful login.

### 2.2. `CK.sUserSimpleCodeUCL` Stored Procedure

This is the single procedure for all operations (`UCL` = Update/Create/Login).

-   **In create/update mode** (`@UserId > 0`): It associates a `SimpleCode` with an existing `UserId`.
-   **In login mode** (`@UserId = 0`): It finds the user corresponding to the provided `SimpleCode` and validates the login.

### 2.3. Transformer on `CK.vUserAuthProvider`

The package registers itself with `CK.DB.Auth` via a TQL transformer:

```sql
-- File: Res/vUserAuthProvider.tql
create transformer on CK.vUserAuthProvider
as
begin
	inject "
	union all
	select UserId, 'SimpleCode', LastLoginTime from CK.tUserSimpleCode where UserId > 0
	" after first part {select};
end
```

## 3. C# API

### 3.1. `UserSimpleCodeTable` Class

This class implements `IGenericAuthenticationProvider<IUserSimpleCodeInfo>` and exposes methods for interacting with the database, such as `CreateOrUpdateSimpleCodeUserAsync`.

### 3.2. `IUserSimpleCodeInfo` Interface

This `IPoco` defines the data contract for this provider.

```csharp
// File: IUserSimpleCodeInfo.cs
public interface IUserSimpleCodeInfo : IPoco
{
    string SimpleCode { get; set; }
}
```
-   It serves as the "payload" for authentication operations.

## 4. Dependencies

-   **`CK.DB.Auth`**: Provides the base authentication framework.