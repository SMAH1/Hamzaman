using Microsoft.AspNetCore.SignalR;

namespace Hamzaman;

// CLIENT ----------
//   FROM endpoint
//     SendMessage(string func, string argument)
//   TO endpoint
//     ReceiveMessage(string func, string argument)

public class MessageHub : Hub
{
    public async Task SendMessage(string func, string argument)
    {
        await Clients.AllExcept([Context.ConnectionId]).SendAsync("ReceiveMessage", func, argument);
    }
}
