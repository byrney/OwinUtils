using System;
using System.Reflection;
using System.Collections.Generic;

namespace OwinUtils
{
    public class RequestObject<T>
    {
        static PropertyInfo[] _props;
        static Dictionary<string, PropertyInfo> _propMap;
        static RequestObject()
        {
            Type t = typeof(T);
            var props = t.GetProperties();
            _propMap = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
            for(var i = 0; i < props.Length; i++)
            {
                _propMap[props[i].Name] = props[i];
            }

        }

        public string GetProperty(string propName)
        {
            PropertyInfo pi;
            if(_propMap.TryGetValue(propName, out pi)) {
                return pi.GetValue(this).ToString();
            }
            return null;
        }
    }

    public class HasProps : RequestObject<HasProps>
    {
        public string banana { get ; set ;}
        public int lemon {get;set;}
    }

}

