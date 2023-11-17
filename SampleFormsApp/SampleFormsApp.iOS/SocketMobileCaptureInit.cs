using Foundation;
using SampleFormsApp.iOS;
using SampleFormsApp.Model;
using SocketMobile.Capture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(SocketMobileCaptureInit))]
namespace SampleFormsApp.iOS
{
    public class SocketMobileCaptureInit : ISocketMobileCaptureInit
    {
        public CaptureHelper capture = new CaptureHelper();
        public MainPage MainPage { get; set; }

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

            // Set SocketCam Overlay to display camera
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var getStatus = await capture.GetSocketCamStatusAsync();
                if (getStatus.Status == CaptureHelper.SocketCamStatus.Enable) await MainPage.selectedDevice.SetSocketCamOverlay();
            });
        }

        private void Capture_DecodedData(object sender, CaptureHelper.DecodedDataArgs e)
        {
            MainPage.DisplayText = string.Format("Decoded Data: {0}", e.DecodedData.DataToUTF8String);
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

        public void DeviceList_Focused(Picker deviceList)
        {
            int i = 0;
            int index = -1;

            foreach (var item in MainPage.deviceListItems)
            {
                if (item.DeviceName == MainPage.selectedDevice.GetDeviceInfo().Name)
                {
                    index = i;
                    break;
                }

                i++;
            }

            deviceList.SelectedIndex = index;
        }

        public void Button_TriggerScan(CaptureHelperDevice device)
        {
            device?.SetTriggerStartAsync();
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