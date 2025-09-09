using Sendify.Shared;

namespace Sendify.Settings;

public class PasswordSettings
{
    public string Salt { get; set; }
    public static PasswordSettings Instance { get; } = new();

    private PasswordSettings()
    {
        ThrowHelper.ThrowIfNull(IHostApplicationBuilderHelper.DefaultHostApplicationBuilder!);
        
        var configuration = IHostApplicationBuilderHelper.DefaultHostApplicationBuilder.Configuration;
        
        Salt = configuration["Password:Salt"]!;
    }
}