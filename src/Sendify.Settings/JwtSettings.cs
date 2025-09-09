using Sendify.Shared;

namespace Sendify.Settings;

public class JwtSettings
{
    public string JwtValidIssuer { get; set; }
    public string JwtValidAudience { get; set; }
    public string JwtSecret { get; set; }
    public static JwtSettings Instance { get; } = new();

    private JwtSettings()
    {
        ThrowHelper.ThrowIfNull(IHostApplicationBuilderHelper.DefaultHostApplicationBuilder!);
        
        var configuration = IHostApplicationBuilderHelper.DefaultHostApplicationBuilder.Configuration;
        
        JwtValidIssuer = configuration["JWT:ValidIssuer"]!;
        JwtValidAudience = configuration["JWT:ValidAudience"]!;
        JwtSecret = configuration["JWT:Secret"]!;
    }
}