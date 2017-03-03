using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace coreJson
{
    public class JsonSerializer
    {
        System.Text.StringBuilder _output;

        internal string ConvertToJSON(object obj)
        {
            _output = new StringBuilder();
            writeValue(obj);
            return _output.ToString();
        }



        private void writeValue(object obj)
        {
            if (obj == null)
                _output.Append("null");

            else if (obj is string || obj is char)
                writeString(obj.ToString());

            else if (obj is Guid)
                writeGuid((Guid)obj);

            else if (obj is bool)
                _output.Append(((bool)obj) ? "true" : "false"); // conform to standard

            else if (
                obj is int || obj is long || obj is double ||
                obj is decimal || obj is float ||
                obj is byte || obj is short ||
                obj is sbyte || obj is ushort ||
                obj is uint || obj is ulong
            )
                //_output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
                _output.Append(string.Format(CultureInfo.InvariantCulture, "{0}", obj));

            else if (obj is DateTime)
                writeDateTime((DateTime)obj);

            else if (obj is IDictionary && obj.GetType().GetTypeInfo().IsGenericType && obj.GetType().GetTypeInfo().GenericTypeArguments.FirstOrDefault() == typeof(string))
                writeStringDictionary((IDictionary)obj);

            else if (obj is IDictionary)
                writeDictionary((IDictionary)obj);
            else if (obj is byte[])
                writeBytes((byte[])obj);

            else if (obj is Array || obj is IList || obj is ICollection)
                writeArray((IEnumerable)obj);

            else if (obj is Enum)
                writeEnum((Enum)obj);
            else
                writeObject(obj);
        }

        private void writeObject(object obj)
        {
            var result = objectToRaw(obj);
            writeValue(result);
        }

        private void writeEnum(Enum e)
        {
            writeString(e.ToString());
        }

        private void writeArray(IEnumerable array)
        {
            _output.Append('[');

            bool pendingSeperator = false;

            foreach (object obj in array)
            {
                if (pendingSeperator) _output.Append(',');

                writeValue(obj);

                pendingSeperator = true;
            }
            _output.Append(']');
        }

        private void writeBytes(byte[] bytes)
        {
            writeString(Convert.ToBase64String(bytes, 0, bytes.Length));
        }

        private void writeDictionary(IDictionary dictionary)
        {
            _output.Append('[');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dictionary)
            {
                if (pendingSeparator) _output.Append(',');
                _output.Append('{');
                writePair("k", entry.Key);
                _output.Append(",");
                writePair("v", entry.Value);
                _output.Append('}');

                pendingSeparator = true;
            }
            _output.Append(']');
        }

        private void writeStringDictionary(IDictionary dictionary)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dictionary)
            {
                if (pendingSeparator) _output.Append(',');

                writePair((string)entry.Key, entry.Value);

                pendingSeparator = true;
            }
            _output.Append('}');
        }

        private void writePair(string key, object value)
        {   
            writeString(key);
            _output.Append(':');
            if (value is string)
                writeString(value.ToString());
            else
                writeValue(value);
        }

        private void writeDateTime(DateTime dateTime)
        {
            // datetime format standard : yyyy-MM-dd HH:mm:ss
            DateTime dt = dateTime;
            //if (_params.UseUTCDateTime)
            //    dt = dateTime.ToUniversalTime();

            _output.AppendFormat("\"{0:yyyy-MM-dd HH:mm:ss}\"", dt);
            

            //if (_params.UseUTCDateTime)
            //    _output.Append("Z");

            
        }

        private void writeGuid(Guid guid)
        {
            writeString(guid.ToString());
        }

        private void writeString(string input)
        {
            _output.Append('\"');
            _output.Append(input);
            _output.Append('\"');
        }

        private Dictionary<string, object> objectToRaw(object input)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            Type type = input.GetType();
            JSON.initializeCache(type);
            var properties = JSON._propertyCache[type];
            foreach (var property in properties.Values)
            {
                //if (property.IsList)
                //    result.Add(property.Name, arrayToRaw(property.UnderlyingType, (IList)property.Get(input)));
                //else
                    result.Add(property.Name, property.Get(input));
            }

            return result;
        }

        private  List<object> arrayToRaw(Type type, IList input)
        {
            List<object> result = new List<object>();
            JSON.initializeCache(type);
            foreach (var element in input)
            {
                if (element == null)
                    result.Add(null);
                else
                    result.Add(objectToRaw(element));
            }

            return result;
        }
    }
}
