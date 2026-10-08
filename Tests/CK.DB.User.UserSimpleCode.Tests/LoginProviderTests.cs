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

namespace CK.DB.User.UserSimpleCode.Tests;

/// <summary>
/// Tests the CK.AuthenticationInfo login and account binding of CK.DB.Auth with a generic provider.
/// </summary>
[TestFixture]
public class LoginProviderTests
{
    static async Task<int> CreateUserAsync()
    {
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>().ShouldNotBeNull();
        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
        return await user.CreateUserAsync( ctx, 1, "Login-" + Guid.NewGuid().ToString() );
    }

    static async Task RegisterAsync( int userId, string code )
    {
        var table = SharedEngine.Map.StObjs.Obtain<UserSimpleCodeTable>().ShouldNotBeNull();
        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
        var info = table.CreateUserInfo<IUserSimpleCodeInfo>();
        info.SimpleCode = code;
        await table.CreateOrUpdateSimpleCodeUserAsync( ctx, 1, userId, info );
    }

    static IUserSimpleCodeInfo Payload( ILoginProvider p, string code )
    {
        var payload = (IUserSimpleCodeInfo)p.CreatePayload( "SimpleCode" );
        payload.SimpleCode = code;
        return payload;
    }

    [Test]
    public async Task SqlLoginProvider_logs_in_with_a_generic_provider_Async()
    {
        var p = SharedEngine.AutomaticServices.GetRequiredService<SqlLoginProvider>();
        p.ProviderNames.ShouldContain( "SimpleCode" );
        var userId = await CreateUserAsync();
        var code = "Code-" + Guid.NewGuid().ToString( "N" );

        var unregistered = await p.LoginAsync( TestHelper.Monitor, "SimpleCode", Payload( p, code ) );
        unregistered.IsSuccess.ShouldBeFalse();
        unregistered.IsUnregisteredUser.ShouldBeTrue();

        await RegisterAsync( userId, code );
        (await p.LoginAsync( TestHelper.Monitor, "SimpleCode", Payload( p, code ), actualLogin: false )).IsSuccess.ShouldBeTrue();
        var success = await p.LoginAsync( TestHelper.Monitor, "SimpleCode", Payload( p, code ) );
        success.IsSuccess.ShouldBeTrue();
        success.Actor!.ActorId.ShouldBe( userId );
        success.Actor.Level.ShouldBe( AuthLevel.Critical );
        success.Actor.HasScheme( "SimpleCode" ).ShouldBeTrue();

        var failed = await p.LoginAsync( TestHelper.Monitor, "SimpleCode", Payload( p, "NO!" + code ) );
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
        var code = "Bind-" + Guid.NewGuid().ToString( "N" );
        var now = DateTime.UtcNow;

        // Normal level is not enough by default.
        var normal = AuthenticationInfo.Create( user, now.AddHours( 1 ), null, null, now );
        var c1 = new LoginContext( "SimpleCode", Payload( p, code ), normal );
        (await binder.BindAccountAsync( TestHelper.Monitor, c1 )).ShouldBeNull();
        c1.ErrorId.ShouldBe( "User.AccountBinding.CriticalLevelRequired" );

        // The basic scheme cannot be bound.
        var critical = AuthenticationInfo.Create( user, now.AddHours( 1 ), now.AddMinutes( 5 ), null, now );
        var c2 = new LoginContext( "Basic", new BasicLoginPayload( "x", "y" ), critical );
        (await binder.BindAccountAsync( TestHelper.Monitor, c2 )).ShouldBeNull();
        c2.ErrorId.ShouldBe( "User.AccountBinding.NotSupported" );

        // Critical: the scheme is bound and the user is logged in.
        var c3 = new LoginContext( "SimpleCode", Payload( p, code ), critical );
        var bound = (await binder.BindAccountAsync( TestHelper.Monitor, c3 )).ShouldNotBeNull();
        c3.HasError.ShouldBeFalse();
        bound.IsSuccess.ShouldBeTrue();
        bound.Actor!.ActorId.ShouldBe( userId );
        bound.Actor.HasScheme( "SimpleCode" ).ShouldBeTrue();

        (await p.LoginAsync( TestHelper.Monitor, "SimpleCode", Payload( p, code ) )).Actor!.ActorId.ShouldBe( userId );
    }
}
