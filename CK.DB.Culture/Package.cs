using CK.Core;
using CK.SqlServer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CK.DB.Culture;

/// <summary>
/// Culture package.
/// Currently, no data is cached by this implementation: eventually <see cref="CultureData"/> and <see cref="ExtendedCultureData"/>
/// should be cached.
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
public abstract partial class Package : SqlPackage
{

}
