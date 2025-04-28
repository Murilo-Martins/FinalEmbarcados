using System.IO;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace ControleV1.Functions
{
    public static class MoveCarFunction
    {
        [FunctionName("MoveCar")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            ILogger log)
        {
            // Leitura do corpo da requisição
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string direction = data?.direction;

            if (string.IsNullOrEmpty(direction))
            {
                return new BadRequestObjectResult("Direction is required.");
            }

            // Enviar o comando via TCP para o servidor que controla o carrinho
            try
            {
                string tcpServerAddress = "IP_DO_SEU_SERVIDOR_TCP"; // Substitua pelo endereço IP do seu servidor TCP
                int tcpPort = 12345; // Porta TCP do seu servidor

                using (TcpClient client = new TcpClient(tcpServerAddress, tcpPort))
                using (NetworkStream stream = client.GetStream())
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(direction);  // Envia o comando via TCP
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Erro ao enviar o comando TCP: {ex.Message}");
                return new StatusCodeResult(500);
            }

            return new OkObjectResult($"Comando '{direction}' enviado com sucesso.");
        }
    }
}
