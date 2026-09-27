using CK.Core;
using System.Diagnostics.CodeAnalysis;

namespace CK.DB.Res.MCResText;

/// <summary>
/// This package brings culture support through CK.DB.Globalization.
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
    /// Gets the tMCResText table from this package.
    /// </summary>
    [InjectObject]
    [AllowNull]
    public MCResTextTable MCResTextTable { get; protected set; }
}
