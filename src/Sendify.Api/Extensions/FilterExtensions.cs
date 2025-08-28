using Sendify.DataManager;
using Sendify.FilterService;

namespace Sendify.Api.Extensions;

public static class FilterExtensions
{
    public static void SetFilterExtensions(this IServiceCollection services)
    {
        var db = new DataContext();

        var filterMessages = db.Patterns.ToArray();

        services.AddSingleton<IFilter>(provaider=> new Filter(filterMessages));
    }
}