using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Sendify.Shared;

namespace Sendify.ServiceCollection.Extensions;

public static class PasswordExtensions
{
    public static void SetPasswordSettings(this IServiceCollection services)
    {
        PasswordSha256.Salt = Encoding.GetEncoding("UTF-8").GetBytes(Sendify.Settings.PasswordSettings.Instance.Salt.ToCharArray());
    }
}