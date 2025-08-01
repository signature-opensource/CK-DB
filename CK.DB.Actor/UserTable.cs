using CK.Core;
using CK.Cris;
using CK.IO.Actor;
using CK.SqlServer;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace CK.DB.Actor;

/// <summary>
/// The UserTable kernel.
/// </summary>
[SqlTable( "CK.tUser", Package = typeof( Package ) )]
[Versions( "5.0.0, 5.0.1, 5.0.2, 5.0.3" )]
[SqlObjectItem( "CK.vUser" )]
public abstract partial class UserTable : SqlTable
{
    void StObjConstruct( ActorTable actor )
    {
    }

    /// <summary>
    /// Reads a user profile for a given user. Throws if user does not exist.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The targeted user.</param>
    /// <returns>A <see cref="IUserProfile"/> Poco.</returns>
    [SqlProcedure( "CK.sUserUserProfileRead" )]
    public abstract Task<IUserProfile> ReadUserProfileAsync( ISqlCallContext ctx, int actorId, int userId );

    /// <summary>
    /// Reads a user profile for a given user.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="IGetUserProfileQCommand"/> command.</param>
    /// <returns>The targeted user profile is exists, throws otherwise.</returns>
    [CommandHandler]
    [SqlProcedure( "CK.sUserUserProfileRead" )]
    public abstract Task<IUserProfile> ReadUserProfileAsync( ISqlCallContext ctx, [ParameterSource] IGetUserProfileQCommand command );

    /// <summary>
    /// Gets a typed <see cref="IUserProfile"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The targeted user identifier.</param>
    /// <returns>A typed <see cref="IUserProfile"/>.</returns>
    public async Task<T> GetUserProfileAsync<T>( ISqlCallContext ctx, int actorId, int userId ) where T : class, IUserProfile
        => (T)await ReadUserProfileAsync( ctx, actorId, userId );

    /// <summary>
    /// Tries to create a new user. Throws if the user name is not unique.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userName">The user name (must be unique).</param>
    /// <returns>A new user identifier.</returns>
    [SqlProcedure( "CK.sUserCreate" )]
    public abstract Task<int> CreateUserAsync( ISqlCallContext ctx, int actorId, string userName );

    /// <summary>
    /// Tries to create a new user. If the user name is not unique, returns -1.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="ICreateUserCommand"/> command.</param>
    /// <returns>
    /// A <see cref="ICreateUserCommandResult"/>.
    /// <para>
    /// Note: The command result is a <see cref="ICrisResultError"/> when the stored procedure throws an exception.
    /// </para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "CK.sUserCreate" )]
    public abstract Task<ICreateUserCommandResult> CreateUserAsync( ISqlCallContext ctx, [ParameterSource] ICreateUserCommand command );

    /// <summary>
    /// Tries to set a new user name.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier to update.</param>
    /// <param name="userName">The user name (must be unique otherwise false is returned).</param>
    /// <returns>True on success, false if the new name already exists.</returns>
    [SqlProcedure( "CK.sUserUserNameSet" )]
    public abstract Task<bool> UserNameSetAsync( ISqlCallContext ctx, int actorId, int userId, string userName );

    /// <summary>
    /// Tries to set a new user name.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="ISetUserNameCommand"/> command.</param>
    /// <returns>
    /// A <see cref="ISetUserNameCommandResult"/>.
    /// <para>
    /// Note: The command result is a <see cref="ICrisResultError"/> when the stored procedure throws an exception.
    /// </para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "CK.sUserUserNameSet" )]
    public abstract Task<ISetUserNameCommandResult> UserNameSetAsync( ISqlCallContext ctx, [ParameterSource] ISetUserNameCommand command );

    [CommandHandler]
    public async Task<bool> CheckUserNameAvailabilityAsync( ISqlCallContext ctx, ICheckUserNameAvailabilityCommand command )
    {
        using( var cmd = new SqlCommand( "select 1 from CK.tUser where UserName = @UserName and UserId <> @UserId;" ) )
        {
            cmd.Parameters.AddWithValue( "@UserId", command.UserId );
            cmd.Parameters.AddWithValue( "@UserName", command.UserName );
            return (await ctx[Database].ExecuteScalarAsync( cmd ).ConfigureAwait( false )) is null;
        }
    }

    /// <summary>
    /// Destroys a user.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier to destroy.</param>
    /// <returns>True if user was successfully destroyed, false otherwise.</returns>
    [SqlProcedure( "CK.sUserDestroy" )]
    public abstract Task DestroyUserAsync( ISqlCallContext ctx, int actorId, int userId );

    /// <summary>
    /// Destroys a user.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="IDestroyUserCommand"/> command.</param>
    /// <returns>
    /// A <see cref="ICrisBasicCommandResult"/>.
    /// <para>
    /// Note: The command result is a <see cref="ICrisResultError"/> when the stored procedure throws an exception.
    /// </para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "CK.sUserDestroy" )]
    public abstract Task<ICrisBasicCommandResult> DestroyUserAsync( ISqlCallContext ctx, [ParameterSource] IDestroyUserCommand command );

    /// <summary>
    /// Removes a user from all the Groups it belongs to.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="userId">The user identifier that must be removed from all its groups.</param>
    /// <returns>True if user was successfully removed from all groups, false otherwise.</returns>
    [SqlProcedure( "CK.sUserRemoveFromAllGroups" )]
    public abstract Task RemoveFromAllGroupsAsync( ISqlCallContext ctx, int actorId, int userId );

    /// <summary>
    /// Removes a user from all the Groups it belongs to.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="IClearUserGroupsCommand"/> command.</param>
    /// <returns>
    /// A <see cref="ICrisBasicCommandResult"/>.
    /// <para>
    /// Note: The command result is a <see cref="ICrisResultError"/> when the stored procedure throws an exception.
    /// </para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "CK.sUserRemoveFromAllGroups" )]
    public abstract Task<ICrisBasicCommandResult> RemoveFromAllGroupsAsync( ISqlCallContext ctx, [ParameterSource] IClearUserGroupsCommand command );

    /// <summary>
    /// Finds the user identifier given its user name.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="userName">The user name to lookup.</param>
    /// <returns>The user identifier or 0 if not found.</returns>
    public async Task<int> FindByNameAsync( ISqlCallContext ctx, string userName )
    {
        using( var cmd = new SqlCommand( "select UserId from CK.tUser where UserName=@Key" ) )
        {
            cmd.Parameters.AddWithValue( "@Key", userName );
            return (await ctx[Database].ExecuteScalarAsync( cmd ).ConfigureAwait( false )) is int id ? id : 0;
        }
    }
}
