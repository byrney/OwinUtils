using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OwinUtils;
using RouteDict = System.Collections.Generic.Dictionary<string, object>;

namespace OwinUtilsTests
{
    class RouteTemplateTests
    {
        [Test]
        public void SlashMatchesSlash()
        {
            var rt = new RouteTemplate("/", false);
            var rd = new RouteDict();
            var md = rt.match("/", rd);
            Assert.IsNotNull(md);
            Assert.AreEqual("", md.pathRemaining);
            Assert.AreEqual("/", md.pathMatched);
        }

        [Test]
        public void LiteralMatches()
        {
            var rt = new RouteTemplate("/branch", true);
            var rd = new RouteDict();
            var md = rt.match("/branch", rd);
            Assert.IsNotNull(md);
            Assert.AreEqual("", md.pathRemaining);
            Assert.AreEqual("/branch", md.pathMatched);
        }

        [Test]
        public void NonPartialsWithOptionalDontMatchLongerUrls()
        {
            var rt = new RouteTemplate("/branch", false);
            var rd = new RouteDict();
            var md = rt.match("/branch/[excess]", rd);
            Assert.IsNull(md);
        }

        [Test]
        public void NonPartialsDontMatchLongerUrls()
        {
            var rt = new RouteTemplate("/branch", false);
            var rd = new RouteDict();
            var md = rt.match("/branch/excess", rd);
            Assert.IsNull(md);
        }


        [Test]
        public void PartialMatches()
        {
            var rt = new RouteTemplate("/branch", true);
            var rd = new RouteDict();
            var md = rt.match("/branch/value", rd);
            Assert.IsNotNull(md);
            Assert.AreEqual("/value", md.pathRemaining);
            Assert.AreEqual("/branch", md.pathMatched);
        }

        [Test]
        public void EmptyOnlyMatchesEmpty()
        {
            var rt = new RouteTemplate("", false);
            var rd = new RouteDict();
            var md = rt.match("/branch", rd);
            Assert.IsNull(md);
        }

        [Test]
        public void TrailingSlashesArePreserved()
        {
            var rt = new RouteTemplate("/branch", true);
            var rd = new RouteDict();
            var md = rt.match("/branch/", rd);
            Assert.IsNotNull(md);
            Assert.AreEqual("/", md.pathRemaining);
            Assert.AreEqual("/branch", md.pathMatched);
        }

        [Test]
        [TestCase("/branch/<value>")]
        [TestCase("/branch/[value]")]
        public void VarsAreExtractedWithTrailingSlash(string template)
        {
            var rt = new RouteTemplate(template, true);
            var rd = new RouteDict();
            var md = rt.match("/branch/abcdefg/", rd);
            Assert.IsNotNull(md);
            Assert.AreEqual("/", md.pathRemaining);
            Assert.AreEqual("/branch/abcdefg", md.pathMatched);
            Assert.AreEqual("abcdefg", rd["value"]);
        }

        [Test]
        [TestCase("/branch/<value>")]
        [TestCase("/branch/[value]")]
        public void RequiredVarsAreExtracted(string template)
        {
            var rt = new RouteTemplate(template, false);
            var rd = new RouteDict();
            var md = rt.match("/branch/abcdefg", rd);
            Assert.IsNotNull(md);
            Assert.AreEqual("", md.pathRemaining);
            Assert.AreEqual("/branch/abcdefg", md.pathMatched);
            Assert.AreEqual("abcdefg", rd["value"]);
        }

  
    }
}
