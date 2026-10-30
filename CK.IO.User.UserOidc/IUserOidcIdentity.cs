using CK.Core;
using System;

namespace CK.IO.User.UserOidc;

/// <summary>
/// An OpenID Connect identity of a user, as listed by <see cref="IGetOidcIdentitiesQCommand"/>. The subject identifier
/// is not exposed (an opaque identifier at the provider): an identity is designated by its <see cref="SchemeSuffix"/>
/// (a user has at most one identity per scheme).
/// </summary>
public interface IUserOidcIdentity : IPoco
{
    /// <summary>
    /// Gets or sets the suffix of the scheme ("Google" for "Oidc.Google"). Can be empty (the "Oidc" scheme).
    /// </summary>
    string SchemeSuffix { get; set; }

    /// <summary>
    /// Gets or sets the claims shared by the provider at the last actual login: a JSON object, "{}" when none is
    /// known. They may hold personal data: they are the user's own data, listed for the user only.
    /// </summary>
    string Claims { get; set; }

    /// <summary>
    /// Gets or sets the last time the identity was used to log in.
    /// </summary>
    DateTime LastLoginTime { get; set; }
}
