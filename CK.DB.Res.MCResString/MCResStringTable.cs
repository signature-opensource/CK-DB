using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Res.MCResString;

/// <summary>
/// This table holds nvarchar(400) value for a culture and a resource.
/// </summary>
[SqlTable( "tMCResString", Package = typeof( Package ) )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:sResDestroy, transform:sCultureDestroy" )]
[SqlObjectItem( "vMCResString" )]
public abstract partial class MCResStringTable : SqlTable
{
    /// <summary>
    /// Gets the resource table.
    /// </summary>
    [InjectObject]
    public ResTable ResTable { get; protected set; }

    /// <summary>
    /// Gets the Globalization Package.
    /// </summary>
    [InjectObject]
    public Globalization.Package Globalization { get; protected set; }

    /// <summary>
    /// Sets a resource string in a given culture.
    /// When <paramref name="value"/> is null, this removes the associated string.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="value">The new string value.</param>
    /// <returns>The awaitable.</returns>
    [SqlProcedure( "sMCResStringSet" )]
    public abstract Task SetStringAsync( ISqlCallContext ctx, int resId, int cultureId, string value );

}
