using Sendify.Shared;

namespace Sendify.Settings;

public class TerminalSerialPortSettings
{
    public string PortName { get; set; }
    public static TerminalSerialPortSettings Instance { get; } = new();

    private TerminalSerialPortSettings()
    {
        ThrowHelper.ThrowIfNull(IHostApplicationBuilderHelper.DefaultIHostApplicationBuilder!);
        
        var configuration = IHostApplicationBuilderHelper.DefaultIHostApplicationBuilder.Configuration;

        PortName = configuration["TerminalSerialPort:PortName"]!;
    }
}