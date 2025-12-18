using Sendify.JobsWorker;
using Sendify.Settings;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings_local.json", optional: true, reloadOnChange: true);

builder.Services.AddHostedService<Worker>();

builder.SetHostApplicationBuilder();

ServicesSettings.SetServices(builder.Services);

var host = builder.Build();
host.Run();
