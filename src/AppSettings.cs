
namespace Hamzaman;

public class AppSettings
{
    public string Host { get; set; } = "localhost";
    public ushort HttpPort { get; set; } = 4000;
    public ushort HttpsPort { get; set; } = 4001;

    public string Root { get; set; } = "./wwwroot";

    public string[] CORS { get; set; } = [];

    public HttpsCertificate HttpsCertificate { get; set; } = new HttpsCertificate();
    public Message Message { get; set; } = new Message();
    public Server Server { get; set; } = new Server();
}

public class HttpsCertificate
{
    public bool Enable { get; set; } = false;
    public string PfxFile { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class Message
{
    public bool Enable { get; set; } = true;
    public string Hub { get; set; } = "messageHub";
}

public class Server : Message
{
    public Server()
    {
        Enable = false;
        Hub = "serverHub";
    }

    public string CredentialCommand { get; set; } = "";
    public string CredentialValue { get; set; } = "";
    public ServerCredentialTotp CredentialTotp { get; set; } = new ServerCredentialTotp();
}

public class ServerCredentialTotp
{
    public bool Enable { get; set; } = false;
    public string Key { get; set; } = string.Empty;
    public int Length { get; set; } = 6;
    public int Peroid { get; set; } = 60;
}

public static class AppSettingsExtensions
{
    public static bool IsEnableAndAvailable(this HttpsCertificate src)
    {
        if (!src.Enable) return false;
        if (string.IsNullOrEmpty(src.PfxFile)) return false;
        return true;
    }

    public static bool IsHttpEnable(this AppSettings src)
    {
        return src.HttpPort > 0;
    }

    public static bool IsHttpsEnable(this AppSettings src)
    {
        if (src.HttpsPort == 0) return false;
        if (src.HttpsPort == src.HttpPort) return false;
        return true;
    }

    public static bool IsEnableAndAvailable(this Message src)
    {
        if (!src.Enable) return false;
        if (string.IsNullOrEmpty(src.Hub)) return false;
        return true;
    }

    public static bool IsEnableAndAvailable(this ServerCredentialTotp src)
    {
        if (!src.Enable) return false;
        if (string.IsNullOrEmpty(src.Key)) return false;
        return true;
    }
}
