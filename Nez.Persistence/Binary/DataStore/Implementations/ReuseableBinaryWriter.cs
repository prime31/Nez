using System;
using System.IO;
using System.Text;


namespace Nez.Persistence.Binary
{
	public sealed class ReuseableBinaryWriter : IPersistableWriter
	{
		// Size should be around the max number of chars/string * Encoding's max bytes/char
		const int LargeByteBufferSize = 256;

		Stream _outStream;

		// temp space for writing primitives to.
		byte[] _buffer;
		Encoding _encoding;
		Encoder _encoder;

		// Perf optimization stuff
		byte[] _largeByteBuffer; // temp space for writing chars.
		int _maxChars; // max # of chars we can put in _largeByteBuffer

		public ReuseableBinaryWriter() : this(new UTF8Encoding(false, true))
		{
		}

		public ReuseableBinaryWriter(Encoding encoding)
		{
			_buffer = new byte[16];
			_encoding = encoding;
			_encoder = _encoding.GetEncoder();
		}

		public ReuseableBinaryWriter(string file) : this(File.OpenWrite(file))
		{
		}

		public ReuseableBinaryWriter(Stream output) : this(output, new UTF8Encoding(false, true))
		{
		}

		public ReuseableBinaryWriter(Stream output, Encoding encoding)
		{
			if (output == null)
				throw new ArgumentNullException(nameof(output));
			if (encoding == null)
				throw new ArgumentNullException(nameof(encoding));
			if (!output.CanWrite)
				throw new ArgumentException("Argument_StreamNotWritable");

			_outStream = output;
			_buffer = new byte[16];
			_encoding = encoding;
			_encoder = _encoding.GetEncoder();
		}

		public void SetStream(Stream stream)
		{
			_outStream = stream;
		}

		public void SetStream(string file)
		{
			SetStream(File.OpenWrite(file));
		}

		public void Flush()
		{
			_outStream.Flush();
		}

		public void Dispose()
		{
			_outStream.Close();
		}

		#region Primitive Writers

		public unsafe void Write(string value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			var len = _encoding.GetByteCount(value);
			Write7BitEncodedInt(len);

			if (_largeByteBuffer == null)
			{
				_largeByteBuffer = new byte[LargeByteBufferSize];
				_maxChars = _largeByteBuffer.Length / _encoding.GetMaxByteCount(1);
			}

			if (len <= _largeByteBuffer.Length)
			{
				_encoding.GetBytes(value, 0, value.Length, _largeByteBuffer, 0);
				_outStream.Write(_largeByteBuffer, 0, len);
			}
			else
			{
				// Aggressively try to not allocate memory in this loop for runtime performance reasons. Use an Encoder to
				// write out the string correctly (handling surrogates crossing buffer boundaries properly).  
				var charStart = 0;
				var numLeft = value.Length;

				while (numLeft > 0)
				{
					// Figure out how many chars to process this round.
					var charCount = (numLeft > _maxChars) ? _maxChars : numLeft;
					int byteLen;

					checked
					{
						if (charStart < 0 || charCount < 0 || charStart + charCount > value.Length)
							throw new ArgumentOutOfRangeException("charCount");

						fixed (char* pChars = value)
						{
							fixed (byte* pBytes = _largeByteBuffer)
							{
								byteLen = _encoder.GetBytes(pChars + charStart, charCount, pBytes,
									_largeByteBuffer.Length, charCount == numLeft);
							}
						}
					}

					_outStream.Write(_largeByteBuffer, 0, byteLen);
					charStart += charCount;
					numLeft -= charCount;
				}
			}
		}

		public void Write(uint value)
		{
			_buffer[0] = (byte) value;
			_buffer[1] = (byte) (value >> 8);
			_buffer[2] = (byte) (value >> 16);
			_buffer[3] = (byte) (value >> 24);
			_outStream.Write(_buffer, 0, 4);
		}

		public void Write(int value)
		{
			_buffer[0] = (byte) value;
			_buffer[1] = (byte) (value >> 8);
			_buffer[2] = (byte) (value >> 16);
			_buffer[3] = (byte) (value >> 24);
			_outStream.Write(_buffer, 0, 4);
		}

		public unsafe void Write(float value)
		{
			uint tmpValue = *(uint*) &value;
			_buffer[0] = (byte) tmpValue;
			_buffer[1] = (byte) (tmpValue >> 8);
			_buffer[2] = (byte) (tmpValue >> 16);
			_buffer[3] = (byte) (tmpValue >> 24);
			_outStream.Write(_buffer, 0, 4);
		}

		public unsafe void Write(double value)
		{
			ulong tmpValue = *(ulong*) &value;
			_buffer[0] = (byte) tmpValue;
			_buffer[1] = (byte) (tmpValue >> 8);
			_buffer[2] = (byte) (tmpValue >> 16);
			_buffer[3] = (byte) (tmpValue >> 24);
			_buffer[4] = (byte) (tmpValue >> 32);
			_buffer[5] = (byte) (tmpValue >> 40);
			_buffer[6] = (byte) (tmpValue >> 48);
			_buffer[7] = (byte) (tmpValue >> 56);
			_outStream.Write(_buffer, 0, 8);
		}

		public void Write(bool value)
		{
			_buffer[0] = (byte) (value ? 1 : 0);
			_outStream.Write(_buffer, 0, 1);
		}

		public void Write(IPersistable value)
		{
			value.Persist(this);
		}

		#endregion

		void Write7BitEncodedInt(int value)
		{
			// Write out an int 7 bits at a time. The high bit of the byte, when on, tells reader to continue reading more bytes.
			var v = (uint) value;
			while (v >= 0x80)
			{
				_outStream.WriteByte((byte) (v | 0x80));
				v >>= 7;
			}

			_outStream.WriteByte((byte) v);
		}
	}
}