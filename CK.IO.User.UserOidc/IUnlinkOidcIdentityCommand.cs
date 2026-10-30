using CK.Auth;
using CK.Cris;

namespace CK.IO.User.UserOidc;

/// <summary>
/// Unlinks an OpenID Connect identity of the current user: the user can no more log in with it.
/// <para>
/// Removing a login method requires the <see cref="ICommandAuthCritical">Critical</see> level (a recent login), and it
/// is refused while impersonating. The "last login method" rule applies: the identity cannot be unlinked when the user
/// has no other way to log in (no other row in <c>CK.vUserAuthProvider</c>).
/// </para>
/// </summary>
public interface IUnlinkOidcIdentityCommand : ICommand<ICrisBasicCommandResult>, ICommandCurrentCulture, ICommandAuthCritical, ICommandAuthImpersonation
{
    /// <summary>
    /// Gets or sets the suffix of the scheme of the identity ("Google" for "Oidc.Google"). Can be empty (the "Oidc" scheme).
    /// </summary>
    string SchemeSuffix { get; set; }
}
