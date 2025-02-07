
function MessageHub(address, receiverCallback) {
    this.receiver = receiverCallback;

    if (address === undefined || address === null || address === '')
        address = undefined;

    this.connection = new signalR.HubConnectionBuilder()
        .withUrl((address ?? "/messageHub"))
        .build();

    this.connection.on("ReceiveMessage", (func, argument) => {
        this.receiver(func, argument);
    });

    this.connection.start().catch(err => console.error(err));

    this.connection.onreconnected((cid) => { this.receiver('', 'RECONNECTED'); });

    this.connection.start().then(() => { this.receiver('', 'CONNECTED'); }).catch(err => console.error(err));
}

MessageHub.prototype.send = async function (func, message) {
    await this.connection.invoke("SendMessage", func, message);
};

function ServerHub(address, receiverCallback, connectionStateCallback) {
    this.receiver = receiverCallback;
    this.connectionState = connectionStateCallback;

    if (address === undefined || address === null || address === '')
        address = undefined;

    this.connection = new signalR.HubConnectionBuilder()
        .withUrl((address ?? "/serverHub"))
        .build();

    this.connection.on("ReceiveMessageFromClient", (connectionId, func, argument) => {
        this.receiver(connectionId, func, argument);
    });
    
    this.connection.on("UserConnected", (connectionId) => {
        if(this.connectionState) this.connectionState(connectionId, true);
    });
    
    this.connection.on("UserDisconnected", (connectionId) => {
        if(this.connectionState) this.connectionState(connectionId, false);
    });

    this.connection.start().catch(err => console.error(err));

    this.connection.onreconnected((cid) => { this.receiver('', 'RECONNECTED'); });

    this.connection.start().then(() => { this.receiver('', 'CONNECTED'); }).catch(err => console.error(err));
}

ServerHub.prototype.sendToClient = async function (connectionId, func, message) {
    await this.connection.invoke("SendMessageToClient", connectionId, func, message);
};
