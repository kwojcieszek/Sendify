using Microsoft.Extensions.Hosting;

namespace Sendify.Settings;

public static class IHostApplicationBuilderHelper
{
    public static IHostApplicationBuilder DefaultIHostApplicationBuilder { get; set; }

    public static void SetHostApplicationBuilder(this IHostApplicationBuilder hostApplicationBuilder)
    {
        DefaultIHostApplicationBuilder = hostApplicationBuilder;
    }
}