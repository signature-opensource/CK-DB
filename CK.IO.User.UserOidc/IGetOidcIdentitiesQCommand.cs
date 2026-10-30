using CK.Auth;
using CK.Cris;
using System.Collections.Generic;

namespace CK.IO.User.UserOidc;

/// <summary>
/// Lists the OpenID Connect identities of the current user.
/// </summary>
public interface IGetOidcIdentitiesQCommand : ICommand<List<IUserOidcIdentity>>, ICommandAuthNormal
{
}
