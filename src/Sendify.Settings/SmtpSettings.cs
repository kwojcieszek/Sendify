using Microsoft.Extensions.Configuration;
using Sendify.Shared;

namespace Sendify.Settings;

public class SmtpSettings
{
    public List<SmtpClient> SmtpClientList { get; set; }
    public static SmtpSettings Instance { get; } = new();

    private SmtpSettings()
    {
        ThrowHelper.ThrowIfNull(IHostApplicationBuilderHelper.DefaultIHostApplicationBuilder!);

        var configuration = IHostApplicationBuilderHelper.DefaultIHostApplicationBuilder.Configuration;

        SmtpClientList = configuration.GetSection("SmtpSettings").Get<List<SmtpClient>>()!;
    }
}