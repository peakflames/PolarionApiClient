namespace Polarion;

/// <summary>
/// Provides comparison logic for outline numbers in a document hierarchy.
/// Compares strings that represent outline numbers (e.g., "1.2.3" vs "1.2.10"),
/// treating numeric parts as numbers rather than strings.
/// </summary>
public class OutlineNumberComparer : IComparer<string>
{
    /// <summary>
    /// Compares two outline number strings.
    /// </summary>
    /// <param name="x">The first outline number to compare.</param>
    /// <param name="y">The second outline number to compare.</param>
    /// <returns>
    /// A negative value if x is less than y;
    /// zero if x equals y;
    /// a positive value if x is greater than y.
    /// </returns>
    /// <remarks>
    /// The comparison splits the strings by dots and compares each segment numerically if possible.
    /// For example, "1.2.3" is considered less than "1.2.10" because 3 is less than 10.
    /// </remarks>
    public int Compare(string? x, string? y)
    {
        if (x == null || y == null)
            return string.Compare(x, y, StringComparison.Ordinal);

        string[] xParts = x.Split('.');
        string[] yParts = y.Split('.');

        int minLength = Math.Min(xParts.Length, yParts.Length);

        for (int i = 0; i < minLength; i++)
        {
            if (int.TryParse(xParts[i], out int xNum) && int.TryParse(yParts[i], out int yNum))
            {
                if (xNum != yNum)
                    return xNum.CompareTo(yNum);
            }
            else
            {
                int stringCompare = string.Compare(xParts[i], yParts[i], StringComparison.Ordinal);
                if (stringCompare != 0)
                    return stringCompare;
            }
        }

        return xParts.Length.CompareTo(yParts.Length);
    }
}
