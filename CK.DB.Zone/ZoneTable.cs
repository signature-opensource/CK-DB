using System.Threading.Tasks;
using CK.Core;
using CK.SqlServer;

namespace CK.DB.Zone;

/// <summary>
/// This table defines Zones that contains Groups.
/// </summary>
[SqlTable( "tZone", Package = typeof( Package ) )]
[Versions( "5.0.0,5.0.1" )]
[SqlObjectItem( "vZone" )]
public abstract partial class ZoneTable : SqlTable
{
    void StObjConstruct( Actor.GroupTable group )
    {
    }

    /// <summary>
    /// Creates a new zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <returns>A new zone identifier.</returns>
    [SqlProcedure( "CK.sZoneCreate" )]
    public abstract Task<int> CreateZoneAsync( ISqlCallContext ctx, int actorId );

    /// <summary>
    /// Destroys a Zone, optionally destroying its groups.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier to destroy.</param>
    /// <param name="forceDestroy">True to destroy the Zone even it is contains User or Groups (its Groups are destroyed).</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "CK.sZoneDestroy" )]
    public abstract Task DestroyZoneAsync( ISqlCallContext ctx, int actorId, int zoneId, bool forceDestroy = false );

    /// <summary>
    /// Registers a User or another kind of Actor (but not a Group) in a Zone: the member can then be added to groups of the zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier into which the member must be added.</param>
    /// <param name="memberId">The member identifier to add.</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "CK.sZoneMemberAdd" )]
    public abstract Task AddMemberAsync( ISqlCallContext ctx, int actorId, int zoneId, int memberId );

    /// <summary>
    /// Removes a member from a Zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="zoneId">The Zone identifier from which the member must be removed.</param>
    /// <param name="memberId">The member identifier to remove.</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "CK.sZoneMemberRemove" )]
    public abstract Task RemoveMemberAsync( ISqlCallContext ctx, int actorId, int zoneId, int memberId );
}
