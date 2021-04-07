using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using VigilCache;

namespace VigilCache.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            IVigilCache cache = new CacheProvider(CacheType.BlobStorageProvider);

            var ddd= cache.Get<List<KPIReport>>(Areas.KPIReport, "DetailReport", "1002|20191208|20191215|Weighted Exceptions|1|0412c9ee-a3af-40b7-85f8-74f8e161f519|True|False|0");
            var empty = cache.Get<ComplexType>(Areas.General, "bad^key", "boy");

            cache.Set(Areas.General, "dan", "dan", "expires soon", TimeSpan.FromSeconds(-1));

            string expired = cache.Get(Areas.General, "dan", "dan");

            ComplexType t = new ComplexType()
            {
                Date = DateTime.Now,
                Value = 489.9,
                Titles = new List<string>() 
                {
                    "President",
                    "Operations"
                }
            };

            cache.Set<ComplexType>(Areas.General, "test", "1", t);

            System.Threading.Thread.Sleep(1000);

            var x = cache.Get<ComplexType>(Areas.General, "test", "1");
            System.Diagnostics.Debug.Assert(x != null);
            Console.WriteLine(x.Value);

            //cache.Delete(Areas.General, "test", "1");

            Console.Read();
        }

        private class ComplexType
        {
            public DateTime Date { get; set; }
            public double Value { get; set; }
            public List<string> Titles { get; set; }
        }
    }
}
