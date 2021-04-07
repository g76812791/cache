using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilCache.UnitTest
{
    [TestClass]
    public class RedisCacheProviderTests
    {
        IVigilCache cache = null;

        [TestInitialize]
        public void Initialize()
        {
            cache = new CacheProvider(CacheType.RedisCacheProvider);
        }

        [TestMethod]
        public void NotExistsTest()
        {
            var empty = cache.Get<ComplexType>(Areas.General, "bad^key", "nothing");
            Assert.IsNull(empty);
        }

        [TestMethod]
        public void WriteComplexTypeTest()
        {
            List<ComplexType> tt = new List<ComplexType>();
            for (var i = 0; i < 1000000; i++)
            {
                ComplexType t = new ComplexType()
                {
                    Date = DateTime.Now,
                    Value = 489.9+i,
                    Titles = new List<string>()
                {
                    "President",
                    "Operations"
                }
                };
                tt.Add(t);
            }
            var timer = new Stopwatch();
            timer.Start();
            var success = cache.Set<List<ComplexType>>(Areas.General, "test", "1", tt, new TimeSpan(0, 2, 0));
            timer.Stop();
            var ts = timer.Elapsed.TotalSeconds;
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void GetComplexTypeTest()
        {
            var timer = new Stopwatch();
            timer.Start();
            var success = cache.Get<List<ComplexType>>(Areas.General, "test", "1");
            timer.Stop();
            var ts = timer.Elapsed.TotalSeconds;
            Assert.IsTrue(success!=null);
        }

        [TestMethod]
        public void WriteSimpleTypeTest()
        {
            var success = cache.Set(Areas.General, "test", "2", "some value");

            Assert.IsTrue(success);
        }

        [TestMethod]
        public void ExpirationTest()
        {
            cache.Set(Areas.General, "vigil", "trends", "expires soon", TimeSpan.FromSeconds(-1));

            System.Threading.Thread.Sleep(2000);

            string expired = cache.Get(Areas.General, "vigil", "trends");

            Assert.IsNull(expired);
        }

        [TestMethod]
        public void DeletionTest()
        {
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

            var success = cache.Set<ComplexType>(Areas.General, "test", "3", t);

            cache.Delete(Areas.General, "test", "3");

            var gone = cache.Get<ComplexType>(Areas.General, "test", "3");

            Assert.IsNull(gone);
        }

        [TestMethod]
        public void CleanupTest()
        {
            cache.Cleanup(Areas.ExceptionQuery, DateTime.Now.AddMonths(-3));
        }

        [TestMethod]
        public void CleanupForKPIReportTest()
        {
            cache.CleanupForKPIReport(Areas.KPIReport, "FullReport", "1002", new DateTime(2019, 10, 8), new DateTime(2019, 10, 8));
        }
    }

}
