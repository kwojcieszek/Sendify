using Sendify.Shared;

namespace Sendify.Settings;

public class PasswordSettings
{
    public string Salt { get; set; }
    public static PasswordSettings Instance { get; } = new();

    private PasswordSettings()
    {
        ThrowHelper.ThrowIfNull(IHostApplicationBuilderHelper.DefaultIHostApplicationBuilder!);
        
        var configuration = IHostApplicationBuilderHelper.DefaultIHostApplicationBuilder.Configuration;
        
        Salt = configuration["Password:Salt"]!;
    }
}