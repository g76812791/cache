using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilCache
{
    public interface IVigilCache
    {
        void SetOptions(CacheLocation cacheLocation);

        bool Set<T>(Areas area, string field, string key, T value);

        bool Set<T>(Areas area, string field, string key, T value, TimeSpan expiration);

        bool Set(Areas area, string field, string key, string value);

        bool Set(Areas area, string field, string key, string value, TimeSpan expiration);

        T Get<T>(Areas area, string field, string key);

        string Get(Areas area, string field, string key);

        void Delete(Areas area, string field, string key);

        IEnumerable<string> ListKeys(Areas area, string field, string key);

        void Cleanup(Areas area, DateTime cutoffDate);

        bool CleanupForKPIReport(Areas area, string field, string customerNumber, DateTime start, DateTime end);

        bool CleanupForKPIReportByUser(Areas area, string field, string customerNumber, string userId);
    }
}
