
let listConnected = [];
let listDisconnected = [];
let playerCount = 0;
let dicPlayerInfo = {};

colors = ['red', 'green', 'blue', 'yellow', 'cyan', 'magenta', 'orange', 'purple', 'brown', 'pink', 'lime', 'teal', 'navy', 'olive', 'maroon', 'gold', 'silver', 'gray', 'coral', 'khaki'];

let board = [];
for (let i = 0; i < 10; i++) {
    board[i] = [];
    for (let j = 0; j < 10; j++) {
        board[i][j] = null;
    }
}

const txtMessage = document.getElementById('msg');

function addMessage(str) {
    const pElement = document.createElement('p');
    pElement.textContent = str;
    txtMessage.appendChild(pElement);
    setTimeout(() => {
        txtMessage.scrollTop = txtMessage.scrollHeight - 1;
    }, 100);
}

function updatePlayerCount() {
    const countPlayers = Object.keys(dicPlayerInfo).length;
    Object.keys(dicPlayerInfo).forEach(function (cid) {
        hub.sendToClient(
            cid,
            "players",
            JSON.stringify({ count: countPlayers })
        );
    });
}

//------------------------------------------------- HUB
function updateMessage(connectionId, func, argument) {
    if(func == 'click')
    {
        argument = JSON.parse(argument);
        if(board[argument.x][argument.y])
            addMessage("Player " + dicPlayerInfo[connectionId].id + " => click on (" + argument.x + "," + argument.y + ") and NOT ACCEPTED!");
        else
        {
            addMessage("Player " + dicPlayerInfo[connectionId].id + " => ACCEPTED click on (" + argument.x + "," + argument.y + ").");
            board[argument.x][argument.y] = dicPlayerInfo[connectionId].color;

            Object.keys(dicPlayerInfo).forEach(function(cid) { 
                hub.sendToClient(
                    cid, 
                    "click",
                    JSON.stringify({x: argument.x, y: argument.y, color: dicPlayerInfo[connectionId].color})
                );
            });
        }
    }
    else if(func == 'ItsMe')
    {
        addMessage(argument);
    }
    else
    {
        addMessage(connectionId + " => (" + func + "): " + argument + "");
    }
}

function connectionState(connectionId, state) {
    state = JSON.parse(state);
    if (state) {
        listDisconnected = listDisconnected.filter(cid => cid !== connectionId);
        listConnected.push(connectionId);

        playerCount++;
        dicPlayerInfo[connectionId] = {id: playerCount, color: colors[(playerCount-1) % colors.length]}
        addMessage("New Player (" + playerCount + "): " + connectionId);

        hub.sendToClient(connectionId, "info", JSON.stringify(dicPlayerInfo[connectionId]));
    
        for (let i = 0; i < 10; i++) {
            for (let j = 0; j < 10; j++) {
                if(board[i][j])
                {
                    hub.sendToClient(
                        connectionId, 
                        "click",
                        JSON.stringify({
                            x: i, 
                            y: j, 
                            color: board[i][j]})
                    ); 
                }
            }
        }

        updatePlayerCount();
    } else {
        listConnected = listConnected.filter(cid => cid !== connectionId);
        listDisconnected.push(connectionId);
        addMessage("Disconnect Player: " + dicPlayerInfo[connectionId].id);
        delete dicPlayerInfo[connectionId];

        updatePlayerCount();
    }
}

addMessage('Ready');

const hub = new ServerHub("", updateMessage, connectionState);
setTimeout(() => {
    hub.sendToClient('', 'ItsMe', "PASSWORD");
}, 1000);

