using CK.DB.Auth;
using CK.SqlServer;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace CK.DB.AuthenticationInfo;

/// <summary>
/// Conversions from the CK.DB.Auth model (that uses the legacy CK.Auth types) to the CK.AuthenticationInfo model.
/// </summary>
static class UserAuthInfoExtensions
{
    /// <summary>
    /// Converts a database user to a <see cref="CK.Core.UserInfo"/> at the highest level it can reach
    /// (<see cref="CK.Core.AuthLevel.Critical"/>: a weak actor is clamped to Unsafe by the "Weak" scheme rule).
    /// </summary>
    internal static CK.Core.UserInfo? ToUserInfo( this IUserAuthInfo? info )
    {
        if( info == null ) return null;
        var schemes = info.Schemes.Select( s => new CK.Core.UserSchemeInfo( s.Name, s.LastUsed ) ).ToImmutableArray();
        return new CK.Core.UserInfo( info.UserId, info.UserName, CK.Core.AuthLevel.Critical, schemes );
    }

    /// <summary>
    /// Converts a database login result. On success, the user is read again (with its schemes).
    /// </summary>
    internal static async ValueTask<CK.Core.UserLoginResult> ToUserLoginResultAsync( this IAuthenticationDatabaseService db,
                                                                                       ISqlCallContext ctx,
                                                                                       LoginResult r )
    {
        if( r.IsSuccess )
        {
            var user = (await db.ReadUserAuthInfoAsync( ctx, 1, r.UserId ).ConfigureAwait( false )).ToUserInfo();
            return user != null
                    ? CK.Core.UserLoginResult.Success( user )
                    : CK.Core.UserLoginResult.Failure( (int)KnownLoginFailureCode.Unspecified, $"User #{r.UserId} cannot be read after its login." );
        }
        int code = r.FailureCode > 0 ? r.FailureCode : (int)KnownLoginFailureCode.Unspecified;
        string reason = string.IsNullOrWhiteSpace( r.FailureReason ) ? KnownLoginFailureCodeExtensions.ToKnownString( code ) : r.FailureReason;
        return CK.Core.UserLoginResult.Failure( code, reason, code == (int)KnownLoginFailureCode.UnregisteredUser );
    }
}
