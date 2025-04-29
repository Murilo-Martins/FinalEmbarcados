using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.ApplicationModel;

namespace MauiCameraViewSample
{
    public partial class MainPage : ContentPage
    {
        private CameraUses cameraUses;

        public MainPage(ICameraProvider cameraProvider)
        {
            InitializeComponent();

            // Instanciando a classe CameraUses
            cameraUses = new CameraUses(cameraProvider, MyCamera, MyImage);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Inicializa a câmera ao aparecer
            await cameraUses.InitializeCameraAsync();
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
