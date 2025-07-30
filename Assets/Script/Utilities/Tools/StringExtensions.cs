using System.Text.RegularExpressions;

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string stringVal)
    {
        return stringVal == null || stringVal.Equals("");
    }
    public static string ToPascalCase(this string input)
    {
        return Regex.Replace(input.ToLowerInvariant(), @"\b[a-z]", m => m.Value.ToUpper());
    }
}