﻿using Microsoft.AspNetCore.SignalR;

namespace Hamzaman;

// SERVER ----------
//   FROM endpoint
//     SendMessageToClient(string connectionId, string func, string argument)
//   TO endpoint
//     ReceiveMessageFromClient(string connectionId, string func, string argument)
//     UserConnected(string connectionId)
//     UserDisconnected(string connectionId)

// CLIENT ----------
//   FROM endpoint
//     SendMessage(string func, string argument)
//   TO endpoint
//     ReceiveMessage(string func, string argument)

public class ServerHub : Hub
{
    private const string GROUP_NAME = "server";
    private static string ServerConnectionId = "";
    private static bool HasServer => !string.IsNullOrEmpty(ServerConnectionId);

    private readonly AppSettings _appSettings;

    public ServerHub(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public override async Task OnConnectedAsync()
    {
        if (HasServer)
        {
            await Clients.Client(ServerConnectionId).CallUserConnectedAsync(Context.ConnectionId);
            await base.OnConnectedAsync();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (HasServer)
        {
            await Clients.Client(ServerConnectionId).CallUserDisconnectedAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }

    public async Task SendMessage(string func, string argument)
    {
        if (string.IsNullOrEmpty(func))
            return;

        if (HasServer)
        {
            await Clients.Client(ServerConnectionId).CallReceiveMessageAsync(Context.ConnectionId, func, argument);
        }
    }

    public async Task SendMessageToClient(string connectionId, string func, string argument)
    {
        if (string.IsNullOrEmpty(func))
            return;

        if (string.IsNullOrEmpty(connectionId))
        {
            if (string.Compare(func, _appSettings.Server.CredentialCommand) == 0)
            {
                if (string.Compare(argument, _appSettings.Server.CredentialValue) == 0)
                {
                    if (string.Compare(ServerConnectionId, Context.ConnectionId) != 0)
                    {
                        if (HasServer)
                        {
                            await Clients.Group(GROUP_NAME).CallReceiveMessageAsync("", func, "EXIT");
                            await Groups.RemoveFromGroupAsync(ServerConnectionId, GROUP_NAME);
                        }

                        ServerConnectionId = Context.ConnectionId;
                        await Groups.AddToGroupAsync(ServerConnectionId, GROUP_NAME);
                        await Clients.Caller.CallReceiveMessageAsync("", func, "SERVER");
                    }
                }
            }
        }
        else if (HasServer && Context.ConnectionId == ServerConnectionId)
        {
            try
            {
                await Clients.Client(connectionId).CallReceiveMessageAsync(func, argument);
            }
            catch { }
        }
    }
}

public static class IClientProxyExtension
{
    public static Task CallReceiveMessageAsync(this IClientProxy clientProxy, string func, string arg, CancellationToken cancellationToken = default)
    {
        return clientProxy.SendCoreAsync("ReceiveMessage", new[] { func, arg }, cancellationToken);
    }

    public static Task CallReceiveMessageAsync(this IClientProxy clientProxy, string cid, string func, string arg, CancellationToken cancellationToken = default)
    {
        return clientProxy.SendCoreAsync("ReceiveMessageFromClient", new[] { cid, func, arg }, cancellationToken);
    }

    public static Task CallUserConnectedAsync(this IClientProxy clientProxy, string cid, CancellationToken cancellationToken = default)
    {
        return clientProxy.SendCoreAsync("UserConnected", new[] { cid }, cancellationToken);
    }

    public static Task CallUserDisconnectedAsync(this IClientProxy clientProxy, string cid, CancellationToken cancellationToken = default)
    {
        return clientProxy.SendCoreAsync("UserDisconnected", new[] { cid }, cancellationToken);
    }
}
