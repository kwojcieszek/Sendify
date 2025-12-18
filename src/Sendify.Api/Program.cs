using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Sendify.Api.Extensions;
using Sendify.Settings;
using Sendify.ServiceCollection.Extensions;
using Microsoft.AspNetCore.Authorization;
using Sendify.Api.Common;
using Sendify.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings_local.json", optional: true, reloadOnChange: true);

builder.SetHostApplicationBuilder();

builder.Services.SetDatabaseSettings();

builder.Services.SetPasswordSettings();

builder.Services.SetFilterExtensions();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
    o.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver"));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        RequireExpirationTime = false,
        ValidAudience = Sendify.Settings.JwtSettings.Instance.JwtValidAudience,
        ValidIssuer = Sendify.Settings.JwtSettings.Instance.JwtValidIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Sendify.Settings.JwtSettings.Instance.JwtSecret))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiTokenPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new ApiTokenRequirement());
    });
});

builder.Services.AddSingleton<ITokensService, ApiTokensService>(p => new ApiTokensService(Sendify.Settings.JwtSettings.Instance.JwtValidIssuer,
        Sendify.Settings.JwtSettings.Instance.JwtValidAudience, Sendify.Settings.JwtSettings.Instance.JwtSecret));

builder.Services.AddSingleton<IAuthorizationHandler, ApiAuthorizationHandler>();

builder.Services.AddSingleton<IPasswordService, PasswordSha256>();

builder.Services.AddSingleton<IAuthentication, ApiAuthentication>();

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.DefaultModelsExpandDepth(-1);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();