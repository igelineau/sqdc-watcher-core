using System;
using NUnit.Framework;
using XFactory.SqdcWatcher.Data.Entities.Markets;

namespace SqdcWatcher.Domain.Tests.MarketsTest
{
    [TestFixture]
    public class MarketTest
    {
        private const string MarketName = "Sqdc";
        private const string BaseUrl = "https://sqdc.ca";

        [Test]
        public void GiveNullName_MarketCreationShouldFail()
        {
             TestDelegate act = () => _ = new Market(null, BaseUrl);
             
            Assert.Throws<Exception>(act);
        }
    }
}