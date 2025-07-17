# CK.DB.User.UserGoogle.EMailColumns

## 1. Package Role

This package extends `CK.DB.User.UserGoogle` to store user email information, as provided by Google during authentication.

## 2. Extension Mechanism

This package transforms the objects of the `CK.DB.User.UserGoogle` package to add its features.

### 2.1. Extension of the `CK.tUserGoogle` Table

The package adds two columns to the `tUserGoogle` table via an `alter table` script.

```sql
-- File: Res/Model.CK.DB.User.UserGoogle.EMailColumns.Package.Install.1.0.0.sql
alter table CK.tUserGoogle add
    EMail nvarchar( 255 ) collate Latin1_General_100_CI_AS not null constraint DF_TEMP1 default(N''),
    EMailVerified bit not null constraint DF_TEMP2 default(0);
```
-   `EMail`: The user's email address.
-   `EMailVerified`: A boolean indicating whether Google has verified this email address.

### 2.2. Extension of the `CK.sUserGoogleUCL` Procedure

To allow the new columns to be populated, the package transforms the `sUserGoogleUCL` procedure:

```sql
-- File: Res/sUserGoogleUCL.tql
create transformer on CK.sUserGoogleUCL
as
begin
	-- 1. Adds optional parameters to the procedure.
	add parameter @EMail nvarchar(255) = null, @EMailVerified bit = null;

	-- 2. Injects the new columns into the INSERT statement.
	in single statement {insert into CK.tUserGoogle}
	begin
		add column	EMail = case when @EMail is not null then @EMail else N'' end, 
					EMailVerified = case when @EMailVerified is not null then @EMailVerified else 0 end;
	end

	-- 3. Injects the update logic into the UPDATE statement.
	in first statement {update CK.tUserGoogle}
	begin
		add column	EMail = case when @EMail is not null then @EMail else EMail end, 
					EMailVerified = case when @EMailVerified is not null then @EMailVerified else EMailVerified end;
	end
end
```
The update logic (`case when @param is not null...`) ensures that if the parameters are not provided during the call, the existing values in the database are preserved.

## 3. C# API

### 3.1. Extension of the `IUserGoogleInfo` Interface

The `IUserGoogleInfo` interface is extended to include the new properties, allowing them to be easily manipulated from C# code.

```csharp
// File: IUserGoogleInfo.cs
public interface IUserGoogleInfo : UserGoogle.IUserGoogleInfo
{
    string? EMail { get; set; }
    bool? EMailVerified { get; set; }
}
```
The `Source Generator` automatically links these properties to the `@EMail` and `@EMailVerified` parameters of the transformed stored procedure.

## 4. Dependencies

-   **`CK.DB.User.UserGoogle`**: The base package that is extended.