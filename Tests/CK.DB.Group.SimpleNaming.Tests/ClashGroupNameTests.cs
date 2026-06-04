using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using CK.Testing;
using NUnit.Framework;
using Shouldly;
using System;

namespace CK.DB.Group.SimpleNaming.Tests;

[TestFixture]
public sealed class ClashGroupNameTests
{
    [TestCase( "", "" )]
    [TestCase( " (", ")" )]
    [TestCase( "--", "" )]
    [TestCase( "__", "" )]
    public void create_group_with_custom_pattern( string patternBefore, string patternAfter )
    {
        var map = SharedEngine.Map;
        var g = map.StObjs.Obtain<GroupTable>();
        var gN = map.StObjs.Obtain<SimpleNaming.Package>();
        using( var ctx = new SqlStandardCallContext() )
        {
            string theGroupName = Guid.NewGuid().ToString();

            int idMain = g.CreateGroup( ctx, 1 );
            gN.GroupRename( ctx, 1, idMain, theGroupName );

            int idClash = g.CreateGroup( ctx, 1 );
            object? corrected = g.Database.ExecuteScalar( "select CK.fGroupGroupNameComputeUnique( @0, @1, @2, @3 );",
                                                          idClash,
                                                          theGroupName,
                                                          patternBefore,
                                                          patternAfter );

            corrected.ShouldNotBeNull();
            corrected.ShouldBe( theGroupName + patternBefore + "1" + patternAfter );
        }
    }
}
