using System.ComponentModel;
using SocketMobile.Capture;
using Xamarin.Forms;

namespace SampleApp
{
    public partial class MainPage : ContentPage
    {
        private string _displayText = $"Decoded Data: ";
        public string DisplayText
        {
            get => _displayText;
            set
            {
                if (_displayText != value)
                {
                    _displayText = value;
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        private string _deviceEventText = $"Device Event: ";
        public string DeviceEventText
        {
            get => _deviceEventText;
            set
            {
                if (_deviceEventText != value)
                {
                    _deviceEventText = value;
                    OnPropertyChanged(nameof(DeviceEventText));
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();

            string appId = "";
            string developerId = "";
            string appKey = "";

            myLabel.BindingContext = this;
            eventLabel.BindingContext = this;

            if (Device.RuntimePlatform == Device.iOS)
            {
                appId = "ios:com.socketmobile.multiplatformtest";
                developerId = "ec9e1b16-c5ae-ec11-983e-000d3a5bbc61";
                appKey = "MC0CFQCmP6oKJtSK5RXgIpPBUhXsjg63rwIUbS8tmz3f2JpdoNjfhHWGmjv5+AI=";

            }
            if (Device.RuntimePlatform == Device.Android)
            {
                appId = "android:com.socketmobile.multiplatformtest.android";
                developerId = "ec9e1b16-c5ae-ec11-983e-000d3a5bbc61";
                appKey = "MCwCFBOpqbd3TEZivPYJ2oKHKASgPD+yAhRVLLyX/+yQpa3LXAMC+MpgiwdXsA==";
            }

            capture.OpenAsync(appId, developerId, appKey)
            .ContinueWith(result => {
                System.Diagnostics.Debug.Print("Open Capture returns {0}", result.Result);
                if (SktErrors.SKTSUCCESS(result.Result))
                {
                    capture.DeviceArrival += Capture_DeviceArrival;
                    capture.DeviceRemoval += Capture_DeviceRemoval; ;
                    capture.DecodedData += Capture_DecodedData;
                }
            });
        }

        private void Capture_DeviceRemoval(object sender, CaptureHelper.DeviceArgs e)
        {
            DeviceEventText = string.Format("Device Removal: {0}", e.CaptureDevice.GetDeviceInfo().Name);
        }

        private void Capture_DeviceArrival(object sender, CaptureHelper.DeviceArgs e)
        {
            DeviceEventText = string.Format("Device Arrival: {0}", e.CaptureDevice.GetDeviceInfo().Name);
        }

        private void Capture_DecodedData(object sender, CaptureHelper.DecodedDataArgs e)
        {
            DisplayText = string.Format("Decoded Data: {0}", e.DecodedData.DataToUTF8String);
        }

        public CaptureHelper capture = new CaptureHelper();
    }
}
