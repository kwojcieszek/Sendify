using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sendify.Shared;

public static class ThrowHelper
{
    public static void ThrowIfNull(
        [NotNull]
        object argument,
        [CallerArgumentExpression(nameof(argument))] string paramName = null)
    {
        if (argument is null)
        {
            Throw(paramName);
        }
    }

    [DoesNotReturn]
    private static void Throw(string paramName) => throw new ArgumentNullException(paramName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static string IfNullOrWhitespace(
        [NotNull]
        string argument,
        [CallerArgumentExpression(nameof(argument))] string paramName = "")
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            if (argument == null)
            {
                throw new ArgumentNullException(paramName);
            }
            else
            {
                throw new ArgumentException(paramName, "Argument is whitespace");
            }
        }

        return argument;
    }
}