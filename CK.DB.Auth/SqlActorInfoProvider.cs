using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Auth;

/// <summary>
/// Implements the CK.AuthenticationInfo <see cref="IActorInfoProvider"/> on the CK.DB.Auth users.
/// The actor is at the highest level it can reach (the session clamps it) and an unknown actor is null.
/// <para>
/// This is a singleton: each call uses its own <see cref="SqlStandardCallContext"/>.
/// </para>
/// </summary>
public sealed class SqlActorInfoProvider : IActorInfoProvider
{
    readonly IAuthenticationDatabaseService _db;

    /// <summary>
    /// Initializes a new <see cref="SqlActorInfoProvider"/>.
    /// </summary>
    /// <param name="db">The authentication database service.</param>
    public SqlActorInfoProvider( IAuthenticationDatabaseService db )
    {
        Throw.CheckNotNullArgument( db );
        _db = db;
    }

    /// <inheritdoc />
    public async ValueTask<ActorInfo?> GetActorInfoAsync( IActivityMonitor monitor, int userId )
    {
        Throw.CheckArgument( "The anonymous cannot be read.", userId != 0 );
        using var ctx = new SqlStandardCallContext( monitor );
        return (await _db.ReadUserAuthInfoAsync( ctx, 1, userId ).ConfigureAwait( false )).ToActorInfo();
    }
}
