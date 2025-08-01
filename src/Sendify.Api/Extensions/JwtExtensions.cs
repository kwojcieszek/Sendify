using Sendify.Api.Common;

namespace Sendify.Api.Extensions;

public static class JwtExtensions
{
    public static IApplicationBuilder UseJwtSettings(this IApplicationBuilder app)
    {
        var configuration = app.ApplicationServices.GetService<IConfiguration>();

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        Authentication.JwtValidIssuer = configuration["JWT:ValidIssuer"]!;
        Authentication.JwtValidAudience = configuration["JWT:ValidAudience"]!;
        Authentication.JwtSecret = configuration["JWT:Secret"]!;

        return app;
    }
}