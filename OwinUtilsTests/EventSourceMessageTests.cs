using NUnit.Framework;
using System.IO;
using System.Text;
using OwinUtils;

namespace OwinUtilsTests
{
    public class EventSourceMessageTests
    {
        [Test]
        public void ToStringBasicallyWorks()
        {
            string eventKey = "key";
            string eventId = "123455";
            string someData = "some data";
            string expected = string.Format("event:{0}\nid:{1}\ndata:{2}", eventKey, eventId, someData);
            var ev = new EventSourceMessage(eventKey, eventId, someData);
            string evs = ev.ToString();
            Assert.AreEqual(expected, evs);
        }

        [Test]
        public void ToStringWorksWithTrailingNewlines()
        {
            string eventKey = "key";
            string eventId = "123455";
            string someData = "some\ninfo";
            string expected = string.Format("event:{0}\nid:{1}\ndata:some\ndata:info", eventKey, eventId);
            var ev = new EventSourceMessage(eventKey, eventId, someData);
            string evs = ev.ToString();
            Assert.AreEqual(expected, evs);
        }
    
        [Test]
        public void CanParseOutput()
        {
            string eventKey = "key";
            string eventId = "123455";
            string someData = "some data";
            var ev = new EventSourceMessage(eventKey, eventId, someData);
            string evs = ev.ToString();
            var parsed = DoParse(evs);
            Assert.AreEqual(ev.EventKey, parsed.EventKey);
            Assert.AreEqual(ev.EventId, parsed.EventId);
            Assert.AreEqual(ev.Data, parsed.Data);
        }

        [Test]
        public void CanParseDataWithNewlines()
        {
            string eventKey = "key";
            string eventId = "123455";
            string someData = "some\ninformation";
            var ev = new EventSourceMessage(eventKey, eventId, someData);
            string evs = ev.ToString();
            var parsed = DoParse(evs);
            Assert.AreEqual(ev.EventKey, parsed.EventKey);
            Assert.AreEqual(ev.EventId, parsed.EventId);
            Assert.AreEqual(ev.Data, parsed.Data);
        }

        private static EventSourceMessage DoParse(string evs)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(evs));
            var reader = new StreamReader(ms);
            var parsed = EventSourceMessage.Parse(reader);
            return parsed;
        }
    }
}
