using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilCache
{
    public enum Areas
    {
        Empty,
        General,
        TrendsLocationQuery,
        TrendsGroupQuery,
        PosQuery,
        ExceptionQuery,
        KPIReport,
        UserAudit,
        TrendsNextWeb
    }

    public enum CacheType
    {
        BlobStorageProvider,
        RedisCacheProvider
    }

    public enum CacheLocation
    {
        East,
        SouthCentral
    }
}
