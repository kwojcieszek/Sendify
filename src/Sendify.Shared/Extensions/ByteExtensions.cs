namespace Sendify.Shared.Extensions;

public static class ByteExtensions
{
    public static string ToStringFromArrary(this byte[] array)
    {
        return System.Text.Encoding.UTF8.GetString(array);
    }
}