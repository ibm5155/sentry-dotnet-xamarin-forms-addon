using System;
using System.Collections.Generic;
using System.Text;

namespace SentryNetXamarinAddon.Services.Interface
{
    public interface ISentryXamarinHelper
    {
        string GetOsName();
        string GetDeviceInfo();
        string GetDeviceModel();
        string GetDeviceManufacture();
        long? GetRamAmount();
        long? GetAvaliableSpace();
        string HasCamera();
        bool? IsEmulator();
    }
}
