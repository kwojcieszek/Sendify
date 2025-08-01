using Sendify.MessagesWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

ServicesSettings.SetServices(builder.Services);

var host = builder.Build();

host.Run();
