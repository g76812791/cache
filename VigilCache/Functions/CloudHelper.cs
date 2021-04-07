using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilCache.Functions
{
    internal class CloudHelper
    {
        public static CloudBlobClient GetCloudBlobClientReference(CacheLocation cacheLocation = CacheLocation.East)
        {
            var connString = string.Empty;
            if (CustomSettingsHelper.IsDevEnviroment()!= TrendsEnvironment.Prod)
            {
                connString = CustomSettingsHelper.GetStorageConnectionString(StorageKeys.cache);
            }
            else
            {
                connString = cacheLocation == CacheLocation.East
            ? CustomSettingsHelper.GetStorageConnectionString(StorageKeys.cache)
            : "DefaultEndpointsProtocol=https;AccountName=3xcache2;AccountKey=8cGuZ3JxUqG3EIKzHW5NqSnRQvMZ3XrN+fCN4F28K8t4GRt8u8BhXh9sIO+7sNCfpAk9Mts1fYkTWjcae2ewNw==;EndpointSuffix=core.windows.net";
            }
            var storageAccount = CloudStorageAccount.Parse(connString);
            var storageClient = storageAccount.CreateCloudBlobClient();
            return storageClient;
        }
    }
}
