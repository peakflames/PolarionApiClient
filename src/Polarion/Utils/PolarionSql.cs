namespace Polarion;

/// <summary>
/// Helpers for embedding caller-supplied values in Polarion SQL queries.
/// </summary>
/// <remarks>
/// Polarion's SOAP SQL methods take a raw query string and offer no parameter binding,
/// so every interpolated value must go through one of these helpers.
/// </remarks>
internal static class PolarionSql
{
    /// <summary>
    /// Clause to append after every <c>LIKE</c> pattern produced by <see cref="EscapeLikePattern"/>.
    /// </summary>
    internal const string LikeEscapeClause = @"ESCAPE '\'";

    /// <summary>
    /// Escapes a value for use inside a single-quoted SQL string literal by doubling single quotes.
    /// </summary>
    /// <param name="value">The raw value</param>
    /// <returns>The value safe to place between single quotes</returns>
    internal static string EscapeLiteral(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Replace("'", "''");
    }

    /// <summary>
    /// Escapes a value for use inside a single-quoted SQL <c>LIKE</c> pattern so that
    /// <c>%</c>, <c>_</c> and <c>\</c> match literally and single quotes cannot end the literal.
    /// </summary>
    /// <param name="value">The raw value</param>
    /// <returns>The escaped pattern fragment; the clause must be followed by <see cref="LikeEscapeClause"/></returns>
    internal static string EscapeLikePattern(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var escaped = value
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");
        return EscapeLiteral(escaped);
    }
}
