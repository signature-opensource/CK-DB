using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Res.MCResHtml;

/// <summary>
/// This table holds nvarchar(max) value that must contain html text for a culture and a resource.
/// </summary>
[SqlTable( "tMCResHtml", Package = typeof( Package ) )]
[Versions( "1.0.0" )]
[SqlObjectItem( "transform:sResDestroy, transform:sCultureDestroy" )]
[SqlObjectItem( "vMCResHtml" )]
public abstract partial class MCResHtmlTable : SqlTable
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
    [SqlProcedure( "sMCResHtmlSet" )]
    public abstract Task SetHtmlAsync( ISqlCallContext ctx, int resId, int cultureId, string value );

}
