using Microsoft.Extensions.Hosting;

namespace Sendify.Settings;

public static class IHostApplicationBuilderHelper
{
    public static IHostApplicationBuilder? DefaultHostApplicationBuilder { get; private set; }

    public static void SetHostApplicationBuilder(this IHostApplicationBuilder hostApplicationBuilder)
    {
        DefaultHostApplicationBuilder = hostApplicationBuilder;
    }
}