using NUnit.Framework;
using XFactory.SqdcWatcher.Core.Utils;

namespace SqdcWatcher.Core.Tests.Utils
{
    public class NumberExtensionsTests
    {
        [Test]
        public void EqualsWithBothZero_ReturnsTrue()
        {
            bool areEqual = 0d.Equals(0, 1);
            
            Assert.IsTrue(areEqual);
        }

        [Test]
        public void DefaultTolerance_TwoDigitsEquivalent_ReturnsTrue()
        {
            bool areEqual = 0.123.Equals(0.124, 2);
            
            Assert.IsTrue(areEqual);
        }

        [Test]
        public void DefaultTolerance_TwoDigitsNonEquivalent_ReturnsFalse()
        {
            bool areEqual = 0.12.Equals(0.13);
            
            Assert.IsFalse(areEqual);
        }
    }
}