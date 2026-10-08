using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Auth.Tests;

/// <summary>
/// Tests the CK.AuthenticationInfo services of CK.DB.Auth that need no provider (the generic login and the account
/// binding are tested with the SimpleCode provider, the basic login with the UserPassword provider).
/// </summary>
[TestFixture]
public class AuthenticationInfoTests
{
    [Test]
    public void the_CK_AuthenticationInfo_services_are_registered()
    {
        var services = SharedEngine.AutomaticServices;
        services.GetRequiredService<IActorInfoProvider>().ShouldBeOfType<SqlActorInfoProvider>();
        services.GetServices<ILoginProvider>().ShouldContain( p => p is SqlLoginProvider );
        services.GetRequiredService<IAutoBindingAccountService>().ShouldBeOfType<SqlAutoBindingAccountService>();
    }

    [Test]
    public async Task SqlUserInfoProvider_reads_the_users_at_Critical_level_and_null_when_unknown_Async()
    {
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>().ShouldNotBeNull();
        int userId;
        var userName = "AuthInfo-" + Guid.NewGuid().ToString();
        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            userId = await user.CreateUserAsync( ctx, 1, userName );
        }
        var provider = SharedEngine.AutomaticServices.GetRequiredService<IActorInfoProvider>();
        var u = (await provider.GetActorInfoAsync( TestHelper.Monitor, userId )).ShouldNotBeNull();
        u.ActorId.ShouldBe( userId );
        u.ActorName.ShouldBe( userName );
        u.Level.ShouldBe( AuthLevel.Critical );
        u.Schemes.ShouldBeEmpty();

        (await provider.GetActorInfoAsync( TestHelper.Monitor, int.MaxValue )).ShouldBeNull();
        await Util.Awaitable( () => provider.GetActorInfoAsync( TestHelper.Monitor, 0 ).AsTask() ).ShouldThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task SqlLoginProvider_basic_requires_a_BasicLoginPayload_and_a_basic_provider_Async()
    {
        var p = SharedEngine.AutomaticServices.GetRequiredService<SqlLoginProvider>();
        // No basic provider here (it is CK.DB.User.UserPassword).
        await Util.Awaitable( () => p.LoginAsync( TestHelper.Monitor, "Basic", new BasicLoginPayload( "A", "B" ) ).AsTask() )
                  .ShouldThrowAsync<ArgumentException>();
        await Util.Awaitable( () => p.LoginAsync( TestHelper.Monitor, "Basic", "not a payload" ).AsTask() )
                  .ShouldThrowAsync<ArgumentException>();
    }
}
