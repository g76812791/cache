using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilCache.Models
{
    public class VigilCacheData
    {
        public VigilCacheData()
        {

        }

        public VigilCacheData(string value)
        {
            this.Value = value;
            this.ExpirationDate = DateTime.MaxValue;
        }

        public VigilCacheData(string value, TimeSpan expiration)
        {
            this.Value = value;
            this.ExpirationDate = expiration == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow.Add(expiration);
        }

        public DateTime ExpirationDate
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public static VigilCacheData FromString(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<VigilCacheData>(json);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not deserialize VigilCacheData object from {0}. {1}", json, ex.Message);
                return null;
            }
        }
    }
}
