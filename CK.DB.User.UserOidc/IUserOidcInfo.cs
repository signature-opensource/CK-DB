using CK.Core;
using System.Text.Json;

namespace CK.DB.User.UserOidc;

/// <summary>
/// Holds information stored for a Oidc user.
/// </summary>
public interface IUserOidcInfo : IPoco
{
    /// <summary>
    /// Gets or sets the suffix client scheme.
    /// Can be empty. Must not start with a '.' nor with "Oidc".
    /// </summary>
    string SchemeSuffix { get; set; }

    /// <summary>
    /// Gets or sets the Subject identifier.
    /// </summary>
    string Sub { get; set; }

    /// <summary>
    /// Gets or sets the claims of the identity, as received from the provider: a JSON object (the values as they are:
    /// strings, booleans, numbers, objects), without the protocol claims nor "sub". They are "external" data: the structured
    /// views of them (an email, a name...) belong to other packages (see <see cref="UserOidcInfoExtension.TryGetClaim"/>).
    /// <para>
    /// Empty (the default) means "not provided": a creation then stores "{}" (no claims known), an update or a login keeps
    /// the stored claims. A non empty value replaces them (at an actual login, not at a check).
    /// </para>
    /// <para>
    /// They may hold personal data (address, phone, birth date...) as soon as the requested scopes include them.
    /// </para>
    /// </summary>
    string Claims { get; set; }
}

/// <summary>
/// Extension method for IUserOidcInfo.
/// </summary>
public static class UserOidcInfoExtension
{
    /// <summary>
    /// Gets either "Oidc" or "Oidc.<see cref="IUserOidcInfo.SchemeSuffix"/>".
    /// </summary>
    /// <param name="this">This IUserOidcInfo.</param>
    /// <returns>The scheme.</returns>
    public static string GetScheme( this IUserOidcInfo @this )
    {
        return string.IsNullOrEmpty( @this.SchemeSuffix )
                ? "Oidc"
                : "Oidc." + @this.SchemeSuffix;
    }

    /// <summary>
    /// Gets a claim of the <see cref="IUserOidcInfo.Claims"/>, as received (a string, a boolean, a number, an object...).
    /// <para>
    /// Caution: a claim is what the provider says. Using an unverified value (an "email" without "email_verified", a
    /// "preferred_username") to look up or bind an account is the classic account takeover path.
    /// </para>
    /// </summary>
    /// <param name="this">This IUserOidcInfo.</param>
    /// <param name="name">The claim name ("email", "name"...).</param>
    /// <param name="value">The claim value (a clone: it doesn't depend on a document to dispose).</param>
    /// <returns>True if the claim exists, false otherwise (or when the claims are empty or invalid).</returns>
    public static bool TryGetClaim( this IUserOidcInfo @this, string name, out JsonElement value )
    {
        value = default;
        if( string.IsNullOrEmpty( @this.Claims ) ) return false;
        try
        {
            using var d = JsonDocument.Parse( @this.Claims );
            if( d.RootElement.ValueKind == JsonValueKind.Object && d.RootElement.TryGetProperty( name, out var v ) )
            {
                value = v.Clone();
                return true;
            }
        }
        catch( JsonException )
        {
        }
        return false;
    }
}
