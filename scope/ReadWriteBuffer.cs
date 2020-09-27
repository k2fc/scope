using System;
using System.Collections.Generic;

namespace DGScope
{
    public class ReadWriteBuffer
    {
        private readonly byte[] _buffer;
        private int _startIndex, _endIndex;

        public ReadWriteBuffer(int capacity)
        {
            _buffer = new byte[capacity];
        }

        public int Count
        {
            get
            {
                if (_endIndex > _startIndex)
                    return _endIndex - _startIndex;
                if (_endIndex < _startIndex)
                    return (_buffer.Length - _startIndex) + _endIndex;
                return 0;
            }
        }

        public void Write(byte[] data)
        {
            if (Count + data.Length > _buffer.Length)
                throw new Exception("buffer overflow");
            if (_endIndex + data.Length >= _buffer.Length)
            {
                var endLen = _buffer.Length - _endIndex;
                var remainingLen = data.Length - endLen;

                Array.Copy(data, 0, _buffer, _endIndex, endLen);
                Array.Copy(data, endLen, _buffer, 0, remainingLen);
                _endIndex = remainingLen;
            }
            else
            {
                Array.Copy(data, 0, _buffer, _endIndex, data.Length);
                _endIndex += data.Length;
            }
        }

        public byte[] Read(int len, bool keepData = false)
        {
            if (len > Count)
                throw new Exception("not enough data in buffer");
            var result = new byte[len];
            if (_startIndex + len < _buffer.Length)
            {
                Array.Copy(_buffer, _startIndex, result, 0, len);
                if (!keepData)
                    _startIndex += len;
                return result;
            }
            else
            {
                var endLen = _buffer.Length - _startIndex;
                var remainingLen = len - endLen;
                Array.Copy(_buffer, _startIndex, result, 0, endLen);
                Array.Copy(_buffer, 0, result, endLen, remainingLen);
                if (!keepData)
                    _startIndex = remainingLen;
                return result;
            }
        }

        public byte this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new ArgumentOutOfRangeException();
                return _buffer[(_startIndex + index) % _buffer.Length];
            }
        }

        public IEnumerable<byte> Bytes
        {
            get
            {
                for (var i = 0; i < Count; i++)
                    yield return _buffer[(_startIndex + i) % _buffer.Length];
            }
        }
    }
}
