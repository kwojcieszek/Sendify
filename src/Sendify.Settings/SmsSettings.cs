using Sendify.Shared;

namespace Sendify.Settings;

public class SmsSettings
{
    public string Mode { get; set; }
    public static SmsSettings Instance { get; } = new();

    private SmsSettings()
    {
        ThrowHelper.ThrowIfNull(IHostApplicationBuilderHelper.DefaultHostApplicationBuilder!);
        
        var configuration = IHostApplicationBuilderHelper.DefaultHostApplicationBuilder.Configuration;

        Mode = configuration["SmsSettings:Mode"] ?? "Text";
    }
}
