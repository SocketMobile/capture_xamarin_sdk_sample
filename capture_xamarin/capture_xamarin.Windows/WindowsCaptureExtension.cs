using capture_xamarin_sdk_sample.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SocketMobile.Capture;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;

[assembly: Dependency(typeof(WindowsCaptureExtension))]
namespace capture_xamarin_sdk_sample.Windows
{
    public class WindowsCaptureExtension : IWindowsCaptureExtension
    {
        CaptureExtension captureExtension;
        UserControl userControl;
        public async void CallWindowsCaptureExtensionInit(int captureHandle, string appId, string developerId, string appKey)
        {
            captureExtension = new CaptureExtension(MainPage.appContext, captureHandle, appId, developerId, appKey);
            captureExtension.SocketCamView += Extension_SocketCamViewEvent;
            captureExtension.Error += CaptureExtension_Error;
            await captureExtension.Start();
        }

        private void CaptureExtension_Error(object sender, CaptureExtensionErrorEventArgs e)
        {
            Debug.WriteLine("Error", $" Capture Extension  - Code: {e.Code} Message: {e.Message}");
        }

        private void Extension_SocketCamViewEvent(object sender, SocketCamViewEventArgs e)
        {
            Debug.WriteLine($"----Extension_SocketCamViewEvent object: {e.SocketCamView}");

            userControl = (UserControl)e.SocketCamView;
            MainPage.DisplayUserControlView(userControl);
        }
    }
}
