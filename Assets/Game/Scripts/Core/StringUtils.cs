using System.Collections.Generic;
using System.Linq;


public static class StringUtils
{
    /// <summary>
    /// Returns the specified string as a readable string.
    /// </summary>
    /// <param name="camelCase">The original, camel case string.</param>
    /// <returns>The readable version of the original string.</returns>
    public static string DisplayCamelCaseString(string camelCase)
    {
        var chars = new List<char> { camelCase[0] };
        foreach (var c in camelCase.Skip(1))
        {
            if (char.IsUpper(c))
            {
                chars.Add(' ');
                chars.Add(char.ToLower(c));
            }
            else
            {
                chars.Add(c);
            }
        }

        return new string(chars.ToArray());
    }
}