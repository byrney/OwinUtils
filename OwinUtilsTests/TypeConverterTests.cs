using System;
using NUnit.Framework;
using System.ComponentModel;
using OwinUtils;

namespace OwinUtilsTests
{

    [TypeConverter(typeof(ConstructorTypeConverter<XConvert>))]
    public class XConvert
    {
        public string val {
            get;
            set;
        }

        public XConvert(string v)
        {
            this.val = v;
        }

    }

    public class TypeConverterTests
    {
      [Test]
        public void CanConvertToClassWithConverter()
        {
            string v = "one two three";
            var cv = TypeDescriptor.GetConverter(typeof(XConvert));
            var x = (XConvert)cv.ConvertFrom(v);
            Assert.AreEqual(v, x.val);
        }

        [Test]
        public void CanConvertToInt()
        {
            string v = "99";
            var cv = TypeDescriptor.GetConverter(typeof(int));
            var x = (int)cv.ConvertFrom(v);
            Assert.AreEqual(v, x.ToString());
        }

        [Test]
        public void CanConvertToDate()
        {
            string v = "2014-11-13";
            var cv = TypeDescriptor.GetConverter(typeof(DateTime));
            var x = (DateTime)cv.ConvertFrom(v);
            Assert.AreEqual(DateTime.Parse(v), x);
        }

    }
}

