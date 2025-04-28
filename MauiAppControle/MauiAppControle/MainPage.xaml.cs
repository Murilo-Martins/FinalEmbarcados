using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel; // Adicione esse namespace

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
    }
}
