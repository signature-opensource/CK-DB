# CK.DB.User.UserGoogle.AuthScope

## 1. Package Role

This package extends `CK.DB.User.UserGoogle` to add management of **authorization scopes**, a fundamental concept in authentication flows like OAuth. It allows associating a specific set of permissions with each Google user.

To do this, it relies on the `CK.DB.Auth.AuthScope` package, which provides the basic mechanism for managing scope sets (`ScopeSet`).

## 2. Extension Mechanism

This package is a perfect example of the CK-DB extensibility model. It does not contain new tables but **transforms** the objects of the `CK.DB.User.UserGoogle` package.

### 2.1. Extension of the `CK.tUserGoogle` Table

The package adds a `ScopeSetId` column to the `tUserGoogle` table via an `alter table` script.

```sql
-- File: Res/Model.CK.DB.User.UserGoogle.AuthScope.Package.Install.1.0.0.sql
alter table CK.tUserGoogle add
	ScopeSetId int not null constraint DF_TEMP default(0);
	 
alter table CK.tUserGoogle add
	constraint FK_CK_UserGoogle_ScopeSetId foreign key (ScopeSetId) references CK.tAuthScopeSet(ScopeSetId);
```
-   `ScopeSetId`: Foreign key to the `CK.tAuthScopeSet` table (from the `CK.DB.Auth.AuthScope` package). Each Google user is thus assigned their own set of scopes.

### 2.2. Extension of the `CK.sUserGoogleUCL` Procedure

When a new Google user is created, they must be assigned a new set of scopes. This package uses a TQL transformer to inject this logic into the creation procedure.

```sql
-- File: Res/sUserGoogleUCL.tql
create transformer on CK.sUserGoogleUCL
as
begin
	-- 1. Injects scope copy logic BEFORE user creation.
	inject "
		declare @DefaultScopeSetId int;
		select @DefaultScopeSetId = ScopeSetId from CK.tUserGoogle where UserId = 0;
		declare @NewScopeSetId int;
		exec CK.sAuthScopeSetCopy @ActorId, @DefaultScopeSetId, 'W', @NewScopeSetId output;"
	into "PreCreate";
		
	-- 2. Adds the ScopeSetId column to the INSERT statement.
	in single part {insert into CK.tUserGoogle}
	begin
		add column ScopeSetId = @NewScopeSetId;
	end
end
```
The logic is as follows:
1.  Before inserting the new user, retrieve the `ScopeSetId` of the "template" user (UserId = 0).
2.  Use the `CK.sAuthScopeSetCopy` procedure (provided by `CK.DB.Auth.AuthScope`) to create a copy of this scope set.
3.  Insert the `UserId` into `tUserGoogle` with their new personal `ScopeSetId`.

The `ScopeSetId` is intrinsic: it is defined at creation and is never modified afterward.

## 3. C# API

### 3.1. Extension of the `IUserGoogleInfo` Interface

This package extends the `IUserGoogleInfo` interface to expose the `ScopeSetId`.

```csharp
// File: IUserGoogleInfo.cs
public interface IUserGoogleInfo : UserGoogle.IUserGoogleInfo
{
    int ScopeSetId { get; set; }
}
```
The `Source Generator` detects this new property and automatically includes it in the C# payload used by the stored procedures.

## 4. Dependencies

-   **`CK.DB.User.UserGoogle`**: The base package that is extended.
-   **`CK.DB.Auth.AuthScope`**: Provides the `ScopeSet` functionality (the `tAuthScopeSet` table and the `sAuthScopeSetCopy` procedure).