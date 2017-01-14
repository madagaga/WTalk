using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coreJson
{
    public class ByteQueue
    {
        private byte[] _buffer;

        int _position = 0;

        public int Count
        {
            get { return _buffer.Length - _position; }
        }

        public int Position
        {
            get
            {
                return _position;
            }
        }

        public ByteQueue(byte[] v)
        {
            this._buffer = v;
        }

        internal byte Dequeue()
        {
            return _buffer[_position++];
        }

        internal byte Peek()
        {
            return _buffer[_position];
        }

        internal byte ReversePeek(int step = 1)
        {
            return _buffer[Math.Max(0,_position-step)];
        }

        internal byte[] Take(int length)
        {
            byte[] buffer = new byte[length];
            for (int i = 0; i < length; i++)
                buffer[i] = _buffer[_position + i];

            return buffer;
        }

        internal void Skip(int length)
        {
            _position += length;
        }
    }
}
