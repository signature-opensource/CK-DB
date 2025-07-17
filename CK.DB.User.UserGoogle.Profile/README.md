# CK.DB.User.UserGoogle.Profile

## 1. Package Role

This package extends `CK.DB.User.UserGoogle` to store basic profile information for the user, such as their first name, last name, and a URL to their profile picture.

## 2. Extension Mechanism

This package transforms the objects of the `CK.DB.User.UserGoogle` package to add its features.

### 2.1. Extension of the `CK.tUserGoogle` Table

The package adds four columns to the `tUserGoogle` table via an `alter table` script.

```sql
-- File: Res/Model.CK.DB.User.UserGoogle.Profile.Package.Install.1.0.0.sql
alter table CK.tUserGoogle add
	FirstName nvarchar( 255 ) collate Latin1_General_100_CI_AS not null constraint DF_TEMP1 default(N''),
	LastName nvarchar( 255 ) collate Latin1_General_100_CI_AS not null constraint DF_TEMP2 default(N''),
	UserName nvarchar( 255 ) collate Latin1_General_100_CI_AS not null constraint DF_TEMP3 default(N''),
	PictureUrl varchar( 255 ) collate Latin1_General_100_BIN2 not null constraint DF_TEMP4 default('');
```
-   `FirstName`: The user's first name.
-   `LastName`: The user's last name.
-   `UserName`: The full name or nickname provided by Google.
-   `PictureUrl`: A URL to the user's avatar.

### 2.2. Extension of the `CK.sUserGoogleUCL` Procedure

To allow the new columns to be populated, the package transforms the `sUserGoogleUCL` procedure:

```sql
-- File: Res/sUserGoogleUCL.tql
create transformer on CK.sUserGoogleUCL
as
begin
	-- 1. Adds optional parameters to the procedure.
	add parameter @FirstName nvarchar(255) = null,
                  @LastName nvarchar(255) = null,
                  @UserName nvarchar(255) = null,
                  @PictureUrl varchar( 255 ) = null;

	-- 2. Injects the new columns into the INSERT statement.
	in single statement {insert into CK.tUserGoogle}
	begin
		add column	FirstName = case when @FirstName is not null then @FirstName else N'' end, 
					LastName = case when @LastName is not null then @LastName else N'' end, 
					UserName = case when @UserName is not null then @UserName else N'' end, 
					PictureUrl = case when @PictureUrl is not null then @PictureUrl else '' end;
	end

	-- 3. Injects the update logic into the UPDATE statement.
	in first statement {update CK.tUserGoogle}
	begin
		add column FirstName = case when @FirstName is not null then @FirstName else FirstName end, 
                   LastName = case when @LastName is not null then @LastName else LastName end, 
                   UserName = case when @UserName is not null then @UserName else UserName end, 
                   PictureUrl = case when @PictureUrl is not null then @PictureUrl else PictureUrl end;
	end
end
```
The update logic ensures that if the parameters are not provided during the call, the existing values in the database are preserved.

## 3. C# API

### 3.1. Extension of the `IUserGoogleInfo` Interface

The `IUserGoogleInfo` interface is extended to include the new properties.

```csharp
// File: IUserGoogleInfo.cs
public interface IUserGoogleInfo : UserGoogle.IUserGoogleInfo
{
    string? FirstName { get; set; }
    string? LastName { get; set; }
    string? UserName { get; set; }
    string? PictureUrl { get; set; }
}
```
The `Source Generator` automatically links these properties to the corresponding parameters of the transformed stored procedure.

## 4. Dependencies

-   **`CK.DB.User.UserGoogle`**: The base package that is extended.