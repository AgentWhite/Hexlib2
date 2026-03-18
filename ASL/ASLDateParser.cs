using System;
using System.Text.RegularExpressions;

namespace ASL;

/// <summary>
/// Provides utility methods for parsing ASL-specific date strings.
/// </summary>
public static class ASLDateParser
{
    /// <summary>
    /// Attempts to parse a date string like "September 2nd, 1941" or "Oct 3, 1942".
    /// Removes ordinal suffixes (st, nd, rd, th) before parsing.
    /// </summary>
    public static DateTime? Parse(string dateText)
    {
        if (string.IsNullOrWhiteSpace(dateText)) return null;

        // Remove st, nd, rd, th from day numbers (e.g., 2nd -> 2)
        // Match a digit followed by st, nd, rd, or th
        string cleaned = Regex.Replace(dateText, @"(\d)(st|nd|rd|th)", "$1", RegexOptions.IgnoreCase);

        if (DateTime.TryParse(cleaned, out DateTime result))
        {
            return result;
        }

        return null;
    }
}
