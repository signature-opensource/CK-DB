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
    /// Converts a database actor to a <see cref="ActorInfo"/> at the highest level it can reach
    /// (<see cref="AuthLevel.Critical"/>: a weak actor is clamped to Unsafe by the "Weak" scheme rule).
    /// </summary>
    internal static ActorInfo? ToActorInfo( this IUserAuthInfo? info )
    {
        if( info == null ) return null;
        var schemes = info.Schemes.ToImmutableArray();
        return new ActorInfo( info.UserId, info.UserName, AuthLevel.Critical, schemes );
    }

    /// <summary>
    /// Converts a database login result. On success, the actor is read again (with its schemes).
    /// </summary>
    internal static async ValueTask<ActorLoginResult> ToUserLoginResultAsync( this IAuthenticationDatabaseService db,
                                                                                       ISqlCallContext ctx,
                                                                                       LoginResult r )
    {
        if( r.IsSuccess )
        {
            var actor = (await db.ReadUserAuthInfoAsync( ctx, 1, r.UserId ).ConfigureAwait( false )).ToActorInfo();
            return actor != null
                    ? ActorLoginResult.Success( actor )
                    : ActorLoginResult.Failure( (int)KnownLoginFailureCode.Unspecified, $"Actor #{r.UserId} cannot be read after its login." );
        }
        int code = r.FailureCode > 0 ? r.FailureCode : (int)KnownLoginFailureCode.Unspecified;
        string reason = string.IsNullOrWhiteSpace( r.FailureReason ) ? KnownLoginFailureCodeExtensions.ToKnownString( code ) : r.FailureReason;
        return ActorLoginResult.Failure( code, reason, code == (int)KnownLoginFailureCode.UnregisteredUser );
    }
}
