using Sendify.Shared;
using System.Text;
using Sendify.ServiceCollection.Extensions;

namespace Sendify.JobsWorker;

internal static class ServicesSettings
{
    public static void SetServices(IServiceCollection services)
    {
        services.SetDatabaseSettings();

        services.SetPasswordSettings();

        PasswordSha256.Salt = Encoding.GetEncoding("UTF-8").GetBytes(Settings.PasswordSettings.Instance.Salt.ToCharArray());
    }
}