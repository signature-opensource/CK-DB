-- Let UserProviderSchemaTableName be in Ordinal.
alter table CK.tAuthProvider
    alter column UserProviderSchemaTableName nvarchar(128) collate Latin1_General_100_BIN2 not null;
