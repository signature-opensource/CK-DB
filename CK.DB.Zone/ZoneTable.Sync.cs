using CK.Core;
using CK.SqlServer;

namespace CK.DB.Zone;

public abstract partial class ZoneTable
{

    /// <summary>
    /// Creates a new zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <returns>A new zone identifier.</returns>
    [SqlProcedure( "CK.sZoneCreate" )]
    public abstract int CreateZone( ISqlCallContext ctx, int actorId );

    /// <summary>
    /// Destroys a Zone, optionally destroying its groups.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier to destroy.</param>
    /// <param name="forceDestroy">True to destroy the Zone even it is contains User or Groups (its Groups are destroyed).</param>
    [SqlProcedure( "CK.sZoneDestroy" )]
    public abstract void DestroyZone( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false );

    /// <summary>
    /// Registers a member or another kind of Actor (but not a Group) in a Zone: the member can then be added to groups of the zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier into which the member must be added.</param>
    /// <param name="memberId">The member identifier to add.</param>
    [SqlProcedure( "sZoneMemberAdd" )]
    public abstract void AddMember( ISqlCallContext ctx, int actorId, int zoneId, int memberId );

    /// <summary>
    /// Removes a member from a Zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier from which the user must be removed.</param>
    /// <param name="memberId">The member identifier to remove.</param>
    [SqlProcedure( "sZoneMemberRemove" )]
    public abstract void RemoveUser( ISqlCallContext ctx, int actorId, int zoneId, int memberId );

}
