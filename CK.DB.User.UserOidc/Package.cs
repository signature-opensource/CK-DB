using CK.Core;

namespace CK.DB.User.UserOidc;

/// <summary>
/// Package that adds Oidc authentication support for users. 
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:vUserAuthProvider" )]
public partial class Package : SqlPackage
{
    void StObjConstruct( Actor.Package actorPackage, Auth.Package authPackage )
    {
    }
}
