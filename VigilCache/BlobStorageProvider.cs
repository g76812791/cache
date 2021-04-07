using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VigilCache.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using VigilCache.Functions;
using System.Text.RegularExpressions;

namespace VigilCache
{
    internal class BlobStorageProvider : IVigilCache
    {
        private CacheLocation _cacheLocation = CacheLocation.East;

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
                return _Set(area, field, key, JsonConvert.SerializeObject(value,settings), expiration);
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

            var blob = GetBlobReference(area, field, key);

            VigilCacheData data = new VigilCacheData(_value, expiration);
            var serializedData = JsonConvert.SerializeObject(data);
            blob.UploadText(serializedData);

            return true;
        }

        #endregion

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

            var blob = GetBlobReference(area, field, key);
            if (!blob.Exists())
                return null;

            var tmp = blob.DownloadText();
            VigilCacheData vcd = VigilCacheData.FromString(tmp);

            if (vcd.ExpirationDate < DateTime.UtcNow)
            {
                blob.Delete();
                return null;
            }

            string result = null;
            if (Compression.IsBase64String(tmp))
                result = Compression.Decompress(vcd.Value);
            else
                result = vcd.Value;

            return result;
        }

        private T _Get<T>(Areas area, string field, string key)
        {
            key = EncodeBlobName(key);

            var blob = GetBlobReference(area, field, key);
            if (!blob.Exists())
                return default(T);

            string json = "";

            try
            {
                var tmp = blob.DownloadText();
                VigilCacheData vcd = VigilCacheData.FromString(tmp);

                if (vcd.ExpirationDate < DateTime.UtcNow)
                {
                    blob.Delete();
                    return default(T);
                }

                if (Compression.IsBase64String(vcd.Value))
                    json = Compression.Decompress(vcd.Value);
                else
                    json = vcd.Value;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not retrieve cache object for key {0}. {1}", key, ex.Message);
                return default(T);
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

        public void SetOptions(CacheLocation cacheLocation)
        {
            _cacheLocation = cacheLocation;
        }

        public void Delete(Areas area, string field, string key)
        {
            key = EncodeBlobName(key);

            var client = CloudHelper.GetCloudBlobClientReference(_cacheLocation);
            var outerContainer = client.GetContainerReference(area.ToString().ToLower());
            var innerContainer = outerContainer.GetDirectoryReference(field.ToLower());

            try
            {
                foreach (CloudBlockBlob blob in innerContainer.ListBlobs())
                {
                    var pieces = blob.Name.Split('/');
                    var name = pieces[pieces.Length - 1];
                    bool delete = false;
                    if (key.Contains('*'))
                    {
                        var wildcards = key.Split('*');
                        if (key.StartsWith("*")) //pattern is like *8643*
                        {
                            if (name.Contains(pieces[1]))
                                delete = true;
                        }
                        else //pattern is like 1001*7244
                        {
                            if (name.StartsWith(wildcards[0]) && name.EndsWith(wildcards[1]))
                                delete = true;
                        }
                    }
                    else if (name == key)
                        delete = true;

                    if (delete)
                        blob.Delete();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not delete from blob cache {0} {1} {2}. {3}", area.ToString(), field, key, ex.Message);
            }
        }

        public IEnumerable<string> ListKeys(Areas area, string field, string key)
        {
            return null;
            //var keys = FindKeys(area, field, key);

            //foreach (var k in keys)
            //{
            //    yield return k.ToString();
            //}
        }


        private CloudBlockBlob GetBlobReference(Areas area, string field, string key)
        {
            var client = CloudHelper.GetCloudBlobClientReference(_cacheLocation);
            var outerContainer = client.GetContainerReference(area.ToString().ToLower());
            outerContainer.CreateIfNotExists();
            var innerContainer = outerContainer.GetDirectoryReference(field.ToLower());
            var blob = innerContainer.GetBlockBlobReference(key);
            return blob;
        }

        private string EncodeBlobName(string key)
        {
            return key.Replace("/", "|");
        }


        public void Cleanup(Areas area, DateTime cutoffDate)
        {
            var client = CloudHelper.GetCloudBlobClientReference(_cacheLocation);
            var outerContainer = client.GetContainerReference(area.ToString().ToLower());
            foreach (CloudBlockBlob blob in outerContainer.ListBlobs(useFlatBlobListing: true))
            {
                if (blob.Properties.LastModified < cutoffDate)
                    blob.Delete();
            }
        }

        /// <summary>
        /// Clear the KPI query cache
        /// </summary>
        /// <param name="area"></param>
        /// <param name="field"></param>
        /// <param name="customerNumber"></param>
        /// <param name="date"></param>
        public bool CleanupForKPIReport(Areas area, string field, string customerNumber, DateTime start, DateTime end)
        {
            if (area != Areas.KPIReport)
            {
                return false;
            }
            try
            {
                var client = CloudHelper.GetCloudBlobClientReference(_cacheLocation);
                var outerContainer = client.GetContainerReference(area.ToString().ToLower());
                var innerContainer = outerContainer.GetDirectoryReference(field.ToLower());
                var prefix = string.Format("{0}/", field.ToLower());
                var customerPrefix = string.Format("/{0}", customerNumber);
                BlobContinuationToken continuationToken = null;
                do
                {
                    var iResults = innerContainer.ListBlobsSegmented(true, BlobListingDetails.None, 1000, continuationToken, null, null);
                    continuationToken = iResults.ContinuationToken;
                   var listBlobItems=iResults.Results.Where(p => p.GetType() == typeof(CloudBlockBlob));
                    foreach (var item in listBlobItems)
                    {
                        var blob = item as CloudBlockBlob;
                        if (blob == null)
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(blob.Name) || blob.Name.Length < prefix.Length)
                        {
                            continue;
                        }
                        var nameArr = blob.Name.Substring(prefix.Length, blob.Name.Length - prefix.Length).Split('|');
                        if (nameArr.Length < 4)
                        {
                            continue;
                        }

                        if ((nameArr[0].LastIndexOf(customerPrefix)== nameArr[0].Length- customerPrefix.Length)
                            &&
                            !(nameArr[2].CompareTo(start.ToString("yyyyMMdd")) < 0 ||
                            nameArr[1].CompareTo(end.ToString("yyyyMMdd")) > 0))
                        {
                            blob.Delete();
                        }
                    }
                }
                while (continuationToken != null);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("CleanupForKPIReport Error: {0}", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Clear the KPI query cache
        /// </summary>
        /// <param name="area"></param>
        /// <param name="field"></param>
        /// <param name="customerNumber"></param>
        /// <param name="date"></param>
        public bool CleanupForKPIReportByUser(Areas area, string field, string customerNumber,string userId)
        {
            if (area != Areas.KPIReport)
            {
                return false;
            }
            try
            {
                var client = CloudHelper.GetCloudBlobClientReference(_cacheLocation);
                var outerContainer = client.GetContainerReference(area.ToString().ToLower());
                var prefix = string.Format("{0}/{1}", field.ToLower(), userId.ToLower());
                var innerContainer = outerContainer.GetDirectoryReference(prefix);
                prefix += "/";

                   BlobContinuationToken continuationToken = null;
                do
                {
                    var iResults = innerContainer.ListBlobsSegmented(true, BlobListingDetails.None, 1000, continuationToken, null, null);
                    continuationToken = iResults.ContinuationToken;
                    var listBlobItems = iResults.Results.Where(p => p.GetType() == typeof(CloudBlockBlob));
                    foreach (var item in listBlobItems)
                    {
                        var blob = item as CloudBlockBlob;
                        if (blob == null)
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(blob.Name) || blob.Name.Length < prefix.Length)
                        {
                            continue;
                        }
                        var nameArr = blob.Name.Substring(prefix.Length, blob.Name.Length - prefix.Length).Split('|');
                        if (nameArr.Length < 4)
                        {
                            continue;
                        }

                        if (nameArr[0].Equals(customerNumber, StringComparison.Ordinal))
                        {
                            blob.Delete();
                        }
                    }
                }
                while (continuationToken != null);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("CleanupForKPIReport Error: {0}", ex.ToString());
                return false;
            }
        }
    }
}
