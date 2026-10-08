using CK.Core;
using CK.SqlServer;
using System;
using System.Threading.Tasks;

namespace CK.DB.Auth;

/// <summary>
/// Implements <see cref="IAutoBindingAccountService"/> on the CK.DB.Auth generic providers: the scheme of a login
/// for which the user is not registered is bound to the currently logged in user (and the user is logged in).
/// <para>
/// By default the current user must be at <see cref="AuthLevel.Critical"/> level (see <see cref="RequiresCriticalLevel"/>).
/// Binding is refused while impersonating and for the basic scheme (a password is not bound by a login).
/// </para>
/// </summary>
public sealed class SqlAutoBindingAccountService : IAutoBindingAccountService
{
    readonly IAuthenticationDatabaseService _db;

    /// <summary>
    /// Initializes a new <see cref="SqlAutoBindingAccountService"/>.
    /// </summary>
    /// <param name="db">The authentication database service.</param>
    public SqlAutoBindingAccountService( IAuthenticationDatabaseService db )
    {
        Throw.CheckNotNullArgument( db );
        _db = db;
    }

    /// <summary>
    /// Gets or sets whether the current user must be at <see cref="AuthLevel.Critical"/> level. Defaults to true.
    /// When false, the <see cref="AuthLevel.Normal"/> level is enough.
    /// </summary>
    public bool RequiresCriticalLevel { get; set; } = true;

    /// <inheritdoc />
    public async ValueTask<ActorLoginResult?> BindAccountAsync( IActivityMonitor monitor, LoginContext context )
    {
        Throw.CheckNotNullArgument( monitor );
        Throw.CheckNotNullArgument( context );
        var auth = context.InitialAuthentication;
        if( auth.IsImpersonated )
        {
            return context.SetError( "User.AccountBinding.Impersonated", "Account binding is not possible while impersonating." );
        }
        var level = auth.UnsafeActor.Level;
        if( RequiresCriticalLevel && level != AuthLevel.Critical )
        {
            return context.SetError( "User.AccountBinding.CriticalLevelRequired", "User must be logged in Critical level." );
        }
        if( level < AuthLevel.Normal )
        {
            return context.SetError( "User.AccountBinding.AtLeastNormalLevelRequired", "User must be logged at least in Normal level." );
        }
        if( string.Equals( LoginScheme.GetProviderName( context.Scheme ), BasicLoginPayload.Scheme, StringComparison.OrdinalIgnoreCase ) )
        {
            return context.SetError( "User.AccountBinding.NotSupported", "The basic scheme cannot be bound by a login." );
        }
        var p = _db.FindRequiredProvider( context.Scheme, mustHavePayload: false );
        using var ctx = new SqlStandardCallContext( monitor );
        // This is an actual login (UCLMode.WithActualLogin): the scheme is bound and the user is logged in with it.
        UCLResult result = await p.CreateOrUpdateUserAsync( ctx, 1, auth.UnsafeActor.ActorId, context.Payload, UCLMode.CreateOrUpdate | UCLMode.WithActualLogin )
                                  .ConfigureAwait( false );
        return await _db.ToUserLoginResultAsync( ctx, result.LoginResult ).ConfigureAwait( false );
    }
}
