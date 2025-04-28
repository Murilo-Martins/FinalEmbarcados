using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MauiAppControle
{
    public class WebSocketServer
    {
        private readonly Action<string> _updateMensagemRecebida;
        private readonly Action<string> _updateStatusConexao;
        private HttpListener _httpListener;

        // Aqui criamos o construtor
        public WebSocketServer(Action<string> updateMensagemRecebida, Action<string> updateStatusConexao)
        {
            _updateMensagemRecebida = updateMensagemRecebida;
            _updateStatusConexao = updateStatusConexao;
        }

        public async Task StartAsync(string url)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(url); // Define o URL do WebSocket
            _httpListener.Start();

            Console.WriteLine("Servidor WebSocket iniciado...");
            _updateStatusConexao?.Invoke("Aguardando conexão...");

            while (true)
            {
                var context = await _httpListener.GetContextAsync();
                var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null); // Contexto WebSocket
                Console.WriteLine("Cliente conectado!");

                _updateStatusConexao?.Invoke("Cliente conectado!");

                var webSocket = webSocketContext.WebSocket;
                await ReceiveMessagesAsync(webSocket);
            }
        }

        private async Task ReceiveMessagesAsync(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Recebido do cliente: {message}");

                    _updateMensagemRecebida?.Invoke(message);

                    // Opcional: mandar de volta uma resposta
                    string responseMessage = $"Mensagem recebida: {message}";
                    byte[] responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);

                    Console.WriteLine($"Mensagem enviada para o cliente: {responseMessage}");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Fechando conexão", CancellationToken.None);
                    Console.WriteLine("Conexão fechada.");
                    _updateStatusConexao?.Invoke("Conexão encerrada.");
                }
            }
        }
    }
}
