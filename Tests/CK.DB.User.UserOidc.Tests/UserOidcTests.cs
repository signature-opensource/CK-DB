using System;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using CK.DB.Auth;
using System.Collections.Generic;
using Shouldly;
using CK.Testing;

namespace CK.DB.User.UserOidc.Tests;

[TestFixture]
public class UserOidcTests
{
    [TestCase( "" )]
    [TestCase( "IdSrv" )]
    public void create_Oidc_user_and_check_read_info_object_method( string schemeSuffix )
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserOidcTable>().ShouldNotBeNull();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>().ShouldNotBeNull();
        var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>().ShouldNotBeNull();
        using( var ctx = new SqlStandardCallContext() )
        {
            var userName = Guid.NewGuid().ToString();
            int userId = user.CreateUser( ctx, 1, userName );
            var sub = Guid.NewGuid().ToString( "N" );

            var info = infoFactory.Create();
            info.SchemeSuffix = schemeSuffix;
            info.Sub = sub;
            var created = u.CreateOrUpdateOidcUser( ctx, 1, userId, info );
            created.OperationResult.ShouldBe( UCResult.Created );
            var info2 = u.FindKnownUserInfo( ctx, schemeSuffix, sub ).ShouldNotBeNull();

            info2.UserId.ShouldBe( userId );
            info2.Info.SchemeSuffix.ShouldBe( schemeSuffix );
            info2.Info.Sub.ShouldBe( sub );

            u.FindKnownUserInfo( ctx, schemeSuffix, Guid.NewGuid().ToString() ).ShouldBeNull();
            user.DestroyUser( ctx, 1, userId );
            u.FindKnownUserInfo( ctx, schemeSuffix, sub ).ShouldBeNull();
        }
    }

    [TestCase( "" )]
    [TestCase( "IdSrv" )]
    public async Task create_Oidc_user_and_check_read_info_object_method_Async( string schemeSuffix )
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserOidcTable>().ShouldNotBeNull();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>().ShouldNotBeNull();
        var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>().ShouldNotBeNull();
        using( var ctx = new SqlStandardCallContext() )
        {
            var userName = Guid.NewGuid().ToString();
            int userId = await user.CreateUserAsync( ctx, 1, userName );
            var sub = Guid.NewGuid().ToString( "N" );

            var info = infoFactory.Create();
            info.SchemeSuffix = schemeSuffix;
            info.Sub = sub;
            var created = await u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, info );
            created.OperationResult.ShouldBe( UCResult.Created );
            var info2 = (await u.FindKnownUserInfoAsync( ctx, schemeSuffix, sub )).ShouldNotBeNull();

            info2.UserId.ShouldBe( userId );
            info2.Info.SchemeSuffix.ShouldBe( schemeSuffix );
            info2.Info.Sub.ShouldBe( sub );

            (await u.FindKnownUserInfoAsync( ctx, schemeSuffix, Guid.NewGuid().ToString() )).ShouldBeNull();
            await user.DestroyUserAsync( ctx, 1, userId );
            (await u.FindKnownUserInfoAsync( ctx, schemeSuffix, sub )).ShouldBeNull();
        }
    }

    [Test]
    public async Task the_claims_are_stored_as_received_and_replaced_by_an_actual_login_Async()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserOidcTable>().ShouldNotBeNull();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>().ShouldNotBeNull();
        var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>().ShouldNotBeNull();
        using var ctx = new SqlStandardCallContext();
        int userId = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
        var sub = Guid.NewGuid().ToString( "N" );
        IUserOidcInfo Info( string claims )
        {
            var i = infoFactory.Create();
            i.SchemeSuffix = "Claims";
            i.Sub = sub;
            i.Claims = claims;
            return i;
        }
        async Task<string> ReadAsync() => (await u.FindKnownUserInfoAsync( ctx, "Claims", sub )).ShouldNotBeNull().Info.Claims;

        // Created with claims: any character is kept (UTF-8 column, nvarchar parameter).
        const string first = """{"name":"Zoë Çağlar 王","email_verified":true,"address":{"country":"FR"},"age":42}""";
        (await u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, Info( first ) )).OperationResult.ShouldBe( UCResult.Created );
        (await ReadAsync()).ShouldBe( first );
        var read = (await u.FindKnownUserInfoAsync( ctx, "Claims", sub )).ShouldNotBeNull().Info;
        read.TryGetClaim( "name", out var name ).ShouldBeTrue();
        name.GetString().ShouldBe( "Zoë Çağlar 王" );
        read.TryGetClaim( "email_verified", out var verified ).ShouldBeTrue();
        verified.GetBoolean().ShouldBeTrue( "A boolean stays a boolean." );
        read.TryGetClaim( "address", out var address ).ShouldBeTrue();
        address.GetProperty( "country" ).GetString().ShouldBe( "FR" );
        read.TryGetClaim( "missing", out _ ).ShouldBeFalse();

        // A check-only login doesn't change them, an actual login replaces them.
        const string second = """{"name":"Zoë"}""";
        (await u.LoginUserAsync( ctx, Info( second ), actualLogin: false )).IsSuccess.ShouldBeTrue();
        (await ReadAsync()).ShouldBe( first, "No side effect at a check." );
        (await u.LoginUserAsync( ctx, Info( second ), actualLogin: true )).IsSuccess.ShouldBeTrue();
        (await ReadAsync()).ShouldBe( second );

        // Not provided (the old stack): the stored claims are kept.
        (await u.LoginUserAsync( ctx, Info( "" ), actualLogin: true )).IsSuccess.ShouldBeTrue();
        (await ReadAsync()).ShouldBe( second );

        // Not a JSON object: refused.
        await Util.Awaitable( () => u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, Info( "not json" ) ) ).ShouldThrowAsync<Exception>();
        await Util.Awaitable( () => u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, Info( "[1,2]" ) ) ).ShouldThrowAsync<Exception>();
        (await ReadAsync()).ShouldBe( second );

        // Created without claims: "{}" (no claims known).
        await u.DestroyOidcUserAsync( ctx, 1, userId, "Claims" );
        (await u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, Info( "" ) )).OperationResult.ShouldBe( UCResult.Created );
        (await ReadAsync()).ShouldBe( "{}" );
        await user.DestroyUserAsync( ctx, 1, userId );
    }

    [Test]
    public async Task the_last_login_method_is_detected_by_CK_DB_Auth_Async()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserOidcTable>().ShouldNotBeNull();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>().ShouldNotBeNull();
        var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>().ShouldNotBeNull();
        var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>().ShouldNotBeNull();
        using var ctx = new SqlStandardCallContext();
        int userId = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
        IUserOidcInfo Info( string suffix )
        {
            var i = infoFactory.Create();
            i.SchemeSuffix = suffix;
            i.Sub = Guid.NewGuid().ToString( "N" );
            return i;
        }
        (await auth.HasOtherSchemeAsync( ctx, userId, "Oidc.A" )).ShouldBeFalse( "No login method at all." );
        await u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, Info( "A" ) );
        (await auth.HasOtherSchemeAsync( ctx, userId, "Oidc.A" )).ShouldBeFalse( "Oidc.A is the last login method: its removal must be refused." );
        (await auth.HasOtherSchemeAsync( ctx, userId, "Basic" )).ShouldBeTrue();
        await u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, Info( "B" ) );
        (await auth.HasOtherSchemeAsync( ctx, userId, "Oidc.A" )).ShouldBeTrue();
        auth.HasOtherScheme( ctx, userId, "Oidc.B" ).ShouldBeTrue();
        await user.DestroyUserAsync( ctx, 1, userId );
    }

    [Test]
    public async Task the_identities_are_listed_and_unlinked_but_never_the_last_login_method_Async()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserOidcTable>().ShouldNotBeNull();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>().ShouldNotBeNull();
        var pocos = SharedEngine.Map.StObjs.Obtain<PocoDirectory>().ShouldNotBeNull();
        var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>().ShouldNotBeNull();
        using var ctx = new SqlStandardCallContext();
        int userId = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
        IUserOidcInfo Info( string suffix, string claims )
        {
            var i = infoFactory.Create();
            i.SchemeSuffix = suffix;
            i.Sub = Guid.NewGuid().ToString( "N" );
            i.Claims = claims;
            return i;
        }
        await u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, Info( "A", """{"email":"albert@a.example"}""" ) );
        await u.CreateOrUpdateOidcUserAsync( ctx, 1, userId, Info( "B", "" ) );

        var identities = await u.ReadUserIdentitiesAsync( ctx, pocos, userId );
        identities.Count.ShouldBe( 2 );
        identities[0].SchemeSuffix.ShouldBe( "A" );
        identities[0].Claims.ShouldBe( """{"email":"albert@a.example"}""" );
        identities[1].SchemeSuffix.ShouldBe( "B" );
        identities[1].Claims.ShouldBe( "{}", "No claims known." );

        (await u.UnlinkIdentityAsync( ctx, userId, "Unknown" )).ShouldBe( OidcUnlinkResult.NotFound );
        (await u.UnlinkIdentityAsync( ctx, userId, "A" )).ShouldBe( OidcUnlinkResult.Unlinked );
        (await u.UnlinkIdentityAsync( ctx, userId, "B" )).ShouldBe( OidcUnlinkResult.LastLoginMethod );
        (await u.ReadUserIdentitiesAsync( ctx, pocos, userId )).ShouldHaveSingleItem().SchemeSuffix.ShouldBe( "B" );
        await user.DestroyUserAsync( ctx, 1, userId );
    }

    [Test]
    public void Oidc_AuthProvider_is_registered()
    {
        Auth.Tests.AuthTests.CheckProviderRegistration( "Oidc" );
    }

    [TestCase( "" )]
    [TestCase( "IdSrv" )]
    public void vUserAuthProvider_reflects_the_user_Oidc_authentication( string schemeSuffix )
    {
        string scheme = schemeSuffix.Length > 0 ? "Oidc." + schemeSuffix : "Oidc";
        var u = SharedEngine.Map.StObjs.Obtain<UserOidcTable>().ShouldNotBeNull();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>().ShouldNotBeNull();
        using( var ctx = new SqlStandardCallContext() )
        {
            string userName = "Oidc auth - " + Guid.NewGuid().ToString();
            var sub = Guid.NewGuid().ToString( "N" );
            var idU = user.CreateUser( ctx, 1, userName );
            u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='{scheme}'" ).ShouldNotBeNull()
                .Rows.ShouldBeEmpty();
            var info = u.CreateUserInfo<IUserOidcInfo>();
            info.SchemeSuffix = schemeSuffix;
            info.Sub = sub;
            u.CreateOrUpdateOidcUser( ctx, 1, idU, info );
            u.Database.ExecuteScalar( $"select count(*) from CK.vUserAuthProvider where UserId={idU} and Scheme='{scheme}'" )
                .ShouldBe( 1 );
            u.DestroyOidcUser( ctx, 1, idU, schemeSuffix );
            u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='{scheme}'" ).ShouldNotBeNull()
                  .Rows.ShouldBeEmpty();
        }
    }

    [TestCase( "" )]
    [TestCase( "IdSrv" )]
    public void standard_generic_tests_for_Oidc_provider( string schemeSuffix )
    {
        string scheme = schemeSuffix.Length > 0 ? "Oidc." + schemeSuffix : "Oidc";

        var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>().ShouldNotBeNull();
        // With IUserOidcInfo POCO.
        var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>().ShouldNotBeNull();
        CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
            auth,
            scheme,
            payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "OidcAccountIdFor:" + userName; } ),
            payloadForLogin: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "OidcAccountIdFor:" + userName; } ),
            payloadForLoginFail: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "NO!" + userName; } )
            );

        // With KeyValuePairs.
        CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
            auth,
            scheme,
            payloadForCreateOrUpdate: ( userId, userName ) => new[]
            {
                new KeyValuePair<string,object>( "SchemeSuffix", schemeSuffix ),
                new KeyValuePair<string,object>( "Sub", "IdFor:" + userName)
            },
            payloadForLogin: ( userId, userName ) => new[]
            {
               new KeyValuePair<string,object>( "Sub", "IdFor:" + userName),
                 new KeyValuePair<string,object>( "SchemeSuffix", schemeSuffix ),
            },
            payloadForLoginFail: ( userId, userName ) => new[]
            {
                new KeyValuePair<string,object>( "Sub", ("IdFor:" + userName).ToUpperInvariant()),
                 new KeyValuePair<string,object>( "SchemeSuffix", schemeSuffix ),
            } );

        // With ValueTuples.
        CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
            auth,
            scheme,
            payloadForCreateOrUpdate: ( userId, userName ) => new[]
            {
                ( "SchemeSuffix", schemeSuffix ),
                ( "Sub", "IdFor:" + userName)
            },
            payloadForLogin: ( userId, userName ) => new[]
            {
               ( "Sub", "IdFor:" + userName),
               ( "SchemeSuffix", schemeSuffix ),
            },
            payloadForLoginFail: ( userId, userName ) => new[]
            {
                ( "Sub", ("IdFor:" + userName).ToUpperInvariant()),
                ( "SchemeSuffix", schemeSuffix ),
            } );
    }

    [TestCase( "" )]
    [TestCase( "IdSrv" )]
    public async Task standard_generic_tests_for_Oidc_provider_Async( string schemeSuffix )
    {
        string scheme = schemeSuffix.Length > 0 ? "Oidc." + schemeSuffix : "Oidc";
        var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>().ShouldNotBeNull();
        var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserOidcInfo>>().ShouldNotBeNull();
        await Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProviderAsync(
            auth,
            scheme,
            payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "OidcAccountIdFor:" + userName; } ),
            payloadForLogin: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "OidcAccountIdFor:" + userName; } ),
            payloadForLoginFail: ( userId, userName ) => f.Create( i => { i.SchemeSuffix = schemeSuffix; i.Sub = "NO!" + userName; } )
            );
    }

}

