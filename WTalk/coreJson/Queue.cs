using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coreJson
{
    public class ByteQueue : IDisposable
    {

        private System.IO.Stream _stream;
        
        public int Count
        {
            get { return (int)(_stream.Length - _stream.Position); }
        }

        public int Position
        {
            get
            {
                return (int)_stream.Position;
            }
        }

        public ByteQueue(System.IO.Stream input)
        {
            _stream = input;
            _stream.Position = 0;
        }

        public ByteQueue(byte[] v)
        {
            _stream = new System.IO.MemoryStream(v);
        }

        internal byte Read()
        {

            int value = _stream.ReadByte();
            if (value != -1)
                return (byte)value;
            else
                return 0;
        }

        internal byte[] Read(int length)
        {
            byte[] buffer = new byte[length];
            _stream.Read(buffer, 0, length);
            _stream.Position--;
            return buffer;
        }

        internal byte Peek(int step = 0)
        {
            long originalPosition = _stream.Position;
            _stream.Position += step;
            byte value = Read();
            _stream.Position = originalPosition;
            return value;
        }
        
        internal void Skip(int length = 1)
        {
            _stream.Position += length;
        }

        public void Dispose()
        {
            _stream.Dispose();            
        }
    }
}
