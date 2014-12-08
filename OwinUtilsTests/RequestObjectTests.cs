using System;
using OwinUtils;
using NUnit.Framework;

namespace OwinUtilsTests
{
    public class RequestObjectTests
    {
       [Test]
        public void TestHasProps()
        {
            var subject = new HasProps();
            var expected = "some stringish thing";
            subject.banana = expected;
            var result = subject.GetProperty("banana");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestCaseInsensitive()
        {
            var subject = new HasProps();
            var expected = "some stringish thing";
            subject.banana = expected;
            var result = subject.GetProperty("BANANA");
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestGetNullForMisingProperties()
        {
            var subject = new HasProps();
            var expected = "some stringish thing";
            subject.banana = expected;
            var result = subject.GetProperty("WaterMelon");
            Assert.IsNull(result);
        }

        [Test]
        public void ConvertsToString()
        {
            var subject = new HasProps();
            var expected = 5;
            subject.lemon = expected;
            var result = subject.GetProperty("Lemon");
            Assert.AreEqual(expected.ToString(), result);
        }

    }
}

