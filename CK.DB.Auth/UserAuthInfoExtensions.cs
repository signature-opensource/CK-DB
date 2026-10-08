using CK.Core;
using CK.SqlServer;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace CK.DB.Auth;

/// <summary>
/// Conversions from the CK.DB.Auth results to the CK.AuthenticationInfo model.
/// </summary>
static class UserAuthInfoExtensions
{
    /// <summary>
    /// Converts a database user to a <see cref="UserInfo"/> at the highest level it can reach
    /// (<see cref="AuthLevel.Critical"/>: a weak actor is clamped to Unsafe by the "Weak" scheme rule).
    /// </summary>
    internal static UserInfo? ToUserInfo( this IUserAuthInfo? info )
    {
        if( info == null ) return null;
        var schemes = info.Schemes.ToImmutableArray();
        return new UserInfo( info.UserId, info.UserName, AuthLevel.Critical, schemes );
    }

    /// <summary>
    /// Converts a database login result. On success, the user is read again (with its schemes).
    /// </summary>
    internal static async ValueTask<UserLoginResult> ToUserLoginResultAsync( this IAuthenticationDatabaseService db,
                                                                                       ISqlCallContext ctx,
                                                                                       LoginResult r )
    {
        if( r.IsSuccess )
        {
            var user = (await db.ReadUserAuthInfoAsync( ctx, 1, r.UserId ).ConfigureAwait( false )).ToUserInfo();
            return user != null
                    ? UserLoginResult.Success( user )
                    : UserLoginResult.Failure( (int)KnownLoginFailureCode.Unspecified, $"User #{r.UserId} cannot be read after its login." );
        }
        int code = r.FailureCode > 0 ? r.FailureCode : (int)KnownLoginFailureCode.Unspecified;
        string reason = string.IsNullOrWhiteSpace( r.FailureReason ) ? KnownLoginFailureCodeExtensions.ToKnownString( code ) : r.FailureReason;
        return UserLoginResult.Failure( code, reason, code == (int)KnownLoginFailureCode.UnregisteredUser );
    }
}
