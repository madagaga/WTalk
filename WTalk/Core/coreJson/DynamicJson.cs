using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coreJson
{
    public class DynamicJson : IEnumerable
    {
        private IDictionary<string, object> _dictionary;
        private List<object> _list;
        private object _object;

        public DynamicJson(string json)
        {
            var parse = JSON.Parse(json);

            if (parse is IDictionary<string, object>)
                _dictionary = (IDictionary<string, object>)parse;
            else
                _list = (List<object>)parse;
        }

        private DynamicJson(object o)
        {
            if (o is IDictionary<string, object>)
                _dictionary = (IDictionary<string, object>)o;
            if (o is List<object>)
                _list = (List<object>)o;
            else
                _object = o;

        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var o in _list)
            {
                yield return new DynamicJson(o);
            }
        }

        public DynamicJson this[int index]
        {
            get
            {
                object o = null;                               
                if (_list != null && index < _list.Count)
                    o = _list[index];

                return o != null ? new DynamicJson(o): null ;
            }
        }

        public DynamicJson this[string key] 
        {
            get
            {
                object o = null;
                if (_dictionary != null && _dictionary.ContainsKey(key))
                    o = _dictionary[key];

                return o != null ? new DynamicJson(o) : null;

            }
        }

        public int Count
        {
            get
            {
                if (_list != null)
                    return _list.Count;
                else if (_dictionary != null)
                    return _dictionary.Count;
                else return 0;

            }
        }

        public object Value { get { return _object; } }

        public override string ToString()
        {
            return Value?.ToString();
        }

        public void RemoveAt(int index)
        {
            if (_list != null)
                _list.RemoveAt(index);
        }
    }
}
