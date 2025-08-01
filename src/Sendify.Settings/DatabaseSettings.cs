using Microsoft.Extensions.Configuration;

namespace Sendify.Settings;

public class DatabaseSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public static DatabaseSettings Instance { get; } = new DatabaseSettings();

    private DatabaseSettings()
    {
        //var configuration = IHostApplicationBuilderHelper.DefaultIHostApplicationBuilder.Configuration;

        //ConnectionString = configuration.GetValue<string>("Database:ConnectionStrings",string.Empty);
        //DatabaseName = configuration.GetValue<string>("Database:DatabaseName", string.Empty);

        ConnectionString = "mongodb://sendify:9cyWXTtsDFilS5jVBtU@172.30.3.2:27017/?authSource=Sendify";
        DatabaseName = "Sendify";
    }
}