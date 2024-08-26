using Android.Util;
using capture_xamarin_sdk_sample.Droid;
using SocketMobile.Capture;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidCaptureExtensionInit))]
namespace capture_xamarin_sdk_sample.Droid
{
    public class AndroidCaptureExtensionInit : IAndroidCaptureExtensionInit
    {
        CaptureExtension captureExtension;
        public void CallAndroidCaptureExtensionInit(int captureHandle)
        {
            captureExtension = new CaptureExtension(Android.App.Application.Context, captureHandle);
            captureExtension.Error += CaptureExtension_Error;
            captureExtension.Start();
        }

        private void CaptureExtension_Error(object sender, CaptureExtensionErrorEventArgs e)
        {
            Log.Debug("Error", $" Capture Extension  - Code: {e.Code} Message: {e.Message}");
        }
    }
}
