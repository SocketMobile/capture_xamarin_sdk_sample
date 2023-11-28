using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace capture_xamarin_sdk_sample
{
    public partial class App : Application
    {
        MainPage rootPage;
        public App()
        {
            InitializeComponent();

            rootPage = new MainPage();
            MainPage = rootPage;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                // (Android only) Re-enable communication with the Service after comming back from deep sleep mode
                rootPage.ReEnableConnection();
            }
        }
    }
}
