using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;

namespace MauiCameraViewSample
{
    public partial class MainPage : ContentPage
    {
        private ICameraProvider cameraProvider;

        //Server
        private WebSocketServer _webSocketServer;
        public string StatusConexao { get; set; } = "Desconectado"; 
        public ObservableCollection<string> MensagensRecebidas { get; set; } = new ObservableCollection<string>();// ObservableCollection para armazenar as mensagens


        public MainPage(ICameraProvider cameraProvider)
        {
            InitializeComponent();

            this.cameraProvider = cameraProvider;
            _webSocketServer = new WebSocketServer(UpdateMensagemRecebida, UpdateStatusConexao);

        }


        protected async override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            
            await cameraProvider.RefreshAvailableCameras(CancellationToken.None);
            MyCamera.SelectedCamera = cameraProvider.AvailableCameras
                .Where(c => c.Position == CameraPosition.Rear).FirstOrDefault();
        }

        
        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            base.OnNavigatedFrom(args);

            MyCamera.MediaCaptured -= MyCamera_MediaCaptured;
            MyCamera.Handler?.DisconnectHandler();
        }

        private async void MyCamera_MediaCaptured(object? sender, CommunityToolkit.Maui.Views.MediaCapturedEventArgs e)
        {
            if (Dispatcher.IsDispatchRequired)
            {
                Dispatcher.Dispatch(() => MyImage.Source = ImageSource.FromStream(() => e.Media));
                
                Stream mediaStream = e.Media;
                try
                {
                    // Converte o Stream para byte[]
                    using (var memoryStream = new MemoryStream())
                    {
                        await mediaStream.CopyToAsync(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();

                        // Envia pelo WebSocket
                        await _webSocketServer.SendImageToAllClients(imageBytes);
                       
                    }
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine($"Erro ao enviar imagem: {ex}");
                }
                return;
            }

            

        }

        private async void GravarCamera(object sender, EventArgs e)
        {
            await MyCamera.CaptureImage(CancellationToken.None);
        }

        //------------------Server ----------------------------------------
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

    

        private bool _isCapturing = false;
        private CancellationTokenSource _captureCancellationTokenSource;

        // Adicionar ANTES de protected async override void OnNavigatedTo(NavigatedToEventArgs args)
        private async void OnStartContinuousCaptureClicked(object sender, EventArgs e)
        {
            if (!_isCapturing)
            {
                _isCapturing = true;
                _captureCancellationTokenSource = new CancellationTokenSource();
                await StartContinuousCaptureAsync(_captureCancellationTokenSource.Token);
            }
        }

        // Adicionar ANTES de protected async override void OnNavigatedTo(NavigatedToEventArgs args)
        private async Task StartContinuousCaptureAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isCapturing)
            {
                try
                {
                    await MyCamera.CaptureImage(cancellationToken);
                    await Task.Delay(500);
                }
                catch (OperationCanceledException)
                {
                    _isCapturing = false;
                    Console.WriteLine("Captura contínua cancelada.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro durante a captura contínua: {ex}");
                    _isCapturing = false;
                }
            }
            _isCapturing = false;
        }

        // ... (seu código existente)
    }

}
