using Sendify.DataManager;
using Sendify.Settings;

namespace Sendify.Api.Extensions;

public static class DatabaseExtensions
{
    public static IApplicationBuilder UseDatabase(this IApplicationBuilder app)
    {
        var configuration = app.ApplicationServices.GetService<IConfiguration>();

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var connectionString = configuration["Database:ConnectionStrings"];
        var databaseName = configuration["Database:DatabaseName"];
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Database connection string is not configured.");
        }

        if (string.IsNullOrEmpty(databaseName))
        {
            throw new ArgumentException("Database name is not configured.");
        }

        DataContext.SetDefaultDatabaseSettings(connectionString, databaseName);

        return app;
    }
}