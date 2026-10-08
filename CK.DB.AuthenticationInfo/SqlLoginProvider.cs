using CK.Core;
using CK.DB.Auth;
using CK.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.DB.AuthenticationInfo;

/// <summary>
/// Implements <see cref="ILoginProvider"/> on all the CK.DB.Auth providers: the basic provider (with a
/// <see cref="BasicLoginPayload"/>) and the generic ones (with their own payload).
/// <para>
/// This is a singleton: each call uses its own <see cref="SqlStandardCallContext"/>.
/// </para>
/// </summary>
public sealed class SqlLoginProvider : ILoginProvider
{
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
        _providerNames = db.AllProviders.Select( p => p.ProviderName ).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> ProviderNames => _providerNames;

    /// <inheritdoc />
    public object CreatePayload( string scheme ) => _db.FindRequiredProvider( scheme, mustHavePayload: true ).CreatePayload();

    /// <inheritdoc />
    public async ValueTask<UserLoginResult> LoginAsync( IActivityMonitor monitor, string scheme, object payload, bool actualLogin = true )
    {
        Throw.CheckNotNullArgument( payload );
        using var ctx = new SqlStandardCallContext( monitor );
        LoginResult r;
        if( string.Equals( LoginScheme.GetProviderName( scheme ), BasicLoginPayload.Scheme, StringComparison.OrdinalIgnoreCase ) )
        {
            if( payload is not BasicLoginPayload basicPayload )
            {
                Throw.ArgumentException( nameof( payload ), $"The '{BasicLoginPayload.Scheme}' scheme requires a BasicLoginPayload." );
                return null!;
            }
            var basic = _db.BasicProvider;
            if( basic == null )
            {
                Throw.ArgumentException( nameof( scheme ), "No basic authentication provider is available." );
                return null!;
            }
            r = await basic.LoginUserAsync( ctx, basicPayload.UserName, basicPayload.Password, actualLogin ).ConfigureAwait( false );
        }
        else
        {
            var p = _db.FindRequiredProvider( scheme, mustHavePayload: false );
            r = await p.LoginUserAsync( ctx, payload, actualLogin ).ConfigureAwait( false );
        }
        return await _db.ToUserLoginResultAsync( ctx, r ).ConfigureAwait( false );
    }
}
