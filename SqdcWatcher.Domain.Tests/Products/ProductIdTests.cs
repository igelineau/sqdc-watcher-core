using NUnit.Framework;
using XFactory.SqdcWatcher.Data.Entities.Products;

namespace SqdcWatcher.Domain.Tests.Products
{
    [TestFixture]
    public class ProductIdTests
    {
        [Test]
        public void GivenImplicitConversionToString_Succeeds()
        {
            const string value = "809582000503-P";

            var createdId = ProductId.Create(value); 
            string convertedValue = (string) createdId;
            
            Assert.AreEqual(value, convertedValue);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("809582000503-")]
        [TestCase("809582000503")]
        public void GivenInvalidProductId_ThrowsInvalidProductIdException(string creationValue)
        {
            Assert.Throws<InvalidProductIdException>(() => ProductId.Create(creationValue));
        }
    }
}