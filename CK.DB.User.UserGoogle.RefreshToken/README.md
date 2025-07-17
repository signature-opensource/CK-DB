# CK.DB.User.UserGoogle.RefreshToken

## 1. Package Role

This package extends `CK.DB.User.UserGoogle` to add the ability to store and manage OAuth **refresh tokens**. These tokens allow an application to obtain new access tokens without requiring the user to log in again.

## 2. Extension Mechanism

This package transforms the objects of the `CK.DB.User.UserGoogle` package to add its features.

### 2.1. Extension of the `CK.tUserGoogle` Table

The package adds two columns to the `tUserGoogle` table via an `alter table` script.

```sql
-- File: Res/Model.CK.DB.User.UserGoogle.RefreshToken.Package.Install.1.0.0.sql
alter table CK.tUserGoogle add
	RefreshToken varchar(max) collate Latin1_General_100_CI_AS not null constraint DF_TEMP1 default(N''),
	LastRefreshTokenTime datetime2(2) not null constraint DF_TEMP2 default('0001-01-01');
```
-   `RefreshToken`: The refresh token, which can be a very long string.
-   `LastRefreshTokenTime`: The timestamp of the last token update.

### 2.2. Extension of the `CK.sUserGoogleUCL` Procedure

To allow the token to be saved, the package transforms the `sUserGoogleUCL` procedure:

```sql
-- File: Res/sUserGoogleUCL.tql
create transformer on CK.sUserGoogleUCL
as
begin
	-- 1. Adds the optional parameter to the procedure.
	add parameter @RefreshToken nvarchar(max) = null;

	-- 2. Injects the new columns into the INSERT statement.
	in single statement {insert into CK.tUserGoogle}
	begin
		add column RefreshToken = case when @RefreshToken is not null then @RefreshToken else '' end,
				   LastRefreshTokenTime = case when @RefreshToken is not null then @Now else '0001-01-01' end;
	end

	-- 3. Injects the update logic into the UPDATE statement.
	in first statement {update CK.tUserGoogle}
	begin
		add column RefreshToken = case when @RefreshToken is not null then @RefreshToken else RefreshToken end, 
				   LastRefreshTokenTime = case when @RefreshToken is not null and RefreshToken <> @RefreshToken 
											then @Now
											else LastRefreshTokenTime
										   end;
	end
end
```
The update logic is designed to:
-   Save the new token if it is provided.
-   Update `LastRefreshTokenTime` only if the token has actually changed, to avoid unnecessary writes.

## 3. C# API

### 3.1. Extension of the `IUserGoogleInfo` Interface

The `IUserGoogleInfo` interface is extended to include the new properties.

```csharp
// File: IUserGoogleInfo.cs
public interface IUserGoogleInfo : UserGoogle.IUserGoogleInfo
{
    DateTime LastRefreshTokenTime { get; set; }
    string? RefreshToken { get; set; }
}
```
The `Source Generator` automatically links these properties to the `@RefreshToken` parameter of the transformed stored procedure.

## 4. Dependencies

-   **`CK.DB.User.UserGoogle`**: The base package that is extended.