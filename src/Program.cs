using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;
using Hamzaman;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSignalR();
builder.Services.Configure<JsonHubProtocolOptions>(o =>
{
    o.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// HTTPS  -----------------------------------------------------------
builder.WebHost.UseKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
    });
});

// Application Settings  --------------------------------------------
AppSettings appSettings = new AppSettings();
var configuration = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(configuration);
configuration.Bind(appSettings);

// CORS  ------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins($"http://{appSettings.Host}:{appSettings.HttpPort}");
            if (appSettings.HttpsPort > 0 && appSettings.HttpsPort != appSettings.HttpPort)
                policy.WithOrigins($"https://{appSettings.Host}:{appSettings.HttpsPort}");

            foreach (var cors in appSettings.CORS)
                policy.WithOrigins(cors);

            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
            ;
        });
});

// Singaleton -------------------------------------------------------
builder.Services.AddSingleton(appSettings);

// APP  -------------------------------------------------------------
var app = builder.Build();

app.Urls.Add($"http://{appSettings.Host}:{appSettings.HttpPort}");
if (appSettings.HttpsPort > 0 && appSettings.HttpsPort != appSettings.HttpPort)
    app.Urls.Add($"https://{appSettings.Host}:{appSettings.HttpsPort}");
else
    Console.WriteLine("HTTPS disabled!");

app.UseCors();

if (appSettings.Message.Enable && !string.IsNullOrEmpty(appSettings.Message.Hub))
{
    app.MapHub<MessageHub>("/" + appSettings.Message.Hub);
}
else
    Console.WriteLine("Message Hub disabled!");

if (appSettings.Server.Enable && !string.IsNullOrEmpty(appSettings.Server.Hub))
{
    app.MapHub<ServerHub>("/" + appSettings.Server.Hub);
}
else
    Console.WriteLine("Server Hub disabled!");

app.MapGet("/", () => TypedResults.Redirect("/index.html"));
app.StaticFilesApi(appSettings);

app.Run();

[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }

