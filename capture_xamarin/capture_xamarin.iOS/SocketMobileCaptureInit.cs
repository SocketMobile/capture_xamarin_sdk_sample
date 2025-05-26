using Foundation;
using capture_xamarin_sdk_sample.iOS;
using capture_xamarin_sdk_sample.Model;
using SocketMobile.Capture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(SocketMobileCaptureInit))]
namespace capture_xamarin_sdk_sample.iOS
{
    public class SocketMobileCaptureInit : ISocketMobileCaptureInit
    {
        public CaptureHelper capture = new CaptureHelper();
        public MainPage MainPage { get; set; }

        private UIViewController _socketCamViewController;

        // SocketCam view: position and dimensions (min = 250 x 250)
        private int _socketCamXPos = 0;
        private int _socketCamYPos = 0;
        private int _socketCamWidth = 250;
        private int _socketCamHeight = 250;

        public void StartCaptureCore(string appId, string developerId, string appKey)
        {
            capture.OpenAsync(appId, developerId, appKey)
            .ContinueWith(result => {
                System.Diagnostics.Debug.Print("Open Capture returns {0}", result.Result);
                if (SktErrors.SKTSUCCESS(result.Result))
                {
                    capture.DeviceArrival += Capture_DeviceArrival;
                    capture.DeviceRemoval += Capture_DeviceRemoval; ;
                    capture.DecodedData += Capture_DecodedData;

                    // (Android-iOS) Check if SocketCam is enabled to set the Switch
                    GetSocketCamStatusInit();
                }
            });
        }

        // Device Events--
        private void Capture_DeviceRemoval(object sender, CaptureHelper.DeviceArgs e)
        {
            MainPage.DeviceEventText = string.Format("Device Removal: {0}", e.CaptureDevice.GetDeviceInfo().Name);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    int i = 0;
                    foreach (var device in MainPage.deviceListItems)
                    {
                        if (device.DeviceName == e.CaptureDevice.GetDeviceInfo().Name)
                        {
                            MainPage.deviceListItems.RemoveAt(i);
                            break;
                        }
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error removing device from list: " + ex.ToString());
                }

            });

            MainPage.SelectedDeviceText = "Selected Device: ";
        }

        private void Capture_DeviceArrival(object sender, CaptureHelper.DeviceArgs e)
        {
            MainPage.DeviceEventText = string.Format("Device Arrival: {0}", e.CaptureDevice.GetDeviceInfo().Name);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MainPage.deviceListItems.Add(new StoredDevice() { DeviceObj = e.CaptureDevice, DeviceName = e.CaptureDevice.GetDeviceInfo().Name });
            });

            MainPage.SelectedDeviceText = string.Format("Selected Device for Trigger Scan Button:\n{0}", e.CaptureDevice.GetDeviceInfo().Name);

            // Last device arrival is the new selected device
            MainPage.selectedDevice = e.CaptureDevice;
        }

        private void Capture_DecodedData(object sender, CaptureHelper.DecodedDataArgs e)
        {
            if (SktErrors.SKTSUCCESS(e.Result))
            {
                MainPage.DisplayText = string.Format("Decoded Data: {0}", e.DecodedData.DataToUTF8String);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (_socketCamViewController != null)
                {
                    _socketCamViewController.View.RemoveFromSuperview();
                }
            });
        }
        // --Device

        public void DeviceList_SelectedIndexChanged(Picker deviceList)
        {
            System.Diagnostics.Debug.Print("here 1");
            if (deviceList.SelectedIndex != -1)
            {
                MainPage.selectedDevice = MainPage.deviceListItems[deviceList.SelectedIndex].DeviceObj;
                MainPage.SelectedDeviceText = string.Format("Selected Device for Trigger Scan Button:\n{0}", MainPage.selectedDevice.GetDeviceInfo().Name);
            }
        }

        public void Button_TriggerScan(CaptureHelperDevice device)
        {
            device?.SetTriggerStartAsync().ContinueWith(result =>
            {
                // To use SocketCam on iOS get the returned object and use it as a View Controller
                var resultDictionary = (NSDictionary)result.Result.ResultObject;
                var resultType = (NSString)resultDictionary[NSObject.FromObject("SKTObjectType")];

                if (resultType == "SKTSocketCamViewControllerType")
                {
                    _socketCamViewController = (UIViewController)resultDictionary[NSObject.FromObject("SKTSocketCamViewController")];

                    if (_socketCamViewController != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            var currentViewController = Platform.GetCurrentUIViewController();
                            _socketCamViewController.View.Frame = new CoreGraphics.CGRect(_socketCamXPos, _socketCamYPos, _socketCamWidth, _socketCamHeight);
                            currentViewController.View.AddSubview(_socketCamViewController.View);
                        });
                    }
                }
            });
        }

        // (Android-iOS) Check if SocketCam is enabled to set the Switch
        public async void GetSocketCamStatusInit()
        {
            var getStatus = await capture.GetSocketCamStatusAsync();
            if (getStatus.Status != CaptureHelper.SocketCamStatus.NotSupported)
            {
                MainPage.IsSocketCamSwitchEnable = true;
            }

            switch (getStatus.Status)
            {
                case CaptureHelper.SocketCamStatus.Enable:
                    MainPage.IsSocketCamEnable = true;
                    break;

                case CaptureHelper.SocketCamStatus.Disable:
                    MainPage.IsSocketCamEnable = false;
                    break;

            }
        }

        // (Android-iOS) 
        public async void Switch_SocketCamStatus()
        {
            await capture.SetSocketCamStatusAsync(MainPage.IsSocketCamEnable ? CaptureHelper.SocketCamStatus.Enable : CaptureHelper.SocketCamStatus.Disable);
        }
    }
}