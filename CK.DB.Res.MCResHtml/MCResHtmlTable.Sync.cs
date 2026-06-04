using CK.Core;
using CK.SqlServer;

namespace CK.DB.Res.MCResHtml;

public abstract partial class MCResHtmlTable : SqlTable
{
    /// <summary>
    /// Sets a resource string in a given culture.
    /// When <paramref name="value"/> is null, this removes the associated string.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="resId">The resource identifier.</param>
    /// <param name="cultureId">The culture identifier.</param>
    /// <param name="value">The new string value.</param>
    [SqlProcedure( "sMCResHtmlSet" )]
    public abstract void SetHtml( ISqlCallContext ctx, int resId, int cultureId, string value );


}
