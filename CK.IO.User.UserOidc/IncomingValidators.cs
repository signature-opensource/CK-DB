using CK.Core;
using CK.Cris;

namespace CK.IO.User.UserOidc;

/// <summary>
/// Validates the OpenID Connect identity commands when they enter the System.
/// </summary>
public class IncomingValidators : IRealObject
{
    /// <summary>
    /// The maximal length of a scheme suffix (the <c>CK.tUserOidc</c> column).
    /// </summary>
    public const int MaxSchemeSuffixLength = 64;

    /// <summary>
    /// Checks the scheme suffix and refuses the command while impersonating: a user's login methods are removed by the
    /// user.
    /// </summary>
    /// <param name="collector">The message collector.</param>
    /// <param name="command">The command.</param>
    [IncomingValidator]
    public virtual void ValidateUnlinkOidcIdentityCommand( UserMessageCollector collector, IUnlinkOidcIdentityCommand command )
    {
        if( command.SchemeSuffix == null || command.SchemeSuffix.Length > MaxSchemeSuffixLength )
        {
            collector.Error( $"The scheme suffix must have at most {MaxSchemeSuffixLength} characters.", "Oidc.InvalidSchemeSuffix" );
        }
        if( command.ActorId != command.ActualActorId )
        {
            collector.Error( "This is refused while impersonating.", "Authentication.Impersonated" );
        }
    }
}
