using System.ComponentModel;

namespace Sendify.Shared.Extensions;

public static class StringExtensions
{
    public static object Parse(this string s,Type type)
    {
        return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(s);
    }
}