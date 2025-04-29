using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;

namespace MauiCameraViewSample
{
    public partial class MainPage : ContentPage
    {
        private CameraUses cameraUses;

        private WebSocketServer _webSocketServer;
        public string StatusConexao { get; set; } = "Desconectado"; // Começa como Desconectado
        // ObservableCollection para armazenar as mensagens
        public ObservableCollection<string> MensagensRecebidas { get; set; } = new ObservableCollection<string>();
        public MainPage(ICameraProvider cameraProvider)
        {
            InitializeComponent();

            // Instanciando a classe CameraUses
            cameraUses = new CameraUses(cameraProvider, MyCamera, MyImage);
            _webSocketServer = new WebSocketServer(UpdateMensagemRecebida, UpdateStatusConexao);
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Inicializa a câmera ao aparecer
            await cameraUses.InitializeCameraAsync();
            base.OnAppearing();
            await _webSocketServer.StartAsync("http://192.168.0.169:8080/");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Limpa os eventos e a câmera ao desaparecer
            cameraUses.Dispose();
        }

        private async void GravarCamera(object sender, EventArgs e)
        {
            await cameraUses.CaptureImageAsync(CancellationToken.None);
        }
    }
}
