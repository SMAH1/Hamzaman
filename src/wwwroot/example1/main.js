

const txtName = document.getElementById('name');
const txtplayers = document.getElementById('players');
const board = document.getElementById('board');

function createGrid() {
    for (let i = 0; i < 100; i++) {
        const cell = document.createElement('div');
        cell.classList.add('grid-item');
        cell.textContent = ` `;
        cell.addEventListener('click', function () {
            hub.send('click', JSON.stringify({x: Math.floor(i / 10), y: i % 10}));
        });
        board.appendChild(cell);
    }
}

function getCell(rowIndex, cellIndex) {
    const cellIndexInGrid = rowIndex * 10 + cellIndex;
    const cell = board.children[cellIndexInGrid];
    return cell;
}

function setCellColor(rowIndex, cellIndex, color) {
    getCell(rowIndex, cellIndex).style.backgroundColor = color;
}

createGrid();

// ---------------------- SERVER ---------------

function infoReceived(info) {
    txtName.textContent = "Player " + info.id;
    txtName.style.backgroundColor = info.color;
}

function clickReceived(info) {
    setCellColor(info.x, info.y, info.color);
}

function playersReceived(info) {
    txtplayers.textContent = "Players count is " + info.count + ".";
}

function updateMessage(func, argument) {
   switch(func) {
    case 'info': infoReceived(JSON.parse(argument)); break;
    case 'click': clickReceived(JSON.parse(argument)); break;
    case 'players': playersReceived(JSON.parse(argument)); break;
   }
}

const hub = new MessageHub("/serverHub", updateMessage);
