using CK.Core;

namespace CK.DB.Res.MCResHtml;

/// <summary>
/// Package that brings in html value (type is nvarchar(max)) for resources and cultures.
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
public class Package : SqlPackage
{
    ResTable _resTable;
    Globalization.Package _globalization;

    void StObjConstruct( ResTable resTable, Globalization.Package globalization )
    {
        _resTable = resTable;
        _globalization = globalization;
    }

    /// <summary>
    /// Gets the resource table (tRes).
    /// </summary>
    public ResTable ResTable => _resTable;

    /// <summary>
    /// Gets the Globalization package.
    /// </summary>
    public Globalization.Package Globalization => _globalization;

    /// <summary>
    /// Gets the tMCResHtml table from this package.
    /// </summary>
    [InjectObject]
    public MCResHtmlTable MCResHtmlTable { get; protected set; }
}
