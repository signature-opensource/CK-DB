using System.Threading.Tasks;
using CK.Core;
using CK.Cris;
using CK.IO.Actor;
using CK.SqlServer;

namespace CK.DB.Actor;

/// <summary>
/// This table holds Groups of User.
/// </summary>
[SqlTable( "tGroup", Package = typeof( Package ) )]
[Versions( "5.0.0, 5.0.1, 5.0.2" )]
[SqlObjectItem( "vGroup" )]
public abstract partial class GroupTable : SqlTable
{
    void StObjConstruct( ActorTable actor )
    {
    }

    /// <summary>
    /// Creates a new Group.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <returns>A new group identifier.</returns>
    [SqlProcedure( "sGroupCreate" )]
    public abstract Task<int> CreateGroupAsync( ISqlCallContext ctx, int actorId );

    /// <summary>
    /// Creates a new Group.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="cmd">The incoming <see cref="ICreateGroupCommand"/> command.</param>
    /// <returns>
    /// A <see cref="ICreateGroupCommandResult"/>.
    /// <para>
    /// Note: The command result is a <see cref="ICrisResultError"/> when the stored procedure throws an exception.
    /// </para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "sGroupCreate" )]
    public abstract Task<ICreateGroupCommandResult> CreateGroupAsync( ISqlCallContext ctx, [ParameterSource] ICreateGroupCommand cmd );

    /// <summary>
    /// Destroys a Group if and only if there is no more users inside.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">
    /// The group identifier to destroy. 
    /// If <paramref name="forceDestroy"/> if false, it must be empty otherwise an exception is thrown.
    /// </param>
    /// <param name="forceDestroy">True to remove all users before destroying the group.</param>
    /// <returns>True when group was successfully destroyed, false otherwise.</returns>
    [SqlProcedure( "sGroupDestroy" )]
    public abstract Task DestroyGroupAsync( ISqlCallContext ctx, int actorId, int groupId, bool forceDestroy = false );

    /// <summary>
    /// Destroys a Group if and only if there is no more users inside.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="cmd">The incoming <see cref="IDestroyGroupCommand"/> command.</param>
    /// <returns>
    /// A <see cref="ICrisBasicCommandResult"/>.
    /// <para>
    /// Note: The command result is a <see cref="ICrisResultError"/> when the stored procedure throws an exception.
    /// </para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "sGroupDestroy" )]
    public abstract Task<ICrisBasicCommandResult> DestroyGroupAsync( ISqlCallContext ctx, [ParameterSource] IDestroyGroupCommand cmd );

    /// <summary>
    /// Adds a user into a group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="userId">The user identifier to add.</param>
    /// <returns>True when target user was successfully added, false otherwise.</returns>
    [SqlProcedure( "sGroupUserAdd" )]
    public abstract Task AddUserAsync( ISqlCallContext ctx, int actorId, int groupId, int userId );

    /// <summary>
    /// Adds a user into a group.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="cmd">The incoming <see cref="IAddUserToGroupCommand"/> command.</param>
    /// <returns>
    /// A <see cref="ICrisBasicCommandResult"/>.
    /// <para>
    /// Note: The command result is a <see cref="ICrisResultError"/> when the stored procedure throws an exception.
    /// </para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "sGroupUserAdd" )]
    public abstract Task<ICrisBasicCommandResult> AddUserAsync( ISqlCallContext ctx, [ParameterSource] IAddUserToGroupCommand cmd );

    /// <summary>
    /// Removes a user from a group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="userId">The user identifier to remove.</param>
    /// <returns>True when target user was removed, false otherwise.</returns>
    [SqlProcedure( "sGroupUserRemove" )]
    public abstract Task RemoveUserAsync( ISqlCallContext ctx, int actorId, int groupId, int userId );

    /// <summary>
    /// Removes a user from a group.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="cmd">The incoming <see cref="IRemoveUserFromGroupCommand"/> command.</param>
    /// <returns>
    /// A <see cref="ICrisBasicCommandResult"/>.
    /// <para>
    /// Note: The command result is a <see cref="ICrisResultError"/> when the stored procedure throws an exception.
    /// </para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "sGroupUserRemove" )]
    public abstract Task<ICrisBasicCommandResult> RemoveUserAsync( ISqlCallContext ctx, [ParameterSource] IRemoveUserFromGroupCommand cmd );

    /// <summary>
    /// Removes all users from a group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">The group identifier to clear.</param>
    /// <returns>True when all users were removed, false otherwise.</returns>
    [SqlProcedure( "sGroupRemoveAllUsers" )]
    public abstract Task RemoveAllUsersAsync( ISqlCallContext ctx, int actorId, int groupId );

    /// <summary>
    /// Removes all users from a group.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="cmd">The incoming <see cref="IRemoveAllUsersFromGroupCommand"/> command.</param>
    /// <returns>
    /// A <see cref="ICrisBasicCommandResult"/>.
    /// <para>
    /// Note: The command result is a <see cref="ICrisResultError"/> when the stored procedure throws an exception.
    /// </para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "sGroupRemoveAllUsers" )]
    public abstract Task<ICrisBasicCommandResult> RemoveAllUsersAsync( ISqlCallContext ctx, [ParameterSource] IRemoveAllUsersFromGroupCommand cmd );
}
