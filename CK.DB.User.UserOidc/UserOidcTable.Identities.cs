using CK.Core;
using CK.IO.User.UserOidc;
using CK.SqlServer;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CK.DB.User.UserOidc;

public abstract partial class UserOidcTable
{
    /// <summary>
    /// Reads the OpenID Connect identities of a user, as listed by <see cref="IGetOidcIdentitiesQCommand"/>.
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="pocoDirectory">The poco directory.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The identities of the user (possibly empty).</returns>
    public async Task<List<IUserOidcIdentity>> ReadUserIdentitiesAsync( ISqlCallContext ctx, PocoDirectory pocoDirectory, int userId, CancellationToken cancellationToken = default )
    {
        using var cmd = new SqlCommand( "select SchemeSuffix, Claims, LastLoginTime from CK.tUserOidc where UserId = @UserId order by SchemeSuffix" );
        cmd.Parameters.AddWithValue( "@UserId", userId );
        return await ctx[Database].ExecuteQueryAsync( cmd, async ( c, t ) =>
        {
            var result = new List<IUserOidcIdentity>();
            using( var r = await c.ExecuteReaderAsync( t ).ConfigureAwait( false ) )
            {
                while( await r.ReadAsync( t ).ConfigureAwait( false ) )
                {
                    result.Add( pocoDirectory.Create<IUserOidcIdentity>( i =>
                    {
                        i.SchemeSuffix = r.GetString( 0 );
                        i.Claims = r.GetString( 1 );
                        i.LastLoginTime = DateTime.SpecifyKind( r.GetDateTime( 2 ), DateTimeKind.Utc );
                    } ) );
                }
            }
            return result;
        }, cancellationToken ).ConfigureAwait( false );
    }

    /// <summary>
    /// The actor unlinks one of its own identities. The "last login method" rule applies (see
    /// <see cref="OidcUnlinkResult.LastLoginMethod"/>).
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="actorId">The acting actor identifier: the user that owns the identity.</param>
    /// <param name="schemeSuffix">The suffix of the scheme of the identity.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The result.</returns>
    public async Task<OidcUnlinkResult> UnlinkIdentityAsync( ISqlCallContext ctx, int actorId, string schemeSuffix, CancellationToken cancellationToken = default )
    {
        return (OidcUnlinkResult)await DoUnlinkIdentityAsync( ctx, actorId, schemeSuffix, cancellationToken ).ConfigureAwait( false );
    }

    [SqlProcedure( "sUserOidcUnlink" )]
    protected abstract Task<int> DoUnlinkIdentityAsync( ISqlCallContext ctx, int actorId, string schemeSuffix, CancellationToken cancellationToken );
}
