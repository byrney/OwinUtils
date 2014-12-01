﻿using System;
using NUnit.Framework;
using System.IO;
using OwinUtils;
using System.Text;

namespace OwinUtilsTests
{
    public class AsyncWriterTests
    {
        [Test]
        public void TestSingleWrite()
        {
            Func<Exception, bool> error = e => false;
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var aw = new AsyncWriter(sw, error);

            var aMessage = "A message";
            var expected = Encoding.UTF8.GetBytes(aMessage);
            aw.WriteAndFlushAsync(aMessage);
           // sw.Write(aMessage);
           // sw.Flush();
            aw.Flush().Wait();
            var result = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual(aMessage, result);
        }

        [Test]
        public void TestDoubleWrite()
        {
            Func<Exception, bool> error = e => false;
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var aw = new AsyncWriter(sw, error);

            var message1 = "A message";
            var message2 = "Another message";
            var expected = Encoding.UTF8.GetBytes(message1);
            aw.WriteAndFlushAsync(message1);
            aw.WriteAndFlushAsync(message2);
            aw.Flush().Wait();
            var result = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual(message1 + message2, result);
        }


        [Test]
        public void TestFailures()
        {
            Func<Exception, bool> error = e => false;
            var buffer = new byte[30];
            var ms = new MemoryStream(buffer, true);
            var sw = new StreamWriter(ms);
            var aw = new AsyncWriter(sw, error);

            var message1 = "0123456789ABCDEF";
            var message2 = "01234567890123456789";
            var expected = Encoding.UTF8.GetBytes(message1);
            aw.WriteAndFlushAsync(message1);
            aw.WriteAndFlushAsync(message2);
            aw.Flush().Wait();
            var result = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual(message1 + message2, result);
        }


    }
}

