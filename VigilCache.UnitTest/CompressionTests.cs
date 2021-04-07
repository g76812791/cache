using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VigilCache.UnitTest
{
    [TestClass]
    public class CompressionTests
    {
        [TestMethod]
        public void Base64InvalidTest()
        {
            string json = "{\"ExpirationDate\":\"2016-03-29T11:21:08.3279004Z\",\"Value\":\"31\"}";
            bool isBase64 = Compression.IsBase64String(json);

            Assert.IsFalse(isBase64);
        }
    }
}
