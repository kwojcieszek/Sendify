using Sendify.Api.Common;

namespace Sendify.Api.Extensions;

public static class JwtExtensions
{
    public static void SetJwtSettings(this IServiceCollection services)
    {
        var jWTSettings = Settings.JwtSettings.Instance;

        Authentication.JwtValidIssuer = jWTSettings.JwtValidIssuer;
        Authentication.JwtValidAudience = jWTSettings.JwtValidAudience;
        Authentication.JwtSecret = jWTSettings.JwtSecret;
    }
}