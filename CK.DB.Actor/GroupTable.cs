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
    /// Destroys a Group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">
    /// The group identifier to destroy. 
    /// When <paramref name="forceDestroy"/> is false (the default), it must be empty otherwise an exception is thrown.
    /// </param>
    /// <param name="forceDestroy">True to remove all members before destroying the group.</param>
    /// <returns>True when group was successfully destroyed, false otherwise.</returns>
    [SqlProcedure( "sGroupDestroy" )]
    public abstract Task DestroyGroupAsync( ISqlCallContext ctx, int actorId, int groupId, bool forceDestroy = false );

    /// <summary>
    /// Destroys a Group.
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
    /// Adds a User or another kind of Actor (that must not be a Group) into a Group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="memberId">The actor identifier to add (must not be a group).</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sGroupMemberAdd" )]
    public abstract Task AddMemberAsync( ISqlCallContext ctx, int actorId, int groupId, int memberId );

    /// <summary>
    /// Adds a member into a group.
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
    [SqlProcedure( "sGroupMemberAdd" )]
    public abstract Task<ICrisBasicCommandResult> AddMemberAsync( ISqlCallContext ctx, [ParameterSource] IAddUserToGroupCommand cmd );

    /// <summary>
    /// Removes a user or another kind of actor (except group) from a group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="memberId">The member identifier to remove.</param>
    /// <returns>True when member was removed, false otherwise.</returns>
    [SqlProcedure( "sGroupMemberRemove" )]
    public abstract Task RemoveMemberAsync( ISqlCallContext ctx, int actorId, int groupId, int memberId );

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
    [SqlProcedure( "sGroupMemberRemove" )]
    public abstract Task<ICrisBasicCommandResult> RemoveMemberAsync( ISqlCallContext ctx, [ParameterSource] IRemoveUserFromGroupCommand cmd );

    /// <summary>
    /// Removes all members from a group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">The group identifier to clear.</param>
    /// <returns>True when all members were removed, false otherwise.</returns>
    [SqlProcedure( "sGroupRemoveAllMembers" )]
    public abstract Task RemoveAllMembersAsync( ISqlCallContext ctx, int actorId, int groupId );

    /// <summary>
    /// Clears a Group: removes all its members.
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
    [SqlProcedure( "sGroupRemoveAllMembers" )]
    public abstract Task<ICrisBasicCommandResult> RemoveAllMembersAsync( ISqlCallContext ctx, [ParameterSource] IRemoveAllUsersFromGroupCommand cmd );
}
