using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.Res.MCResString;

/// <summary>
/// Package that brings in string value (type is nvarchar(400)) for resources and cultures.
/// </summary>
[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
public class Package : SqlPackage
{
    [AllowNull] ResTable _resTable;
    [AllowNull] Globalization.Package _globalization;

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
    /// Gets the tMCResString table from this package.
    /// </summary>
    [InjectObject]
    [AllowNull]
    public MCResStringTable MCResStringTable { get; protected set; }
}
