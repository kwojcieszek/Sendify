using Sendify.JobsWorker;
using Sendify.Settings;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.SetHostApplicationBuilder();

ServicesSettings.SetServices(builder.Services);

var host = builder.Build();
host.Run();
