using CK.Core;
using CK.SqlServer;

namespace CK.DB.HZone;

public abstract partial class ZoneTable
{
    /// <summary>
    /// Creates a new Zone under an another one (or 0 for a root zone).
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="parentZoneId">The parent zone identifier.</param>
    /// <returns>A new Zone identifier.</returns>
    [SqlProcedure( "transform:CK.sZoneCreate" )]
    public abstract int CreateZone( ISqlCallContext ctx, int actorId, int parentZoneId );

    /// <summary>
    /// Destroys a Zone, optionally destroying its groups and child zones.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier to destroy.</param>
    /// <param name="forceDestroy">True to destroy the Zone even it is contains User or Groups (its Groups are destroyed).</param>
    /// <param name="destroySubZone">
    /// True to destroy any existing child zones (recursively), false to raise an error if a child zone exists.
    /// Letting it to null uses the <paramref name="forceDestroy"/> parameter value.
    /// </param>
    [SqlProcedure( "transform:CK.sZoneDestroy" )]
    public abstract void DestroyZone( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false, bool? destroySubZone = null );

    /// <summary>
    /// Registers a User or another kind of Actor (but not a Group) in a Zone: the member can then be added to groups of the zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier into which the user must be added.</param>
    /// <param name="memberId">The member identifier to add.</param>
    /// <param name="autoAddMemberInParentZone">True to automatically add the member to its parent zones.</param>
    [SqlProcedure( "transform:CK.sZoneMemberAdd" )]
    public abstract void AddMember( ISqlCallContext ctx, int actorId, int zoneId, int memberId, bool autoAddMemberInParentZone = false );

    /// <summary>
    /// Removes a member from a Zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier from which the member must be removed.</param>
    /// <param name="memberId">The member identifier to remove.</param>
    /// <param name="autoRemoveMemberFromChildZone">
    /// True to automatically remove the member from any child zones.
    /// False will raise an error if the member is registered in child zones.
    /// </param>
    [SqlProcedure( "transform:sZoneMemberRemove" )]
    public abstract void RemoveMember( ISqlCallContext ctx, int actorId, int zoneId, int memberId, bool autoRemoveMemberFromChildZone = true );

    /// <summary>
    /// Moves a Zone to another parent zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier to move.</param>
    /// <param name="newParentZoneId">The target Zone identifier.</param>
    /// <param name="option">Options that control the move. See <see cref="Zone.GroupMoveOption"/>.</param>
    /// <param name="nextSiblingId">
    /// Optional identifier of a child zone in the target zone.
    /// When not specified, the zone is moved at the end of the existing child ot the target zone.
    /// </param>
    [SqlProcedure( "sZoneMove" )]
    public abstract void MoveZone( ISqlCallContext ctx, int actorId, int zoneId, int newParentZoneId, Zone.GroupMoveOption option = Zone.GroupMoveOption.None, int nextSiblingId = 0 );

}
