--[beginscript]

-- The claims as received from the provider: a JSON object (UTF-8, case sensitive), '{}' when none is known (the default
-- fills the existing rows: the old stack never provides claims).
-- The JSON object check is the SQL Server 2016+ form (ISJSON( Claims, OBJECT ) requires SQL Server 2022).
alter table CK.tUserOidc add
	Claims varchar(max) collate Latin1_General_100_BIN2_UTF8 not null
		constraint DF_CK_UserOidc_Claims default( '{}' ),
	constraint CK_CK_UserOidc_Claims check (ISJSON( Claims ) = 1 and left( Claims, 1 ) = '{');

--[endscript]
