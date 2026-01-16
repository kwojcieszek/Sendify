using System.ComponentModel;
using System.Text;

namespace Sendify.Shared.Extensions;

public static class StringExtensions
{
    public static object Parse(this string s, Type type)
    {
        return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(s);
    }

    public static string NormalizeText(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;

        }

        var normalizedString = s.Normalize(NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string ReplacePolishDiacritics(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        return s
            .Replace('ą', 'a').Replace('Ą', 'A')
            .Replace('ć', 'c').Replace('Ć', 'C')
            .Replace('ę', 'e').Replace('Ę', 'E')
            .Replace('ł', 'l').Replace('Ł', 'L')
            .Replace('ń', 'n').Replace('Ń', 'N')
            .Replace('ó', 'o').Replace('Ó', 'O')
            .Replace('ś', 's').Replace('Ś', 'S')
            .Replace('ż', 'z').Replace('Ż', 'Z')
            .Replace('ź', 'z').Replace('Ź', 'Z');
    }

    public static string JsonEscape(this string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        return s
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
    }

    public static string RemoveLineEndings(this string s)
    {
        return s.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
    }

    public static string[] SplitByLength(this string s, int length)
    {
        if (string.IsNullOrEmpty(s) || length <= 0)
        {
            return Array.Empty<string>();
        }

        var result = new List<string>();
        for (int i = 0; i < s.Length; i += length)
        {
            if (i + length > s.Length)
            {
                result.Add(s.Substring(i));
            }
            else
            {
                result.Add(s.Substring(i, length));
            }
        }

        return result.ToArray();
    }
}