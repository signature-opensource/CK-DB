using CK.Core;
using CK.SqlServer;
using CK.Testing;
using Shouldly;
using NUnit.Framework;

namespace CK.DB.Res.ResText.Tests;

[TestFixture]
public class ResTextTests
{
    [Test]
    public void setting_and_clearing_resource_string()
    {
        var t = SharedEngine.Map.StObjs.Obtain<ResTextTable>().ShouldNotBeNull();
        using( var ctx = new SqlStandardCallContext() )
        {
            int resId = t.ResTable.Create( ctx );
            t.Database.ExecuteReader( "select * from CK.tResText where ResId = @0", resId ).ShouldNotBeNull()
                .Rows.ShouldBeEmpty();
            t.SetText( ctx, resId, "Hello World!" );
            t.Database.ExecuteScalar( "select Value from CK.tResText where ResId = @0", resId )
                .ShouldBe( "Hello World!" );
            t.SetText( ctx, resId, null );
            t.Database.ExecuteReader( "select * from CK.tResText where ResId = @0", resId ).ShouldNotBeNull()
                .Rows.ShouldBeEmpty();
            t.SetText( ctx, resId, "Hello World!" );
            t.ResTable.Destroy( ctx, resId );
        }
    }

    [Test]
    public void negative_resource_0_and_1_can_not_be_changed()
    {
        var t = SharedEngine.Map.StObjs.Obtain<ResTextTable>().ShouldNotBeNull();
        using( var ctx = new SqlStandardCallContext() )
        {
            Should.Throw<SqlDetailedException>( () => t.SetText( ctx, -1, "No way" ) );
            Should.Throw<SqlDetailedException>( () => t.SetText( ctx, 0, "No way" ) );
            Should.Throw<SqlDetailedException>( () => t.SetText(ctx, 1, "No way") );
        }
    }
}
