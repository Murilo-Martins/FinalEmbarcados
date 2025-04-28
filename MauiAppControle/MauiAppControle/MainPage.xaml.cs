using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System;

namespace MauiAppControle
{
    public partial class MainPage : ContentPage
    {
        private WebSocketServer _webSocketServer;

        public string MensagemRecebida { get; set; }
        public string StatusConexao { get; set; } = "Desconectado"; // Começa como Desconectado

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
                MensagemRecebida = mensagem;
                OnPropertyChanged(nameof(MensagemRecebida));
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
    }
}
