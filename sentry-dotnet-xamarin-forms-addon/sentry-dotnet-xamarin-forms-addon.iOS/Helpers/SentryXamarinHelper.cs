using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using SentryNetXamarinAddon.Services.Interface;
using UIKit;

namespace sentry_dotnet_xamarin_forms_addon.iOS.Helpers
{
    public class SentryXamarinHelper : ISentryXamarinHelper
    {
        public long? GetAvaliableSpace()
        {
            var statfs = NSFileManager.DefaultManager.GetFileSystemAttributes(Environment.GetFolderPath(Environment.SpecialFolder.Personal)).FreeSize;
            return (long)statfs;
        }

        public string GetDeviceInfo()
        {
            return NSProcessInfo.ProcessInfo.OperatingSystemVersionString;
        }

        public string GetDeviceManufacture()
        {
            return "Apple";
        }

        public string GetDeviceModel()
        {
            return IOSModel.GetModel();
        }

        public string GetOsName()
        {
            return "IOS";
        }

        public long? GetRamAmount()
        {
            ulong ram = NSProcessInfo.ProcessInfo.PhysicalMemory;

            return (long)ram;
        }

        public string HasCamera()
        {
            try
            {
                return (!IsEmulator()).ToString();//iOS simulator has no camera

            }
            catch
            {
                return null;
            }
        }

        public bool? IsEmulator()
        {
            return (ObjCRuntime.Runtime.Arch != ObjCRuntime.Arch.DEVICE);
        }

    }
}