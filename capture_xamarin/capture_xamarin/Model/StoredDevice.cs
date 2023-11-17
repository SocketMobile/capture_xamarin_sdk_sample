using System;
using System.Collections.Generic;
using System.Text;
using SocketMobile.Capture;

namespace capture_xamarin_sdk_sample.Model
{
    public class StoredDevice
    {
        public CaptureHelperDevice DeviceObj {  get; set; }
        public string DeviceName { get; set; }
    }
}
