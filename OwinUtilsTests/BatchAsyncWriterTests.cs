using NUnit.Framework;
using System;
using OwinUtils;
using System.IO;
using System.Text;

namespace OwinUtilsTests
{
    public class BatchAsyncWriterTests
    {
        bool NoExceptionExpected(Exception e)
        {
            Assert.Fail("Should not get to here. No exception expected but got {0}", e);
            return false;
        }

        [Test]
        public void CanConstruct()
        {
            var s = new MemoryStream();
            var bat = new BatchAsyncWriter(s, NoExceptionExpected);
        }

        [Test]
        public void ZeroBatchingWorks()
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var aw = new BatchAsyncWriter(ms, NoExceptionExpected, 0);
            var aMessage = "A message";
            aw.WriteAsync(aMessage);
            aw.Finished().Wait();
            Assert.AreEqual(0, aw.Remaining);
            aw.Flush();
            var result = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual(aMessage, result);
        }  

        [Test]
        public void MessageSizeBatchingWorks()
        {
            var ms = new MemoryStream();
            var aMessage = "A message";
            var batchSize = aMessage.Length;
            var aw = new BatchAsyncWriter(ms, NoExceptionExpected, batchSize);
            foreach(char c in aMessage) {
                aw.WriteAsync(c.ToString());
            }
            aw.Flush();
            aw.Finished().Wait();
            Assert.AreEqual(0, aw.Remaining);
            var result = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual(aMessage, result);
        }  

        [Test]
        public void LargeBatchSmallMessageWorks()
        {
            var ms = new MemoryStream();
            var aMessage = "A message";
            var batchSize = aMessage.Length * 10;
            var aw = new BatchAsyncWriter(ms, NoExceptionExpected, batchSize);
            foreach(char c in aMessage) {
                aw.WriteAsync(c.ToString());
            }
            Assert.AreEqual(aMessage.Length, aw.Remaining);
            aw.Finished().Wait();
            Assert.AreEqual(0, aw.Remaining);
            aw.Flush();
            var result = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual(aMessage, result);
        }  

        [Test]
        public void PartialBatchWorks()
        {
            var ms = new MemoryStream();
            var aMessage = "A message";
            var batchSize = aMessage.Length * -2;
            var aw = new BatchAsyncWriter(ms, NoExceptionExpected, batchSize);
            foreach(char c in aMessage) {
                aw.WriteAsync(c.ToString());
            }
            aw.Finished().Wait();
            Assert.AreEqual(0, aw.Remaining);
            aw.Flush();
            var result = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual(aMessage, result);
        }  



    }
}

