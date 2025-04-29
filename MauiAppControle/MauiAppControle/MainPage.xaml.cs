using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel; // Adicione esse namespace
using CommunityToolkit.Maui.Views;
using static Android.Print.PrintAttributes;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace MauiAppControle
{
    public partial class MainPage : ContentPage
    {
        private WebSocketServer _webSocketServer;

        public string StatusConexao { get; set; } = "Desconectado"; // Começa como Desconectado

        // ObservableCollection para armazenar as mensagens
        public ObservableCollection<string> MensagensRecebidas { get; set; } = new ObservableCollection<string>();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            _webSocketServer = new WebSocketServer(UpdateMensagemRecebida, UpdateStatusConexao);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _webSocketServer.StartAsync("http://192.168.0.169:8080/");
        }

        private void UpdateMensagemRecebida(string mensagem)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MensagensRecebidas.Add(mensagem); // Adiciona a nova mensagem à lista
                OnPropertyChanged(nameof(MensagensRecebidas));
            });
        }

        private void UpdateStatusConexao(string status)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusConexao = status;
                OnPropertyChanged(nameof(StatusConexao));
            });
        }
        private async void OnCaptureAndSendFrameClicked(object sender, EventArgs e)
        {
            try
            {
                mensagemLabel.Text = "Solicitando permissão...";

                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        mensagemLabel.Text = "Permissão de câmera negada.";
                        return;
                    }
                }

                mensagemLabel.Text = "Capturando imagem...";
                await cameraView.CaptureImage(CancellationToken.None);
            }
            catch (Exception ex)
            {
                mensagemLabel.Text = $"Erro ao capturar: {ex.Message}";
                Console.WriteLine($"Erro ao capturar imagem: {ex}");
            }
        }

        private void MyCamera_MediaCaptured(object sender, CommunityToolkit.Maui.Views.MediaCapturedEventArgs e)
        {
            mensagemLabel.Text = "Entrei";
            Console.WriteLine($"e.Media is null: {e.Media == null}");

            if (e.Media != null)
            {
                if (Dispatcher.IsDispatchRequired)
                {
                    Dispatcher.Dispatch(() => imagePreview.Source = ImageSource.FromStream(() => e.Media));
                    mensagemLabel.Text = "Testouaquidentro";
                    return;
                }

                imagePreview.Source = ImageSource.FromStream(() => e.Media);
                mensagemLabel.Text = "Testouaquifora";
            }
            else
            {
                mensagemLabel.Text = "Erro: e.Media está nulo no evento MediaCaptured.";
                Console.WriteLine("Erro: e.Media está nulo no evento MediaCaptured.");
                // Adicione aqui qualquer tratamento de erro adicional
            }
        }
        private async Task CaptureAndSendFrame()
        {
            //var screenshotResult = await cameraView.CaptureAsync();
            
            

            /*
            if (screenshotResult != null)

            {
                using (var stream = await screenshotResult.OpenReadAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);

                        //byte[] imageBytes = memoryStream.ToArray();
                        //imagePreview.Source = ImageSource.FromStream(() => stream);
                        //await _webSocketServer.SendImageToAllClients(imageBytes);
                    }
                }
            }*/
        }


        private async Task<byte[]> ConvertImageToByteArray(FileResult fileResult)
        {
            using (var stream = await fileResult.OpenReadAsync()) // Usa o método OpenReadAsync() do FileResult
            {
                byte[] byteArray = new byte[stream.Length];
                await stream.ReadAsync(byteArray, 0, (int)stream.Length);
                return byteArray;
            }
        }
    }
}
