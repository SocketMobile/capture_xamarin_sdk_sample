using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;
using capture_xamarin_sdk_sample.Model;
using SocketMobile.Capture;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace capture_xamarin_sdk_sample
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public CaptureHelper capture = new CaptureHelper();

        public static ObservableCollection<StoredDevice> deviceListItems = new ObservableCollection<StoredDevice>();

        public static CaptureHelperDevice selectedDevice;

        string appId = "";
        string developerId = "";
        string appKey = "";

        private bool isFirstLaunch = true;

        private string _selectedDeviceText = $"Selected Device for Trigger Scan Button: ";
        public string SelectedDeviceText
        {
            get => _selectedDeviceText;
            set
            {
                if (_selectedDeviceText != value)
                {
                    _selectedDeviceText = value;
                    OnPropertyChanged(nameof(SelectedDeviceText));
                }
            }
        }

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

        private bool _isVisibleAndroid;
        public bool IsVisibleAndroid
        {
            get => _isVisibleAndroid;
            set
            {
                _isVisibleAndroid = value;
                OnPropertyChanged(nameof(IsVisibleAndroid));
            }
        }

        private bool _isSocketCamEnable;
        public bool IsSocketCamEnable
        {
            get => _isSocketCamEnable;
            set
            {
                _isSocketCamEnable = value;
                OnPropertyChanged(nameof(IsSocketCamEnable));
            }
        }

        private bool _isSocketCamSwitchEnable;
        public bool IsSocketCamSwitchEnable
        {
            get => _isSocketCamSwitchEnable;
            set
            {
                _isSocketCamSwitchEnable = value;
                OnPropertyChanged(nameof(IsSocketCamSwitchEnable));
            }
        }

        public MainPage()
        {
            InitializeComponent();

            switchLabel.BindingContext = this;
            selectedDeviceText.BindingContext = this;
            deviceList.BindingContext = this;
            deviceList.ItemsSource = deviceListItems;
            deviceArrivalRemoval.BindingContext = this;
            deviceDecodedData.BindingContext = this;
            socketCamSwitch.BindingContext = this;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    appId = "<User App ID>";
                    developerId = "<User Developer ID>";
                    appKey = "<User App Key>";
                    break;

                case Device.Android:
                    appId = "<User App ID>";
                    developerId = "<User Developer ID>";
                    appKey = "<User App Key>";
                    break;

                case Device.UWP:
                    appId = "<User App ID>";
                    developerId = "<User Developer ID>";
                    appKey = "<User App Key>";
                    break;

            }

            IsVisibleAndroid = Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS;

            if (Device.RuntimePlatform == Device.iOS)
            {
                DependencyService.Get<ISocketMobileCaptureInit>().MainPage = this;
                DependencyService.Get<ISocketMobileCaptureInit>().StartCaptureCore(appId, developerId, appKey);
            }
            else
            {
                // (Android only) use to check and start (if not running already) the Android Service
                //AndroidService().ContinueWith(res =>
                //{
                // Place Open() here and remove from below
                //});

                Open();
            }
        }

        public void Open()
        {
            capture.OpenAsync(appId, developerId, appKey)
                .ContinueWith(result =>
                {
                    System.Diagnostics.Debug.Print("Open Capture returns {0}", result.Result);
                    if (SktErrors.SKTSUCCESS(result.Result))
                    {
                        if (isFirstLaunch)
                        {
                            capture.DeviceArrival += Capture_DeviceArrival;
                            capture.DeviceRemoval += Capture_DeviceRemoval;
                            capture.DecodedData += Capture_DecodedData;

                            // Start CaptureExtension to gain access to SocketCam device
                            if (Device.RuntimePlatform == Device.Android)
                            {
                                DependencyService.Get<IAndroidCaptureExtensionInit>().CallAndroidCaptureExtensionInit(capture.GetHandle());
                            }
                            else if (Device.RuntimePlatform == Device.UWP)
                            {
                                DependencyService.Get<IWindowsCaptureExtension>().CallWindowsCaptureExtensionInit(capture.GetHandle(), appId, developerId, appKey);
                            }

                            // (Android-iOS) Check if SocketCam is enabled to set the Switch
                            GetSocketCamStatusInit();
                        }
                    }
                });
        }

        // (Android only) re-enable communication with the Service after comming back from deep sleep mode
        // Not compatible with SocketCam
        public void ReEnableConnection()
        {
            // List will be repopulated on OpenAsync()
            deviceListItems.Clear();

            capture.CloseAsync().ContinueWith(result =>
            {
                if (SktErrors.SKTSUCCESS(result.Result))
                {
                    isFirstLaunch = false;
                    Open();
                }
            });
        }

        // (Android only) Check if Android Service is installed and running. If it is not running then starts the Service
        private async Task AndroidService()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                switch (capture.IsAndroidServiceInstalled())
                {
                    case true:
                        if (capture.IsAndroidServiceRunning() == false) capture.StartAndroidService();
                        break;

                    case false:
                        // Make sure that Companion is installed 
                        break;

                }
            }
            // Add time to let the Service start
            await Task.Delay(1000);
        }

        // Device Events--
        private void Capture_DeviceRemoval(object sender, CaptureHelper.DeviceArgs e)
        {
            DeviceEventText = string.Format("Device Removal: {0}", e.CaptureDevice.GetDeviceInfo().Name);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    int i = 0;
                    foreach (var device in deviceListItems)
                    {
                        if (device.DeviceName == e.CaptureDevice.GetDeviceInfo().Name)
                        {
                            deviceListItems.RemoveAt(i);
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

            SelectedDeviceText = "Selected Device: ";
        }

        private void Capture_DeviceArrival(object sender, CaptureHelper.DeviceArgs e)
        {
            DeviceEventText = string.Format("Device Arrival: {0}", e.CaptureDevice.GetDeviceInfo().Name);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                deviceListItems.Add(new StoredDevice() { DeviceObj = e.CaptureDevice, DeviceName = e.CaptureDevice.GetDeviceInfo().Name });
            });

            SelectedDeviceText = string.Format("Selected Device for Trigger Scan Button:\n{0}", e.CaptureDevice.GetDeviceInfo().Name);

            // Last device arrival is the new selected device
            selectedDevice = e.CaptureDevice;
        }

        private void Capture_DecodedData(object sender, CaptureHelper.DecodedDataArgs e)
        {
            DisplayText = string.Format("Decoded Data: {0}", e.DecodedData.DataToUTF8String);
        }
        // --Device

        private void DeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (deviceList.SelectedIndex != -1 && Device.RuntimePlatform == Device.iOS)
            {
                DependencyService.Get<ISocketMobileCaptureInit>().DeviceList_SelectedIndexChanged(deviceList);
            }

            // Uncomment below if using Uwp or Android (Comment if using iOS)
            //if (deviceList.SelectedIndex != -1)
            //{
            //    selectedDevice = deviceListItems[deviceList.SelectedIndex].DeviceObj;
            //    SelectedDeviceText = string.Format("Selected Device for Trigger Scan Button:\n{0}", selectedDevice.GetDeviceInfo().Name);
            //}
        }

        private void DeviceList_Focused(object sender, FocusEventArgs e)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                DependencyService.Get<ISocketMobileCaptureInit>().DeviceList_Focused(deviceList);
            }

            // Uncomment below if using Uwp or Android (Comment if using iOS)
            //int i = 0;
            //int index = -1;

            //foreach (var item in deviceListItems)
            //{
            //    if (item.DeviceName == selectedDevice.GetDeviceInfo().Name)
            //    {
            //        index = i;
            //        break;
            //    }
                
            //    i++;
            //}

            //deviceList.SelectedIndex = index;
        }

        // (iOS only)
        private void DeviceList_Unfocused(object sender, FocusEventArgs e)
        {
            deviceList.SelectedIndex = -1;
        }

        private void Button_TriggerScan(object sender, EventArgs e)
        {
            selectedDevice?.SetTriggerStartAsync();
        }

        // (Android-iOS) Check if SocketCam is enabled to set the Switch
        private async void GetSocketCamStatusInit()
        {
            var getStatus = await capture.GetSocketCamStatusAsync();
            if (getStatus.Status != CaptureHelper.SocketCamStatus.NotSupported)
            {
                IsSocketCamSwitchEnable = true;
            }

            switch (getStatus.Status) 
            {
                case CaptureHelper.SocketCamStatus.Enable:
                    IsSocketCamEnable = true;
                    break;

                case CaptureHelper.SocketCamStatus.Disable:
                    IsSocketCamEnable = false;
                    break;

            }
        }

        // (Android-iOS) 
        private async void Switch_SocketCamStatus(object sender, ToggledEventArgs e)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                DependencyService.Get<ISocketMobileCaptureInit>().Switch_SocketCamStatus();
            }
            else
            {
                await capture.SetSocketCamStatusAsync(_isSocketCamEnable ? CaptureHelper.SocketCamStatus.Enable : CaptureHelper.SocketCamStatus.Disable);
            }
        }
    }
}
