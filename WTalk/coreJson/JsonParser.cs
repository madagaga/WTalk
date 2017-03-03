using System;
using System.Collections.Generic;
using System.Linq;

namespace coreJson
{
    public class JsonParser : IDisposable
    {
        enum ParseToken
        {
            None = -1,           // Used to denote no Lookahead available
            Curly_Open,
            Curly_Close,
            Squared_Open,
            Squared_Close,
            Colon,
            Comma,
            String,
            UnprotectedString,
            Number,
            True,
            False,
            Null
        }

        #region constants 
        // tokens char
        static readonly Dictionary<byte, ParseToken> _tokens = new Dictionary<byte, ParseToken>()
        {
            { 123, ParseToken.Curly_Open },
            { 125, ParseToken.Curly_Close },
            { 91, ParseToken.Squared_Open },
            { 93, ParseToken.Squared_Close }
        };

        static readonly Dictionary<byte, ParseToken> _stringDelimiter = new Dictionary<byte, ParseToken>()
        {
            { 34, ParseToken.String },
            { 39, ParseToken.String }
        };

        // delimiters/separators char
        static readonly Dictionary<byte, ParseToken> _separators = new Dictionary<byte, ParseToken>()
        {            
            { 44, ParseToken.Comma},
            { 58, ParseToken.Colon}
        };

        // invisible char
        static readonly Dictionary<byte, string> _invisibles = new Dictionary<byte, string>()
        {
            { 32, "space"},
            { 9, "tab"},
            { 10, "line feed"},
            { 13, "carriage return"}            
        };

        static readonly Dictionary<byte, KeyValuePair<byte[], ParseToken>> _constants = new Dictionary<byte, KeyValuePair<byte[], ParseToken>>()
        {
            { 102, new KeyValuePair<byte[], ParseToken>( new byte[] {102,97,108,115,101 }, ParseToken.False)},
            { 116, new KeyValuePair<byte[], ParseToken>( new byte[] {116,114,117,101 }, ParseToken.True)},
            { 110, new KeyValuePair<byte[], ParseToken>( new byte[] {110,117,108,108 }, ParseToken.Null)}
        };

        #endregion

        ByteQueue _rawData;
        ParseToken _nextToken = ParseToken.None;
        public JsonParser(){ }

        public object Parse(System.IO.Stream input)
        {
            _rawData = new ByteQueue(input);
            return parseValue();
        }

        public object Parse(string input)
        {
            _rawData = new ByteQueue(System.Text.Encoding.UTF8.GetBytes(input));
            return parseValue();
        }

        private object parseValue()
        {
            switch (getNextToken())
            {
                case ParseToken.Number:
                    return parseNumber();

                case ParseToken.String:
                case ParseToken.UnprotectedString:
                    return parseString();

                case ParseToken.Curly_Open:
                    return parseObject();

                case ParseToken.Squared_Open:
                    return parseArray();

                case ParseToken.True:
                    consumeToken();
                    return true;

                case ParseToken.False:
                    consumeToken();
                    return false;

                case ParseToken.Null:
                    consumeToken();
                    return null;
            }

            throw new Exception("Unrecognized token at index");
        }

        private object parseNumber()
        {
            consumeToken();
            string number = parseString(true);

            try
            {
                if (number.IndexOf('.') > -1)
                    return float.Parse(number, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

                return long.Parse(number, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("error :{1} {0}", e.Message, number));
                return 0;
            }
            
        }

        private string parseString(bool include_comma = false)
        {
            Func<byte, bool> condition = null;
            Action<byte> consumeDelimiter = null;
            byte delimiter = _rawData.Peek(-1); // should be 34 or 39

            if (include_comma || getNextToken() == ParseToken.UnprotectedString)
            {
                condition = (b) => { return !_tokens.ContainsKey(b) && !_separators.ContainsKey(b) && !_stringDelimiter.ContainsKey(b); };
                consumeDelimiter = (b) =>
                {
                    if (_stringDelimiter.ContainsKey(b))
                        _rawData.Skip();
                };
            }
            else
            {

                condition = (b) =>
                {

                    return b != delimiter || (b == delimiter &&  _rawData.Peek(-1) == 92);
                    //this should be the normal parsing. Start delimiter is end delimiter

                    //return !_stringDelimiter.ContainsKey(b) || (_stringDelimiter.ContainsKey(b) && _rawData.ReversePeek() == 92);

                };
                consumeDelimiter = (b) =>
                {
                    if (b == delimiter)
                        _rawData.Skip();
                };
            }

            List<byte> result = new List<byte>();
            
            consumeToken();

            byte c = _rawData.Peek();

            consumeDelimiter(c);

            while (condition(c))
            {
                result.Add(_rawData.Read());
                c = _rawData.Peek();
            }

            c = _rawData.Peek();

            consumeDelimiter(c);

            return System.Text.RegularExpressions.Regex.Unescape(System.Text.Encoding.UTF8.GetString(result.ToArray(),0,result.Count).ToString());

        }

        private object parseObject()
        {
            Dictionary<string, object> table = new Dictionary<string, object>();

            consumeToken(); // {

            while (true)
            {
                switch (getNextToken())
                {

                    case ParseToken.Comma:
                        consumeToken();
                        break;

                    case ParseToken.Curly_Close:
                        consumeToken();
                        return table;

                    default:
                        {

                            // name
                            string name = parseString();
                            //if (_ignorecase)
                            //    name = name.ToLower();

                            // :
                            if (getNextToken() != ParseToken.Colon)
                                throw new Exception("Expected colon at index ");

                            consumeToken();
                            // value
                            object value = parseValue();

                            table[name] = value;
                        }
                        break;
                }
            }
        }

        private object parseArray()
        {
            List<object> array = new List<object>();
            consumeToken(); // [

            while (true)
            {
                switch (getNextToken())
                {

                    case ParseToken.Comma:
                        if (_rawData.Peek(-2) == 91)
                            array.Add(null);
                        consumeToken();
                        if (getNextToken() == ParseToken.Comma)
                        {
                            array.Add(null);
                            //consumeToken();
                        }

                        break;

                    case ParseToken.Squared_Close:
                        consumeToken();
                        return array;

                    default:
                        array.Add(parseValue());
                        break;
                }
            }
        }

        void consumeToken()
        {
            _nextToken = ParseToken.None;
        }

        ParseToken getNextToken()
        {
            if (_nextToken != ParseToken.None)
                return _nextToken;

            _nextToken = gotoNextToken();

            if (_nextToken != ParseToken.None && _nextToken != ParseToken.UnprotectedString && _nextToken != ParseToken.Number)
                _rawData.Skip();

            return _nextToken;
        }

        ParseToken gotoNextToken()
        {
            byte c;
            do
            {
                c = _rawData.Peek();
                if (!_invisibles.ContainsKey(c))
                    break;
                _rawData.Skip();

            } while (_rawData.Count > 0);


            if(_rawData.Count == 0)
                throw new Exception("Reached end of string unexpectedly");

            // if token
            if (_tokens.ContainsKey(c))
                return _tokens[c];
            // if string 
            if (_stringDelimiter.ContainsKey(c))
                return _stringDelimiter[c];
            // if separator 
            if (_separators.ContainsKey(c))
                return _separators[c];

            // if number 
            if ((c >= 48 && c <= 57) || c == 43 || c == 45 || c == 46)
                return ParseToken.Number;

            // if constant
            if(_constants.ContainsKey(c))
            {
                byte[] buffer = _rawData.Read(_constants[c].Key.Length);
                if (buffer.SequenceEqual(_constants[c].Key))
                {
                    //_rawData.Skip(buffer.Length -1);
                    return _constants[c].Value;
                }
            }

            //default set string if not detected
            return ParseToken.UnprotectedString;



        }

        public void Dispose()
        {
            _rawData.Dispose();
        }
    }
}
