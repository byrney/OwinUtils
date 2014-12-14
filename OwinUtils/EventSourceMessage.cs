using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OwinUtils
{
    /// <summary>
    /// An HTML EventSource message
    /// </summary>
    public class EventSourceMessage
    {
        private const char _newline = '\n';

        public EventSourceMessage()
        {
        }

        public EventSourceMessage(string eventKey, string eventId, string data)
        {
            EventKey = eventKey;
            EventId = eventId;
            Data = data;
        }

        private static void OnNewLine(StringBuilder builder, string format, params object[] data)
        {
            if(builder.Length != 0) {
                builder.Append(_newline);
            }
            builder.AppendFormat(format, data);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            if(null != EventKey) {
                OnNewLine(result, "event:{0}", EventKey);
            }
            if(null != EventId) {
                OnNewLine(result, "id:{0}", EventId);
            }
            if(null != Data) {
                var r = new StringReader(Data);
                string d;
                int lines = 0;
                while((d = r.ReadLine()) != null) {
                    OnNewLine(result, "data:{0}", d);
                }
            }
            return result.ToString();
        }

        public static string ReadStream(StreamReader reader, int timeout)
        {
            var start = reader.ReadLineAsync();
            if (start.Wait(timeout)) {
                return start.Result;
            }
            throw new TimeoutException();
        }

        public static EventSourceMessage Parse(StreamReader reader, int timeout = 100)
        {
            EventSourceMessage result = null; //new HttpEventSourceMessage();
            var builder = new StringBuilder();
            string line;
            while ((line = ReadStream(reader, timeout)) != null) {
                if (line.Length == 0) {
                    if (result != null) {
                        result.Data = builder.ToString();
                        return result; //  empty line marks end of this event
                    } else {
                        continue;
                    }
                } else {
                    result = result ?? new EventSourceMessage();
                    var segments = line.Split(colon, 2, StringSplitOptions.None);
                    switch(segments[0]) {
                        case "event":
                            result.EventKey = segments[1];
                            break;
                        case "data":
                            OnNewLine(builder, "{0}", segments[1]);
                            break;
                        case "id":
                            result.EventId = segments[1];
                            break;
                        default:
                            throw new ParseException("Unexpected format");
                    }
                }
            }
            if(result != null)
                result.Data = builder.ToString();
            return result;
        }

        private static char[] colon = new[] { ':' };
        public string Data { get; set; }

        public string EventId { get; set; }

        public string EventKey { get; set; }
    }

    [Serializable]
    internal class ParseException : Exception
    {
        public ParseException(string unexpectedFormat)
        {
        }
    }
}
