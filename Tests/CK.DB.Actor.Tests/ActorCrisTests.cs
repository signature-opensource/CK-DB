using CK.Core;
using CK.Cris;
using CK.IO.Actor;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Actor.Tests;

[TestFixture]
public class ActorCrisTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    AsyncServiceScope _scope;
    PocoDirectory _pocoDir;
    CrisExecutionContext _exec;
    GroupTable _groupTable;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _scope = SharedEngine.AutomaticServices.CreateAsyncScope();
        var services = _scope.ServiceProvider;
        _exec = services.GetRequiredService<CrisExecutionContext>();
        _pocoDir = services.GetRequiredService<PocoDirectory>();
        _groupTable = services.GetRequiredService<GroupTable>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _scope.DisposeAsync();
    }

    #region UserCommandHandlers
    [Test]
    public async Task can_create_user_Async()
    {
        var userName = Guid.NewGuid().ToString();
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = userName;
        } ) );

        var res = executedCmd.WithResult<ICreateUserCommandResult>().Result;
        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue();
        res.UserIdResult.ShouldBeGreaterThan( 2 );
        res.UserName.ShouldBe( userName );
        res.UserMessages.ShouldBeEmpty();
    }

    [Test]
    public async Task cannot_create_user_with_existing_username_Async()
    {
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = "System";
        } ) );

        var res = executedCmd.Result.ShouldNotBeNull();
        res.ShouldBeAssignableTo<ICrisResultError>().Errors.ShouldNotBeEmpty();
    }

    [Test]
    public async Task can_set_username_Async()
    {
        var userId = await CreateUserAsync();

        var newName = Guid.NewGuid().ToString();
        var setNameCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ISetUserNameCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = userId;
            c.UserName = newName;
        } ) );
        var setResult = setNameCmd.WithResult<ISetUserNameCommandResult>().Result;
        setResult.ShouldNotBeNull();
        setResult.Success.ShouldBeTrue();
        setResult.UserName.ShouldBe( newName );
    }

    [Test]
    public async Task cannot_set_username_with_existing_one_Async()
    {
        var userId = await CreateUserAsync();

        var newName = Guid.NewGuid().ToString();
        var setNameCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ISetUserNameCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = userId;
            c.UserName = "System";
        } ) );
        var setResult = setNameCmd.WithResult<ISetUserNameCommandResult>().Result;
        setResult.ShouldNotBeNull();
        setResult.Success.ShouldBeFalse();
    }

    [Test]
    public async Task can_destroy_user_Async()
    {
        var userId = await CreateUserAsync();

        var execDestroyCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IDestroyUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = userId;
        } ) );
        var destroyRes = execDestroyCmd.WithResult<ICrisBasicCommandResult>().Result;
        destroyRes.ShouldNotBeNull();
        destroyRes.Success.ShouldBeTrue();
    }

    [Test]
    public async Task cannot_destroy_user_with_userId_lesser_than_2_Async()
    {
        var execDestroyCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IDestroyUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = 1;
        } ) );
        var destroyRes = execDestroyCmd.Result.ShouldNotBeNull();
        destroyRes.ShouldBeAssignableTo<ICrisResultError>().Errors.ShouldNotBeEmpty();
    }

    [Test]
    public async Task can_clear_user_groups_Async()
    {
        var userId = await CreateUserAsync();

        using var ctx = new SqlStandardCallContext();
        await _groupTable.AddUserAsync( ctx, actorId: 1, groupId: 2, userId );

        _groupTable.Database.ExecuteReader( "select * from CK.tActorProfile where ActorId = @0 and ActorId <> GroupId", userId )
            .ShouldNotBeNull()
            .Rows
            .ShouldNotBeEmpty();

        var execClearCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IClearUserGroupsCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = userId;
        } ) );
        var clearRes = execClearCmd.WithResult<ICrisBasicCommandResult>().Result;
        clearRes.ShouldNotBeNull();
        clearRes.Success.ShouldBeTrue();

        _groupTable.Database.ExecuteReader( "select * from CK.tActorProfile where ActorId = @0 and ActorId <> GroupId", userId )
            .ShouldNotBeNull()
            .Rows
            .ShouldBeEmpty();
    }

    [Test]
    public async Task can_get_user_profile_Async()
    {
        var execCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IGetUserProfileQCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = 1;
        } ) );
        var userProfile = execCmd.WithResult<IUserProfile?>().Result;
        userProfile.ShouldNotBeNull();
        userProfile.UserId.ShouldBe( 1 );
        userProfile.UserName.ShouldBe( "System" );
    }

    [Test]
    public async Task can_check_username_availability_Async()
    {
        var execCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICheckUserNameAvailabilityCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = 3712;
            c.UserName = "System";
        } ) );
        execCmd.WithResult<bool>().Result.ShouldBeFalse();

        execCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICheckUserNameAvailabilityCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = 1;
            c.UserName = "System";
        } ) );
        execCmd.WithResult<bool>().Result.ShouldBeTrue();

        execCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICheckUserNameAvailabilityCommand>( c =>
        {
            c.ActorId = 1;
            c.UserId = 1;
            c.UserName = Guid.NewGuid().ToString();
        } ) );
        execCmd.WithResult<bool>().Result.ShouldBeTrue();
    }
    #endregion

    #region GroupCommandHandlers
    [Test]
    public async Task can_create_group_Async()
    {
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICreateGroupCommand>( c =>
        {
            c.ActorId = 1;
        } ) );

        var res = executedCmd.WithResult<ICreateGroupCommandResult>().Result;
        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue();
        res.GroupIdResult.ShouldBeGreaterThan( 2 );
    }

    [Test]
    public async Task can_destroy_empty_group_Async()
    {
        var groupId = await CreateGroupAsync();

        var execDestroyCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IDestroyGroupCommand>( c =>
        {
            c.ActorId = 1;
            c.GroupId = groupId;
        } ) );
        var destroyRes = execDestroyCmd.WithResult<ICrisBasicCommandResult>().Result;
        destroyRes.ShouldNotBeNull();
        destroyRes.Success.ShouldBeTrue();
    }

    [Test]
    public async Task cannot_destroy_not_empty_group_when_not_forceDestroying_Async()
    {
        var groupId = await CreateGroupAsync();
        var userId = await CreateUserAsync();

        using var ctx = new SqlStandardCallContext();
        await _groupTable.AddUserAsync( ctx, actorId: 1, groupId, userId );

        var execDestroyCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IDestroyGroupCommand>( c =>
        {
            c.ActorId = 1;
            c.GroupId = groupId;
        } ) );
        var destroyRes = execDestroyCmd.Result.ShouldNotBeNull();
        destroyRes.ShouldBeAssignableTo<ICrisResultError>().Errors.ShouldNotBeEmpty();
    }

    [Test]
    public async Task can_remove_all_users_from_group_Async()
    {
        var groupId = await CreateGroupAsync();

        var execCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IRemoveAllUsersFromGroupCommand>( c =>
        {
            c.ActorId = 1;
            c.GroupId = groupId;
        } ) );
        var destroyRes = execCmd.WithResult<ICrisBasicCommandResult>().Result;
        destroyRes.ShouldNotBeNull();
        destroyRes.Success.ShouldBeTrue();
    }

    [Test]
    public async Task can_add_user_to_group_Async()
    {
        var groupId = await CreateGroupAsync();
        var userId = await CreateUserAsync();
        var execCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IAddUserToGroupCommand>( c =>
        {
            c.ActorId = 1;
            c.GroupId = groupId;
            c.UserId = userId;
        } ) );
        var execRes = execCmd.WithResult<ICrisBasicCommandResult>().Result;
        execRes.ShouldNotBeNull();
        execRes.Success.ShouldBeTrue();
    }

    [Test]
    public async Task can_remove_user_from_group_Async()
    {
        var groupId = await CreateGroupAsync();
        var userId = await CreateUserAsync();

        using var ctx = new SqlStandardCallContext();
        await _groupTable.AddUserAsync( ctx, actorId: 1, groupId, userId );

        var execCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IRemoveUserFromGroupCommand>( c =>
        {
            c.ActorId = 1;
            c.GroupId = groupId;
            c.UserId = userId;
        } ) );
        var execRes = execCmd.WithResult<ICrisBasicCommandResult>().Result;
        execRes.ShouldNotBeNull();
        execRes.Success.ShouldBeTrue();
    }
    #endregion

    async Task<int> CreateUserAsync()
    {
        var userName = Guid.NewGuid().ToString();
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICreateUserCommand>( c =>
        {
            c.ActorId = 1;
            c.UserName = userName;
        } ) );

        var createRes = executedCmd.WithResult<ICreateUserCommandResult>().Result;
        createRes.ShouldNotBeNull().UserIdResult.ShouldBeGreaterThan( 2 );

        return createRes.UserIdResult;
    }

    async Task<int> CreateGroupAsync()
    {
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICreateGroupCommand>( c =>
        {
            c.ActorId = 1;
        } ) );

        var res = executedCmd.WithResult<ICreateGroupCommandResult>().Result;
        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue();

        return res.GroupIdResult;
    }
}
