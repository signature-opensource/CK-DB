using CK.Core;
using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.DB.Auth;

/// <summary>
/// Implements <see cref="ILoginProvider"/> on the generic CK.DB.Auth providers (with their own payload).
/// <para>
/// The "Basic" scheme is not handled here: it is the <see cref="ILoginProvider"/> of the package that implements the
/// basic provider (CK.DB.User.UserPassword), whose payload is the login command ("Package First": nothing about the
/// Basic scheme in the generic packages).
/// </para>
/// <para>
/// This is a singleton: each call uses its own <see cref="SqlStandardCallContext"/>.
/// </para>
/// </summary>
public sealed class SqlLoginProvider : ILoginProvider
{
    /// <summary>
    /// The name of the basic provider, that this login provider doesn't handle: "Basic".
    /// </summary>
    public const string BasicProviderName = "Basic";

    readonly IAuthenticationDatabaseService _db;
    readonly IReadOnlyCollection<string> _providerNames;

    /// <summary>
    /// Initializes a new <see cref="SqlLoginProvider"/>.
    /// </summary>
    /// <param name="db">The authentication database service.</param>
    public SqlLoginProvider( IAuthenticationDatabaseService db )
    {
        Throw.CheckNotNullArgument( db );
        _db = db;
        _providerNames = db.AllProviders.Select( p => p.ProviderName )
                                        .Where( n => !IsBasic( n ) )
                                        .ToArray();
    }

    static bool IsBasic( string schemeOrProviderName ) => string.Equals( LoginScheme.GetProviderName( schemeOrProviderName ), BasicProviderName, StringComparison.OrdinalIgnoreCase );

    /// <inheritdoc />
    public IReadOnlyCollection<string> ProviderNames => _providerNames;

    /// <inheritdoc />
    public object CreatePayload( string scheme ) => _db.FindRequiredProvider( scheme, mustHavePayload: true ).CreatePayload();

    /// <inheritdoc />
    public async ValueTask<ActorLoginResult> LoginAsync( IActivityMonitor monitor, string scheme, object payload, bool actualLogin = true )
    {
        Throw.CheckNotNullArgument( payload );
        if( IsBasic( scheme ) )
        {
            Throw.ArgumentException( nameof( scheme ), $"The '{BasicProviderName}' scheme is not handled by the {nameof( SqlLoginProvider )}: it is handled by the login provider of the basic provider package (CK.DB.User.UserPassword)." );
        }
        using var ctx = new SqlStandardCallContext( monitor );
        var p = _db.FindRequiredProvider( scheme, mustHavePayload: false );
        var r = await p.LoginUserAsync( ctx, payload, actualLogin ).ConfigureAwait( false );
        return await _db.ToUserLoginResultAsync( ctx, r ).ConfigureAwait( false );
    }
}
