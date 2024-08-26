using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace capture_xamarin_sdk_sample.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public static Window appContext;
        public MainPage()
        {
            this.InitializeComponent();

            appContext = Window.Current;

            LoadApplication(new capture_xamarin_sdk_sample.App());

        }

        public async static void DisplayUserControlView(UserControl userControl)
        {
            await appContext.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Handle closing the UI element:
                // Close button will trigger Capture's DecodedData event with a result of SktErrors.ESKT_CANCEL
                Popup pop = new Popup();
                userControl.Width = 250;
                userControl.Height = 250;
                pop.Child = userControl;
                pop.IsOpen = true;
            });
        }
    }


}
