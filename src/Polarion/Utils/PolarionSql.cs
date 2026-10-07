namespace Polarion;

/// <summary>
/// Helpers for embedding caller-supplied values in Polarion SQL queries.
/// </summary>
/// <remarks>
/// Polarion's SOAP SQL methods take a raw query string and offer no parameter binding,
/// so every interpolated value must go through one of these helpers.
/// <para>
/// Assumes the database runs with <c>standard_conforming_strings=on</c> (the PostgreSQL default since
/// 9.1): a backslash in an ordinary string literal is then a plain character, so doubling single quotes
/// is enough to keep a value inside its literal, and <c>\</c> only acts as the escape character where
/// <see cref="LikeEscapeClause"/> declares it. With the setting off, backslashes in a value would be
/// read as escapes and this escaping would not be sufficient.
/// </para>
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
