using CK.Core;
using CK.SqlServer;
using CK.Testing;
using NUnit.Framework;
using Shouldly;
using System;

namespace CK.DB.Culture.Tests;

[TestFixture]
public class BazookationTests
{
    // Legacy seed values present in CK.tLCID / CK.tXLCID (CK.DB.Culture).
    const int LegacyEnLCID = 9;
    const int LegacyFrLCID = 12;

    // New ids seeded by CK.DB.Globalization in CK.tCulture / CK.tExtendedCulture.
    const int EnCultureId = 221272233;
    const int FrCultureId = 210327884;

    [Test]
    public void Culture_bazookation_remaps_LCID_references_and_renames_column_to_CultureId()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        var legacyTable = "tCKBazookaLCID_" + Guid.NewGuid().ToString( "N" ).Substring( 0, 8 );

        try
        {
            // Legacy-style table referencing CK.tLCID.LCID. tLCID has no inbound FK from other
            // CK tables on a fresh setup, so the bazooka cascade only touches this test table.
            p.Database.ExecuteNonQuery( $@"
create table CK.{legacyTable}
(
    Id int not null identity( 1, 1 ),
    LCID int not null,
    constraint PK_{legacyTable} primary key( Id ),
    constraint FK_{legacyTable}_LCID foreign key( LCID ) references CK.tLCID( LCID )
);
insert into CK.{legacyTable}( LCID ) values( {LegacyEnLCID} );
insert into CK.{legacyTable}( LCID ) values( {LegacyFrLCID} );" );

            // sRefBazookation rewrites legacy LCID values to the new CultureId values
            // in every table whose FK points at CK.tLCID.LCID.
            p.Database.ExecuteNonQuery( $@"
exec CKCore.sRefBazookation 'CK', 'tLCID', 'LCID', '{LegacyEnLCID}', '{EnCultureId}', 0;
exec CKCore.sRefBazookation 'CK', 'tLCID', 'LCID', '{LegacyFrLCID}', '{FrCultureId}', 0;" );

            p.Database.ExecuteScalar( $"select count(*) from CK.{legacyTable} where LCID = {EnCultureId};" )
                .ShouldBe( 1 );
            p.Database.ExecuteScalar( $"select count(*) from CK.{legacyTable} where LCID = {FrCultureId};" )
                .ShouldBe( 1 );

            // sColumnBazookation renames the column LCID -> CultureId and retargets the FK
            // from CK.tLCID(LCID) to CK.tCulture(CultureId).
            p.Database.ExecuteNonQuery( @"
exec CKCore.sColumnBazookation
    'CK', 'tLCID', 'LCID',
    'CK', 'tCulture', 'CultureId',
    'FK_CK_{SOURCETABLE}_CultureId foreign key (CultureId) references CK.tCulture(CultureId)';" );

            p.Database.ExecuteScalar( $@"
select count(*) from sys.columns c
inner join sys.tables t on t.object_id = c.object_id
inner join sys.schemas s on s.schema_id = t.schema_id
where s.name = 'CK' and t.name = '{legacyTable}' and c.name = 'CultureId';" )
                .ShouldBe( 1 );
            p.Database.ExecuteScalar( $@"
select count(*) from sys.columns c
inner join sys.tables t on t.object_id = c.object_id
where t.name = '{legacyTable}' and c.name = 'LCID';" )
                .ShouldBe( 0 );

            p.Database.ExecuteScalar( $@"
select count(*) from sys.foreign_keys fk
inner join sys.tables pt on pt.object_id = fk.parent_object_id
inner join sys.tables rt on rt.object_id = fk.referenced_object_id
where pt.name = '{legacyTable}' and rt.name = 'tCulture';" )
                .ShouldBe( 1 );

            p.Database.ExecuteScalar( $"select count(*) from CK.{legacyTable} where CultureId = {EnCultureId};" )
                .ShouldBe( 1 );
        }
        finally
        {
            p.Database.ExecuteNonQuery( $"if object_id('CK.{legacyTable}') is not null drop table CK.{legacyTable};" );
        }
    }

    [Test]
    public void ExtendedCulture_bazookation_remaps_XLCID_references_and_renames_column_to_ExtendedCultureId()
    {
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        var legacyTable = "tCKBazookaXLCID_" + Guid.NewGuid().ToString( "N" ).Substring( 0, 8 );

        // CK.tXLCID has inbound FKs from CK.tLCID and CK.tXLCIDMap. The bazooka would also
        // walk into tLCID (CHECK LCID < 0xFFFF blocks remapping to a new ExtendedCultureId)
        // and destructively rewrite tXLCIDMap. The sColumnBazookation PK-composite logic
        // ALSO picks up tXLCIDMap independently (its PK contains an "XLCID" column) which
        // breaks the rename. Drop legacy back-references and tXLCIDMap for the duration of
        // the test, restore both at the end.
        p.Database.ExecuteNonQuery( @"
alter table CK.tLCID drop constraint FK_CK_LCID_XLCID;
drop table CK.tXLCIDMap;" );

        try
        {
            p.Database.ExecuteNonQuery( $@"
create table CK.{legacyTable}
(
    Id int not null identity( 1, 1 ),
    XLCID int not null,
    constraint PK_{legacyTable} primary key( Id ),
    constraint FK_{legacyTable}_XLCID foreign key( XLCID ) references CK.tXLCID( XLCID )
);
insert into CK.{legacyTable}( XLCID ) values( {LegacyEnLCID} );
insert into CK.{legacyTable}( XLCID ) values( {LegacyFrLCID} );" );

            p.Database.ExecuteNonQuery( $@"
exec CKCore.sRefBazookation 'CK', 'tXLCID', 'XLCID', '{LegacyEnLCID}', '{EnCultureId}', 0;
exec CKCore.sRefBazookation 'CK', 'tXLCID', 'XLCID', '{LegacyFrLCID}', '{FrCultureId}', 0;" );

            p.Database.ExecuteScalar( $"select count(*) from CK.{legacyTable} where XLCID = {EnCultureId};" )
                .ShouldBe( 1 );
            p.Database.ExecuteScalar( $"select count(*) from CK.{legacyTable} where XLCID = {FrCultureId};" )
                .ShouldBe( 1 );

            p.Database.ExecuteNonQuery( @"
exec CKCore.sColumnBazookation
    'CK', 'tXLCID', 'XLCID',
    'CK', 'tExtendedCulture', 'ExtendedCultureId',
    'FK_CK_{SOURCETABLE}_ExtendedCultureId foreign key (ExtendedCultureId) references CK.tExtendedCulture(ExtendedCultureId)';" );

            p.Database.ExecuteScalar( $@"
select count(*) from sys.columns c
inner join sys.tables t on t.object_id = c.object_id
inner join sys.schemas s on s.schema_id = t.schema_id
where s.name = 'CK' and t.name = '{legacyTable}' and c.name = 'ExtendedCultureId';" )
                .ShouldBe( 1 );
            p.Database.ExecuteScalar( $@"
select count(*) from sys.columns c
inner join sys.tables t on t.object_id = c.object_id
where t.name = '{legacyTable}' and c.name = 'XLCID';" )
                .ShouldBe( 0 );

            p.Database.ExecuteScalar( $@"
select count(*) from sys.foreign_keys fk
inner join sys.tables pt on pt.object_id = fk.parent_object_id
inner join sys.tables rt on rt.object_id = fk.referenced_object_id
where pt.name = '{legacyTable}' and rt.name = 'tExtendedCulture';" )
                .ShouldBe( 1 );

            p.Database.ExecuteScalar( $"select count(*) from CK.{legacyTable} where ExtendedCultureId = {EnCultureId};" )
                .ShouldBe( 1 );
            p.Database.ExecuteScalar( $"select count(*) from CK.{legacyTable} where ExtendedCultureId = {FrCultureId};" )
                .ShouldBe( 1 );
        }
        finally
        {
            // Drop the test table first so its FK no longer holds tXLCID rows down.
            p.Database.ExecuteNonQuery( $"if object_id('CK.{legacyTable}') is not null drop table CK.{legacyTable};" );
            // Restore tXLCIDMap and the legacy back-reference on tLCID. Use NOCHECK on the
            // tLCID FK so any non-legacy rows present in tXLCID don't break validation.
            p.Database.ExecuteNonQuery( @"
if object_id('CK.tXLCIDMap') is null
begin
    create table CK.tXLCIDMap
    (
        XLCID int not null,
        Idx smallint not null,
        LCID int not null,
        constraint PK_CK_XLCIDMap primary key (XLCID, Idx),
        constraint FK_CK_XLCIDMap_XLCID foreign key( XLCID ) references CK.tXLCID( XLCID ),
        constraint FK_CK_XLCIDMap_LCID foreign key( LCID ) references CK.tLCID( LCID )
    );
end
alter table CK.tLCID with nocheck
    add constraint FK_CK_LCID_XLCID foreign key( LCID ) references CK.tXLCID( XLCID );" );
        }
    }
}
