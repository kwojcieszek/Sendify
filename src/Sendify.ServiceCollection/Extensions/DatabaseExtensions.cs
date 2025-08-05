using Microsoft.Extensions.DependencyInjection;
using Sendify.DataManager;
using Sendify.Settings;

namespace Sendify.ServiceCollection.Extensions;

public static class DatabaseExtensions
{
    public static void SetDatabaseSettings(this IServiceCollection services)
    {
        var databaseSettings = DatabaseSettings.Instance;

        if (databaseSettings.ConnectionString == null || databaseSettings.DatabaseName == null)
        {
            throw new InvalidOperationException("Default database settings are not set.");
        }

        DataContext.SetDefaultDatabaseSettings(databaseSettings.ConnectionString, databaseSettings.DatabaseName);
    }
}