using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VigilCache.Models;

namespace VigilCache
{

    public class RedisCacheProvider : IVigilCache
    {

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = "general.redis.cache.windows.net:6380,password=ZbI3e7sGcybqexQpQuTWDuvyOufO79CsoMY2rjwBTic=,ssl=True,ConnectTimeout=60000,connectRetry=5,syncTimeout=60000,abortConnect=False";
            return ConnectionMultiplexer.Connect(cacheConnection);
        });

        private static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        public void Cleanup(Areas area, DateTime cutoffDate)
        {
            return;
        }

        public bool CleanupForKPIReport(Areas area, string field, string customerNumber, DateTime start, DateTime end)
        {
            return false;
        }

        public void Delete(Areas area, string field, string key)
        {
            IDatabase cache = lazyConnection.Value.GetDatabase();
            var redisKey = string.Format("{0}^{1}^{2}", area.ToString(), field, key);
            if (cache.KeyExists(redisKey))
            {
                cache.KeyDelete(redisKey);
            }
        }

        #region Public Getters

        public T Get<T>(Areas area, string field, string key)
        {
            return _Get<T>(area, field, key);
        }

        public string Get(Areas area, string field, string key)
        {
            return _Get(area, field, key);
        }

        #endregion

        #region Private Getters

        private string _Get(Areas area, string field, string key)
        {
            key = EncodeBlobName(key);
            var redisKey = string.Format("{0}^{1}^{2}", area.ToString(), field, key);
            IDatabase cache = lazyConnection.Value.GetDatabase();
            if (cache.KeyExists(redisKey))
            {
                var tmp = cache.StringGet(redisKey);
                VigilCacheData vcd = VigilCacheData.FromString(tmp);

                if (vcd != null && vcd.ExpirationDate < DateTime.UtcNow)
                {
                    cache.KeyDelete(redisKey);
                    return null;
                }

                string result = null;
                if (Compression.IsBase64String(tmp))
                    result = Compression.Decompress(vcd.Value);
                else
                    result = vcd.Value;
                return result;
            }
          
            return null;
        }

        private T _Get<T>(Areas area, string field, string key)
        {
            key = EncodeBlobName(key);
            var redisKey = string.Format("{0}^{1}^{2}", area.ToString(), field, key);
            IDatabase cache = lazyConnection.Value.GetDatabase();
            string json = "";
            if (cache.KeyExists(redisKey))
            {
                var tmp = cache.StringGet(redisKey);
                VigilCacheData vcd = VigilCacheData.FromString(tmp);

                if (vcd == null || vcd.ExpirationDate < DateTime.UtcNow)
                {
                    cache.KeyDelete(redisKey);
                    return default(T); ;
                }

                if (Compression.IsBase64String(vcd.Value))
                    json = Compression.Decompress(vcd.Value);
                else
                    json = vcd.Value;
            }

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Could not deserialize cache object for key {0}. {1}", key, ex.Message);
                }
            }

            return default(T);
        }

        #endregion

        public IEnumerable<string> ListKeys(Areas area, string field, string key)
        {
            return null;
        }

        private string EncodeBlobName(string key)
        {
            return key.Replace("/", "|");
        }

        #region public setters
        public bool Set<T>(Areas area, string field, string key, T value)
        {
            return _Set<T>(area, field, key, value);
        }

        public bool Set<T>(Areas area, string field, string key, T value, TimeSpan expiration)
        {
            return _Set<T>(area, field, key, value, expiration);
        }

        public bool Set(Areas area, string field, string key, string value)
        {
            return _Set(area, field, key, value);
        }

        public bool Set(Areas area, string field, string key, string value, TimeSpan expiration)
        {
            return _Set(area, field, key, value, expiration);
        }
        #endregion

        #region private setters

        private bool _Set<T>(Areas area, string field, string key, T value)
        {
            return _Set<T>(area, field, key, value, TimeSpan.MaxValue);
        }

        private bool _Set<T>(Areas area, string field, string key, T value, TimeSpan expiration)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;// ignore loop handling
                return _Set(area, field, key, JsonConvert.SerializeObject(value, settings), expiration);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not serialize cache object for key {0}. {1}", key, ex.Message);
                return false;
            }
        }

        private bool _Set(Areas area, string field, string key, string value)
        {
            return _Set(area, field, key, value, TimeSpan.MaxValue);
        }

        private bool _Set(Areas area, string field, string key, string value, TimeSpan expiration)
        {
            key = EncodeBlobName(key);

            string _value = null;

            /* if the string is over 250K in length (approximately 500K in size) then compress it for performance */
            if (value.Length > 1024 * 250)
                _value = Compression.Compress(value);
            else
                _value = value;

            var redisKey = string.Format("{0}^{1}^{2}", area.ToString(), field, key);
            IDatabase cache = lazyConnection.Value.GetDatabase();

            VigilCacheData data = new VigilCacheData(_value, expiration);
            var serializedData = JsonConvert.SerializeObject(data);
            cache.StringSet(redisKey, serializedData, expiration);
            return true;
        }

        public void SetOptions(CacheLocation cacheLocation)
        {
            return;
        }

        #endregion
    }
}
