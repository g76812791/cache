using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilCache
{
    public class CacheProvider : IVigilCache
    {
        IVigilCache cache = null;
        public CacheProvider(VigilCache.CacheType cacheType)
        {
            switch (cacheType)
            {
                case CacheType.BlobStorageProvider:
                    cache = new BlobStorageProvider();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public CacheProvider()
            : this(new BlobStorageProvider())
        {

        }

        public CacheProvider(IVigilCache vigilCache)
        {
            cache = vigilCache;
        }

        public void SetOptions(CacheLocation cacheLocation)
        {
            cache.SetOptions(cacheLocation);
        }

        public bool Set<T>(Areas area, string field, string key, T value)
        {
            return cache.Set<T>(area, field, key, value);
        }

        public bool Set<T>(Areas area, string field, string key, T value, TimeSpan expiration)
        {
            return cache.Set<T>(area, field, key, value, expiration);
        }

        public bool Set(Areas area, string field, string key, string value)
        {
            return cache.Set(area, field, key, value);
        }

        public bool Set(Areas area, string field, string key, string value, TimeSpan expiration)
        {
            return cache.Set(area, field, key, value, expiration);
        }

        public T Get<T>(Areas area, string field, string key)
        {
            return cache.Get<T>(area, field, key);
        }

        public string Get(Areas area, string field, string key)
        {
            return cache.Get(area, field, key);
        }

        public void Delete(Areas area, string field, string key)
        {
            cache.Delete(area, field, key);
        }

        public IEnumerable<string> ListKeys(Areas area, string field, string key)
        {
            return cache.ListKeys(area, field, key);
        }

        public void Cleanup(Areas area, DateTime cutoffDate)
        {
            cache.Cleanup(area, cutoffDate);
        }

        public bool CleanupForKPIReport(Areas area, string field, string customerNumber, DateTime start, DateTime end)
        {
            return cache.CleanupForKPIReport(area, field, customerNumber, start, end);
        }

        public bool CleanupForKPIReportByUser(Areas area, string field, string customerNumber, string userId)
        {
            return cache.CleanupForKPIReportByUser(area, field, customerNumber, userId);
        }
    }
}
