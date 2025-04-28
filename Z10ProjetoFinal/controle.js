// controle.js
document.addEventListener('DOMContentLoaded', () => {
    const buttons = {
        up: document.getElementById('btn-up'),
        left: document.getElementById('btn-left'),
        down: document.getElementById('btn-down'),
        right: document.getElementById('btn-right')
    };

    // Função que envia o comando para o backend
    function sendCommand(command) {
        fetch('https://seu-backend-url.azurewebsites.net/api/move', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ direction: command })
        })
        .then(response => response.json())
        .catch(error => console.error('Erro ao enviar comando:', error));
    }

    // Adicionar event listeners para os botões
    buttons.up.addEventListener('click', () => sendCommand('up'));
    buttons.left.addEventListener('click', () => sendCommand('left'));
    buttons.down.addEventListener('click', () => sendCommand('down'));
    buttons.right.addEventListener('click', () => sendCommand('right'));
});
