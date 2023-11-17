using SocketMobile.Capture;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace capture_xamarin_sdk_sample
{
    public interface ISocketMobileCaptureInit
    {
        MainPage MainPage { get; set; }
        void StartCaptureCore(string appId, string developerId, string appKey);

        void DeviceList_SelectedIndexChanged(Picker deviceList);
        void DeviceList_Focused(Picker deviceList);
        void GetSocketCamStatusInit();
        void Switch_SocketCamStatus();
    }
}
