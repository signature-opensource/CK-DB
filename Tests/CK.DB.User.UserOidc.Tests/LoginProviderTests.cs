using CK.Core;
using CK.DB.Actor;
using CK.DB.Auth;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserOidc.Tests;

/// <summary>
/// Tests the CK.AuthenticationInfo login and account binding of CK.DB.Auth with a generic provider (the OpenID Connect
/// one: its payload is created by the provider and filled from a remote assertion).
/// </summary>
[TestFixture]
public class LoginProviderTests
{
    const string Scheme = "Oidc.Login";

    static async Task<int> CreateUserAsync()
    {
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>().ShouldNotBeNull();
        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
        return await user.CreateUserAsync( ctx, 1, "Login-" + Guid.NewGuid().ToString() );
    }

    static async Task RegisterAsync( int userId, string sub )
    {
        var table = SharedEngine.Map.StObjs.Obtain<UserOidcTable>().ShouldNotBeNull();
        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
        var info = table.CreateUserInfo<IUserOidcInfo>();
        info.SchemeSuffix = "Login";
        info.Sub = sub;
        await table.CreateOrUpdateOidcUserAsync( ctx, 1, userId, info );
    }

    static IUserOidcInfo Payload( ILoginProvider p, string sub )
    {
        var payload = (IUserOidcInfo)p.CreatePayload( Scheme );
        payload.SchemeSuffix = "Login";
        payload.Sub = sub;
        return payload;
    }

    [Test]
    public async Task SqlLoginProvider_logs_in_with_a_generic_provider_Async()
    {
        var p = SharedEngine.AutomaticServices.GetRequiredService<SqlLoginProvider>();
        p.ProviderNames.ShouldContain( "Oidc" );
        var userId = await CreateUserAsync();
        var sub = "Sub-" + Guid.NewGuid().ToString( "N" );

        var unregistered = await p.LoginAsync( TestHelper.Monitor, Scheme, Payload( p, sub ) );
        unregistered.IsSuccess.ShouldBeFalse();
        unregistered.IsUnregisteredUser.ShouldBeTrue();

        await RegisterAsync( userId, sub );
        (await p.LoginAsync( TestHelper.Monitor, Scheme, Payload( p, sub ), actualLogin: false )).IsSuccess.ShouldBeTrue();
        var success = await p.LoginAsync( TestHelper.Monitor, Scheme, Payload( p, sub ) );
        success.IsSuccess.ShouldBeTrue();
        success.Actor!.ActorId.ShouldBe( userId );
        success.Actor.Level.ShouldBe( AuthLevel.Critical );
        success.Actor.HasScheme( Scheme ).ShouldBeTrue();

        var failed = await p.LoginAsync( TestHelper.Monitor, Scheme, Payload( p, "NO!" + sub ) );
        failed.IsSuccess.ShouldBeFalse();
        failed.FailureCode.ShouldBeGreaterThan( 0 );
    }

    [Test]
    public async Task SqlAutoBindingAccountService_binds_the_scheme_to_the_Critical_user_Async()
    {
        var p = SharedEngine.AutomaticServices.GetRequiredService<SqlLoginProvider>();
        var binder = SharedEngine.AutomaticServices.GetRequiredService<SqlAutoBindingAccountService>();
        var users = SharedEngine.AutomaticServices.GetRequiredService<IActorInfoProvider>();
        var userId = await CreateUserAsync();
        var user = (await users.GetActorInfoAsync( TestHelper.Monitor, userId ))!;
        var sub = "Bind-" + Guid.NewGuid().ToString( "N" );
        var now = DateTime.UtcNow;

        // Normal level is not enough by default.
        var normal = AuthenticationInfo.Create( user, now.AddHours( 1 ), null, null, now );
        var c1 = new LoginContext( Scheme, Payload( p, sub ), normal );
        (await binder.BindAccountAsync( TestHelper.Monitor, c1 )).ShouldBeNull();
        c1.ErrorId.ShouldBe( "User.AccountBinding.CriticalLevelRequired" );

        // Only a provider whose payload comes from a remote assertion can be bound (the Basic scheme cannot: its refusal is
        // tested by CK.DB.User.UserPassword).
        IAuthenticationDatabaseService db = SharedEngine.Map.StObjs.Obtain<Auth.Package>().ShouldNotBeNull();
        db.FindRequiredProvider( Scheme, mustHavePayload: false ).CanCreatePayload.ShouldBeTrue();
        var critical = AuthenticationInfo.Create( user, now.AddHours( 1 ), now.AddMinutes( 5 ), null, now );

        // Critical: the scheme is bound and the user is logged in.
        var c3 = new LoginContext( Scheme, Payload( p, sub ), critical );
        var bound = (await binder.BindAccountAsync( TestHelper.Monitor, c3 )).ShouldNotBeNull();
        c3.HasError.ShouldBeFalse();
        bound.IsSuccess.ShouldBeTrue();
        bound.Actor!.ActorId.ShouldBe( userId );
        bound.Actor.HasScheme( Scheme ).ShouldBeTrue();

        (await p.LoginAsync( TestHelper.Monitor, Scheme, Payload( p, sub ) )).Actor!.ActorId.ShouldBe( userId );
    }
}
