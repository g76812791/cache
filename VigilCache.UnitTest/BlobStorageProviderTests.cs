using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace VigilCache.UnitTest
{
    [TestClass]
    public class BlobStorageProviderTests
    {
        IVigilCache cache = null;

        [TestInitialize]
        public void Initialize()
        {
            cache = new CacheProvider(CacheType.BlobStorageProvider);
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

            var success = cache.Set<ComplexType>(Areas.General, "test", "1", t);

            Assert.IsTrue(success);
        }

        [TestMethod]
        public void WriteSimpleTypeTest()
        {
            var success = cache.Set(Areas.General, "test/123", "2", "some value");

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
            cache.CleanupForKPIReport(Areas.KPIReport, "FullReport", "1002", new DateTime(2020, 04, 13), new DateTime(2020, 04, 13));

        }

        [TestMethod]
        public void CleanupForKPIReportByUserTest()
        {
            cache.CleanupForKPIReportByUser(Areas.KPIReport, "MainReport", "1002", "6ab40d8a-0ea7-494a-b5f9-c3fd9ae1fe1e");
        }
    }

    internal class ComplexType
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public List<string> Titles { get; set; }
    }
}
