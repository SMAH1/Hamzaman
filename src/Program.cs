using Hamzaman;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

static class Program
{
    public enum ErrorMainReturn : int
    {
        OK = 0,

        ErrorConfigFileNotExists = 1,
        ErrorJsonFormatOfConfigFile = 2,
        ErrorReadConfigFile = 3,
        ErrorConfigInvalidate = 4,
        ErrorHttpsCertificateInvalidate = 5,

        ErrorUnknown = 100,
    }

    [STAThread]
    static int Main(string[] args)
    {
        ErrorMainReturn ret = ErrorMainReturn.OK;
        string? appSettingFile = null;

        if (NeedShowHelp(args))
            return 0;

        (ret, appSettingFile) = GetSettingFile(args);
        if (ret != ErrorMainReturn.OK) return (int)ret;

        if (appSettingFile is not null)
        {
            ret = CheckJsonFile(appSettingFile);
            if (ret != ErrorMainReturn.OK) return (int)ret;
        }

        Initialize(appSettingFile);

        return 0;
    }

    private static bool NeedShowHelp(string[] args)
    {
        if (args.Length > 0)
        {
            foreach (var str in args)
            {
                if (string.Compare(str, "--help", true) == 0 || string.Compare(str, "-h", true) == 0)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Hamzaman is an app for creating real-time collaborative apps.");
                    Console.WriteLine("");
                    Console.WriteLine("Help:");
                    Console.WriteLine("  --help, -h : Show this help");
                    Console.WriteLine("  --connfig  : Change configuration file (default is 'appsettings.json')");
                    Console.WriteLine("");
                    Console.WriteLine("See more information: https://github.com/SMAH1/Hamzaman");
                    return true;
                }
            }
        }
        return false;
    }

    static (ErrorMainReturn, string?) GetSettingFile(string[] args)
    {
        string? appSettingFile = null;
        if (args.Length >= 2 && string.Compare(args[0], "--config", true) == 0)
        {
            appSettingFile = args[1];
            if (!File.Exists(appSettingFile))
            {
                Console.Error.WriteLine($"File '{appSettingFile}' is not exists.");
                return (ErrorMainReturn.ErrorConfigFileNotExists, null);
            }
        }
        return (ErrorMainReturn.OK, appSettingFile);
    }

    static ErrorMainReturn CheckJsonFile(string appSettingFile)
    {
        try
        {
            using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(appSettingFile))) { }
        }
        catch (JsonException)
        {
            Console.Error.WriteLine($"File '{appSettingFile}' is not JSON format.");
            return ErrorMainReturn.ErrorJsonFormatOfConfigFile;
        }
        catch
        {
            Console.Error.WriteLine($"Uknown error in read file '{appSettingFile}'.");
            return ErrorMainReturn.ErrorReadConfigFile;
        }
        return ErrorMainReturn.OK;
    }

    static ErrorMainReturn Initialize(string? appSettingFile)
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.Services.AddSignalR();
        builder.Services.Configure<JsonHubProtocolOptions>(o =>
        {
            o.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        // Application Settings  --------------------------------------------
        AppSettings appSettings = new AppSettings();
        if (appSettingFile is not null)
            builder.Configuration.AddJsonFile(appSettingFile, true, false);
        var configuration = builder.Configuration.GetSection("AppSettings");
        builder.Services.Configure<AppSettings>(configuration);
        configuration.Bind(appSettings);

        if (!appSettings.IsHttpEnable() && !appSettings.IsHttpsEnable())
        {
            Console.Error.WriteLine($"HTTP and HTTPS are disabled!");
            return ErrorMainReturn.ErrorConfigInvalidate;
        }

        if (
            !appSettings.Message.IsEnableAndAvailable() &&
            !appSettings.Server.IsEnableAndAvailable()
            )
        {
            Console.Error.WriteLine($"Message hub and Server hub are disabled!");
            return ErrorMainReturn.ErrorConfigInvalidate;
        }

        // HTTPS  -----------------------------------------------------------
        builder.WebHost.UseKestrel(options =>
        {
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                if (appSettings.HttpsCertificate.IsEnableAndAvailable())
                {
                    httpsOptions.ServerCertificate = X509CertificateLoader.LoadPkcs12FromFile(
                        appSettings.HttpsCertificate.PfxFile,
                        appSettings.HttpsCertificate.Password);
                }
            });
        });

        // Embedded Files  --------------------------------------------------
        var embeddedFiles = new EmbeddedFiles();

        // CORS  ------------------------------------------------------------
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    if (appSettings.IsHttpEnable()) policy.WithOrigins($"http://{appSettings.Host}:{appSettings.HttpPort}");
                    if (appSettings.IsHttpsEnable()) policy.WithOrigins($"https://{appSettings.Host}:{appSettings.HttpsPort}");

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
        builder.Services.AddSingleton(embeddedFiles);

        // APP  -------------------------------------------------------------
        var app = builder.Build();

        if (appSettings.IsHttpEnable())
            app.Urls.Add($"http://{appSettings.Host}:{appSettings.HttpPort}");
        else
            Console.WriteLine("HTTP disabled!");

        if (appSettings.IsHttpsEnable())
            app.Urls.Add($"https://{appSettings.Host}:{appSettings.HttpsPort}");
        else
            Console.WriteLine("HTTPS disabled!");

        app.UseCors();

        if (appSettings.Message.IsEnableAndAvailable())
        {
            app.MapHub<MessageHub>("/" + appSettings.Message.Hub);
        }
        else
            Console.WriteLine("Message Hub disabled!");

        if (appSettings.Server.IsEnableAndAvailable())
        {
            app.MapHub<ServerHub>("/" + appSettings.Server.Hub);
        }
        else
            Console.WriteLine("Server Hub disabled!");

        app.MapGet("/", () => TypedResults.Redirect("/index.html"));
        app.StaticFilesApi(appSettings, embeddedFiles);

        try
        {
            app.Run();
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            Console.Error.WriteLine($"Error in load of certificate file '{appSettings.HttpsCertificate.PfxFile}' or password error.");
            return ErrorMainReturn.ErrorHttpsCertificateInvalidate;
        }
        catch (Exception exp)
        {
            Console.Error.WriteLine(exp.Message);
            return ErrorMainReturn.ErrorUnknown;
        }

        return ErrorMainReturn.OK;
    }
}

[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }

