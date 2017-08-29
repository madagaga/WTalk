using System;
using System.Collections;
using System.Collections.Generic;
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


        public DynamicJson(System.IO.Stream jsonStream)
        {
            var parse = JSON.Parse(jsonStream);
            initialize(parse);
        }

        public DynamicJson(string json)
        {
            var parse = JSON.Parse(json);
            initialize(parse);
        }

        private DynamicJson(object o)
        {       
            initialize(o);
        }


        private void initialize(object o)
        {
            if (o is IDictionary<string, object>)
                _dictionary = (IDictionary<string, object>)o;
            if (o is List<object>)
                _list = (List<object>)o;
            else
                _object = o;

        }

        DynamicJson createDynamicJson(object o)
        {
            if (o == null)
                return o as DynamicJson;
            else if (o is DynamicJson)
                return (DynamicJson)o;
            else
                return new DynamicJson(o);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {

            for(int i=0;i<_list.Count;i++)
            {
                _list[i] = createDynamicJson(_list[i]);
                yield return _list[i];
            }
            
        }

        public DynamicJson this[int index]
        {
            get
            {
                object o = null;                               
                if (_list != null && index < _list.Count)
                {
                    _list[index] = createDynamicJson(_list[index]);
                    o = _list[index];
                }
                    

                return o != null ? o as DynamicJson: null ;
            }
        }

        public DynamicJson this[string key] 
        {
            get
            {
                object o = null;
                if (_dictionary != null && _dictionary.ContainsKey(key))
                {
                    _dictionary[key] = createDynamicJson(_dictionary[key]);
                    o = _dictionary[key];
                }

                return o != null ? o as DynamicJson : null;

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
            return Value != null ? Value.ToString() : null;
        }

        public void RemoveAt(int index)
        {
            if (_list != null)
                _list.RemoveAt(index);
        }
    }
}
