using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using SentryNetXamarinAddon.Services.Interface;

namespace sentry_dotnet_xamarin_forms_addon.Droid.Helpers
{
    public class SentryXamarinHelper : ISentryXamarinHelper
    {
        public long? GetAvaliableSpace()
        {
            try
            {
                var statfs = new StatFs(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
                return statfs.AvailableBytes;
            }
            catch
            {
                return null;
            }
        }

        public string GetDeviceInfo()
        {
            try
            {
                return Build.VERSION.SdkInt.ToString();
            }
            catch
            {
                return null;
            }
        }

        public string GetDeviceManufacture()
        {
            try
            {
                return Build.Manufacturer;
            }
            catch
            {
                return null;
            }
        }

        public string GetDeviceModel()
        {
            try
            {
                string manufacturer = Build.Manufacturer;
                string model = Build.Model;
                if (model.ToLower().StartsWith(manufacturer.ToLower()))
                {
                    return capitalize(model);
                }
                else
                {
                    return capitalize(manufacturer) + " " + model;
                }
            }
            catch
            {
                return null;
            }
        }

        public string GetOsName()
        {
            return "ANDROID";
        }

        public long? GetRamAmount()
        {
            try
            {
                //Not the best way but its the way i found that worked
                Java.IO.RandomAccessFile reader = new Java.IO.RandomAccessFile("/proc/meminfo", "r");
                var memory = reader.ReadLine();
                reader.Close();
                memory = Regex.Match(memory, @"\d+").Value;
                return int.Parse(memory) * 1024; //convert KB to bytes
            }
            catch
            {
                return null;
            }
        }

        public string HasCamera()
        {
            var context = Application.Context;
            PackageManager pm = context.PackageManager;
            //Must have a targetSdk >= 9 defined in the AndroidManifest
            var hasCamera = pm.HasSystemFeature(PackageManager.FeatureCameraFront) || pm.HasSystemFeature(PackageManager.FeatureCamera);
            return hasCamera.ToString();
        }

        public bool? IsEmulator()
        {
            var isEmulator = Build.Fingerprint.StartsWith("generic")
                || Build.Fingerprint.StartsWith("unknown")
                || Build.Model.Contains("google_sdk")
                || Build.Model.Contains("Emulator")
                || Build.Model.Contains("Android SDK built for x86")
                || Build.Manufacturer.Contains("Genymotion")
                || (Build.Brand.StartsWith("generic") && Build.Device.StartsWith("generic"))
                || "google_sdk".Equals(Build.Product);
            return isEmulator;
        }

        private string capitalize(string s)
        {
            if (s == null || s.Length == 0)
            {
                return "";
            }
            char first = s[0];
            if (Character.IsUpperCase(first))
            {
                return s;
            }
            else
            {
                return Character.ToUpperCase(first) + s.Substring(1);
            }

        }
    }
}