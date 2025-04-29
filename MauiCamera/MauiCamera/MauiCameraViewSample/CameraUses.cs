using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core.Primitives;

namespace MauiCameraViewSample
{
    public class CameraUses
    {
        private readonly ICameraProvider cameraProvider;
        private readonly CameraView MyCamera;
        private readonly Image MyImage;
        private readonly WebSocketServer _webSocketServer;
        private readonly Image _imageControl;

        public CameraUses(ICameraProvider cameraProvider, CameraView cameraView, Image image)
        {
            this.cameraProvider = cameraProvider;
            this.MyCamera = cameraView;
            this.MyImage = image;

            MyCamera.MediaCaptured += MyCamera_MediaCaptured;
        }

        public async Task InitializeCameraAsync()
        {
            await cameraProvider.RefreshAvailableCameras(CancellationToken.None);
            // Aqui substituímos o CameraPosition.Rear por uma busca pela câmera traseira
            MyCamera.SelectedCamera = cameraProvider.AvailableCameras
                .Where(c => c.Position == CameraPosition.Rear).FirstOrDefault();
        }

        public void Dispose()
        {
            MyCamera.MediaCaptured -= MyCamera_MediaCaptured;
            MyCamera.Handler?.DisconnectHandler();
        }

        private async void MyCamera_MediaCaptured(object? sender, MediaCapturedEventArgs e)
        {
            // Acessando o Dispatcher via MainThread
            if (MainThread.IsMainThread)
            {
                MyImage.Source = ImageSource.FromStream(() => e.Media);
                return;
            }

            // Caso o código não esteja no thread principal, usamos o MainThread para atualizar
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MyImage.Source = ImageSource.FromStream(() => e.Media);
            });

            // Envia a imagem pelo WebSocket
            using var memoryStream = new MemoryStream();
            await e.Media.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            await _webSocketServer.SendImageToAllClients(imageBytes);

        }

        public async Task CaptureImageAsync(CancellationToken cancellationToken)
        {
            await MyCamera.CaptureImage(cancellationToken);
        }
    }
}
