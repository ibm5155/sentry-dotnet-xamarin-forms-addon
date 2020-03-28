using SentryNetXamarinAddon.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SentryNetXamarinAddon.Services.Interface
{
    public interface IOfflineCacheHelper
    {
        List<string> GetFileNames();

        bool ClearLogCache();

        string SaveCache(CacheLog cache, string key);

        CacheLog GetCache(string fileName, string key);

        bool RemoveLogCache(string fileName);
    }
}
