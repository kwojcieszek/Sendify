using Microsoft.Extensions.Configuration;
using Sendify.Shared;

namespace Sendify.Settings;

public class DatabaseSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public static DatabaseSettings Instance { get; } = new();

    private DatabaseSettings()
    {
        ThrowHelper.ThrowIfNull(IHostApplicationBuilderHelper.DefaultHostApplicationBuilder!);
        
        var configuration = IHostApplicationBuilderHelper.DefaultHostApplicationBuilder.Configuration;

        ConnectionString = configuration.GetValue<string>("Database:ConnectionStrings", string.Empty);
        DatabaseName = configuration.GetValue<string>("Database:DatabaseName", string.Empty);
    }
}