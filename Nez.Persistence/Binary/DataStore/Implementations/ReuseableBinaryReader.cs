using System;
using System.IO;
using System.Text;


namespace Nez.Persistence.Binary
{
	// if we want to get really crazy, we can switch this to a buffered reader: https://jacksondunstan.com/articles/3568
	// the string read code would be the only tricky part.
	public sealed class ReuseableBinaryReader : IPersistableReader
	{
		const int MaxCharBytesSize = 128;
		const int MaxBuilderSize = 360;

		Stream _stream;
		byte[] _buffer;
		Decoder _decoder;
		byte[] _charBytes;
		char[] _charBuffer;
		int _maxCharsSize;
		StringBuilder _stringBuilder;


		public ReuseableBinaryReader() : this(new UTF8Encoding())
		{
		}

		public ReuseableBinaryReader(Encoding encoding)
		{
			_decoder = encoding.GetDecoder();
			_maxCharsSize = encoding.GetMaxCharCount(MaxCharBytesSize);

			// max bytes per one char
			var minBufferSize = encoding.GetMaxByteCount(1);
			if (minBufferSize < 16)
				minBufferSize = 16;
			_buffer = new byte[minBufferSize];
		}

		public ReuseableBinaryReader(string filename) : this(File.OpenRead(filename))
		{
		}

		public ReuseableBinaryReader(Stream input) : this(input, new UTF8Encoding())
		{
		}

		public ReuseableBinaryReader(Stream input, Encoding encoding)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			if (encoding == null)
				throw new ArgumentNullException(nameof(encoding));
			if (!input.CanRead)
				throw new ArgumentException("Argument_StreamNotReadable");

			_stream = input;
			_decoder = encoding.GetDecoder();
			_maxCharsSize = encoding.GetMaxCharCount(MaxCharBytesSize);

			// max bytes per one char
			var minBufferSize = encoding.GetMaxByteCount(1);
			if (minBufferSize < 16)
				minBufferSize = 16;
			_buffer = new byte[minBufferSize];
		}

		~ReuseableBinaryReader()
		{
			Dispose(true);
		}

		public void SetStream(Stream stream)
		{
			_stream = stream;
		}

		public void SetStream(string file)
		{
			SetStream(File.OpenRead(file));
		}

		public void Close()
		{
			_stream.Close();
			_stream = null;
		}

		public void Dispose()
		{
			Dispose(false);
		}

		void Dispose(bool isGarbageCollected)
		{
			var copyOfStream = _stream;
			_stream = null;
			if (copyOfStream != null)
				copyOfStream.Close();

			if (isGarbageCollected)
			{
				_stream = null;
				_buffer = null;
				_decoder = null;
				_charBytes = null;
				_charBuffer = null;
				_stringBuilder = null;
			}
		}

		#region Primitive Readers

		public bool ReadBool()
		{
			FillBuffer(1);
			return (_buffer[0] != 0);
		}

		public byte ReadByte()
		{
			int b = _stream.ReadByte();
			if (b == -1)
				throw new Exception("__Error.EndOfFile()");

			return (byte) b;
		}

		public int ReadInt()
		{
			FillBuffer(4);
			return (int) (_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
		}

		public uint ReadUInt()
		{
			FillBuffer(4);
			return (uint) (_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
		}

		public unsafe float ReadFloat()
		{
			FillBuffer(4);
			uint tmpBuffer = (uint) (_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
			return *((float*) &tmpBuffer);
		}

		public unsafe double ReadDouble()
		{
			FillBuffer(8);
			uint lo = (uint) (_buffer[0] | _buffer[1] << 8 |
			                  _buffer[2] << 16 | _buffer[3] << 24);
			uint hi = (uint) (_buffer[4] | _buffer[5] << 8 |
			                  _buffer[6] << 16 | _buffer[7] << 24);

			ulong tmpBuffer = ((ulong) hi) << 32 | lo;
			return *((double*) &tmpBuffer);
		}

		public string ReadString()
		{
			if (_stream == null)
				throw new Exception("__Error.FileNotOpen()");

			int currPos = 0;
			int n;
			int stringLength;
			int readLength;
			int charsRead;

			// Length of the string in bytes, not chars
			stringLength = Read7BitEncodedInt();
			if (stringLength < 0)
				throw new IOException("IO.IO_InvalidStringLen_Len" + stringLength);

			if (stringLength == 0)
				return String.Empty;

			if (_charBytes == null)
				_charBytes = new byte[MaxCharBytesSize];

			if (_charBuffer == null)
				_charBuffer = new char[_maxCharsSize];

			do
			{
				readLength = ((stringLength - currPos) > MaxCharBytesSize)
					? MaxCharBytesSize
					: (stringLength - currPos);

				n = _stream.Read(_charBytes, 0, readLength);
				if (n == 0)
					throw new Exception("__Error.EndOfFile()");

				charsRead = _decoder.GetChars(_charBytes, 0, n, _charBuffer, 0);

				if (currPos == 0 && n == stringLength)
					return new string(_charBuffer, 0, charsRead);

				// if we got this far, we have a big string so lazily create the StringBuilder and cache it then start appending
				if (_stringBuilder == null)
					_stringBuilder = new StringBuilder(MaxBuilderSize);
				_stringBuilder.Append(_charBuffer, 0, charsRead);
				currPos += n;
			} while (currPos < stringLength);

			// fetch the string, clear the builder and return it so that next iteration we are ready to go
			var ret = _stringBuilder.ToString();
			_stringBuilder.Clear();
			return ret;
		}

		#endregion

		void FillBuffer(int numBytes)
		{
			var bytesRead = 0;
			var n = 0;

			// Need to find a good threshold for calling ReadByte() repeatedly
			// vs. calling Read(byte[], int, int) for both buffered & unbuffered streams.
			if (numBytes == 1)
			{
				n = _stream.ReadByte();
				if (n == -1)
					throw new Exception("__Error.EndOfFile()");

				_buffer[0] = (byte) n;
				return;
			}

			do
			{
				n = _stream.Read(_buffer, bytesRead, numBytes - bytesRead);
				if (n == 0)
					throw new Exception("__Error.EndOfFile()");

				bytesRead += n;
			} while (bytesRead < numBytes);
		}

		int Read7BitEncodedInt()
		{
			// Read out an Int32 7 bits at a time.  The high bit
			// of the byte when on means to continue reading more bytes.
			int count = 0;
			int shift = 0;
			byte b;
			do
			{
				// Check for a corrupted stream. Read a max of 5 bytes.
				if (shift == 5 * 7) // 5 bytes max per Int32, shift += 7
					throw new FormatException("Format_Bad7BitInt32");

				// ReadByte handles end of stream cases for us.
				b = ReadByte();
				count |= (b & 0x7F) << shift;
				shift += 7;
			} while ((b & 0x80) != 0);

			return count;
		}
	}
}