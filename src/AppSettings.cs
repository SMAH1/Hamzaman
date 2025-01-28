
namespace Hamzaman;

public class AppSettings
{
    public string Host { get; set; } = "localhost";
    public ushort HttpPort { get; set; } = 4000;
    public ushort HttpsPort { get; set; } = 4001;

    public string Root { get; set; } = "./wwwroot";

    public string[] CORS { get; set; } = [];

    public Message Message { get; set; } = new Message();
    public Server Server { get; set; } = new Server();
}

public class Message
{
    public bool Enable { get; set; } = true;
    public string Hub { get; set; } = "messageHub";
}

public class Server
{
    public bool Enable { get; set; } = false;
    public string Hub { get; set; } = "serverHub";
    public string CredentialCommand { get; set; } = "";
    public string CredentialValue { get; set; } = "";
}
