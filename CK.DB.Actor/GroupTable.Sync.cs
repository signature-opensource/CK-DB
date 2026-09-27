using CK.Core;
using CK.SqlServer;

namespace CK.DB.Actor;

public abstract partial class GroupTable
{
    /// <summary>
    /// Creates a new Group.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <returns>A new group identifier.</returns>
    [SqlProcedure( "sGroupCreate" )]
    public abstract int CreateGroup( ISqlCallContext ctx, int actorId );

    /// <summary>
    /// Destroys a Group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">
    /// The group identifier to destroy. 
    /// If <paramref name="forceDestroy"/> if false, it must be empty otherwise an exception is thrown.
    /// </param>
    /// <param name="forceDestroy">True to remove all members before destroying the group.</param>
    [SqlProcedure( "sGroupDestroy" )]
    public abstract void DestroyGroup( ISqlCallContext ctx, int actorId, int groupId, bool forceDestroy = false );

    /// <summary>
    /// Adds a User or another kind of Actor (that must not be a Group) into a Group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="memberId">The member identifier to add.</param>
    [SqlProcedure( "sGroupMemberAdd" )]
    public abstract void AddMember( ISqlCallContext ctx, int actorId, int groupId, int memberId );

    /// <summary>
    /// Removes a member from a group.
    /// Idempotent.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="memberId">The member identifier to remove.</param>
    [SqlProcedure( "sGroupMemberRemove" )]
    public abstract void RemoveMember( ISqlCallContext ctx, int actorId, int groupId, int memberId );

    /// <summary>
    /// Clears a Group: removes all its members.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="groupId">The group identifier to clear.</param>
    [SqlProcedure( "sGroupRemoveAllMembers" )]
    public abstract void RemoveAllMembers( ISqlCallContext ctx, int actorId, int groupId );

}
