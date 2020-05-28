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
        public void CanCreateMarketWithNameAndBaseUrl()
        {
            var market = new Market(new MarketIdentity("Sqdc"), "https://sqdc.ca");
            Assert.AreEqual(MarketName, market.Identity.Name);
            Assert.AreEqual("https://sqdc.ca", market.BaseUrl);
        }

        [Test]
        public void GiveNullName_MarketCreationShouldFail()
        {
             TestDelegate act = () => _ = new Market(null, BaseUrl);
             
            Assert.Throws<Exception>(act);
        }

        [Ignore("coding")] [Test]
        public void GivenNullBaseUrl_CreationShouldFail()
        {
            TestDelegate act = () => _ = new Market(new MarketIdentity(MarketName), null);

            Assert.Throws<Exception>(act);
        }
    }
}