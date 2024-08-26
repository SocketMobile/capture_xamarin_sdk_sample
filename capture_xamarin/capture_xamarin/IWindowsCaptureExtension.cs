using System;
using System.Collections.Generic;
using System.Text;

namespace capture_xamarin_sdk_sample
{
    public interface IWindowsCaptureExtension
    {
        void CallWindowsCaptureExtensionInit(int captureHandle, string appId, string developerId, string appKey);
    }
}
