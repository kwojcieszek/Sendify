using Microsoft.Extensions.Configuration;
using Sendify.Shared;

namespace Sendify.Settings;

public class SmtpSettings
{
    public List<SmtpClient> SmtpClientList { get; set; }
    public static SmtpSettings Instance { get; } = new();

    private SmtpSettings()
    {
        ThrowHelper.ThrowIfNull(IHostApplicationBuilderHelper.DefaultHostApplicationBuilder!);

        var configuration = IHostApplicationBuilderHelper.DefaultHostApplicationBuilder.Configuration;

        SmtpClientList = configuration.GetSection("SmtpSettings").Get<List<SmtpClient>>()!;
    }
}