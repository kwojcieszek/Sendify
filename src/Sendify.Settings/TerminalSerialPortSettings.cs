using Sendify.Shared;

namespace Sendify.Settings;

public class TerminalSerialPortSettings
{
    public string PortName { get; set; }
    public static TerminalSerialPortSettings Instance { get; } = new();

    private TerminalSerialPortSettings()
    {
        ThrowHelper.ThrowIfNull(IHostApplicationBuilderHelper.DefaultHostApplicationBuilder!);
        
        var configuration = IHostApplicationBuilderHelper.DefaultHostApplicationBuilder.Configuration;

        PortName = configuration["TerminalSerialPort:PortName"]!;
    }
}