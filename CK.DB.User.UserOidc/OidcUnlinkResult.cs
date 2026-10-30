namespace CK.DB.User.UserOidc;

/// <summary>
/// The result of <see cref="UserOidcTable.UnlinkIdentityAsync"/>.
/// </summary>
public enum OidcUnlinkResult
{
    /// <summary>
    /// The user has no identity for this scheme.
    /// </summary>
    NotFound = 0,

    /// <summary>
    /// The identity has been unlinked.
    /// </summary>
    Unlinked = 1,

    /// <summary>
    /// Refused: this identity is the user's last way to log in.
    /// </summary>
    LastLoginMethod = 2
}
