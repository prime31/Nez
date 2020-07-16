using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;


namespace Nez.Persistence
{
	/// <summary>
	/// responsible for serializing an object to JSON
	/// </summary>
	public sealed class NsonEncoder : INsonEncoder
	{
		public NsonSettings settings => _settings;

		static readonly StringBuilder _builder = new StringBuilder(2000);
		readonly NsonCacheResolver _cacheResolver = new NsonCacheResolver();
		readonly NsonSettings _settings;
		readonly Dictionary<object, int> _referenceTracker = new Dictionary<object, int>();
		int _referenceCounter = 0;
		int indent;

		/// <summary>
		/// maintains state on whether the first item of an object/array was written. This lets us know if we
		/// need to stick a comma in between values
		/// </summary>
		Stack<bool> _isFirstItemWrittenStack = new Stack<bool>();


		public static string ToNson(object obj, NsonSettings settings)
		{
			var encoder = new NsonEncoder(settings);
			encoder.EncodeValue(obj);
			return encoder.NsonString();
		}

		public NsonEncoder(NsonSettings settings)
		{
			_settings = settings;
			indent = 0;
		}

		public void EncodeKeyValuePair(string key, object value)
		{
			WriteValueDelimiter();
			EncodeString(key, true);
			AppendColon();
			EncodeValue(value);
		}

		/// <summary>
		/// handles writing any arbitrary object type to the JSON string
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="forceTypeHint">If set to <c>true</c> force type hint.</param>
		public void EncodeValue(object value)
		{
			if (value == null)
			{
				_builder.Append("null");
			}
			else if (value is string)
			{
				EncodeString((string)value);
			}
			else if (value is char)
			{
				EncodeString(value.ToString());
			}
			else if (value is bool)
			{
				_builder.Append((bool)value ? "true" : "false");
			}
			else if (value is DateTime)
			{
				var output = ((DateTime)value).ToString(NsonConstants.iso8601Format[0], CultureInfo.InvariantCulture);
				EncodeString(output);
			}
			else if (value is Enum)
			{
				EncodeString(value.ToString());
			}
			else if (value is Array)
			{
				EncodeArray((Array)value);
			}
			else if (value is IList)
			{
				EncodeList((IList)value);
			}
			else if (value is IDictionary)
			{
				EncodeDictionary((IDictionary)value);
			}
			else if (value is float ||
				value is double ||
				value is int ||
				value is uint ||
				value is long ||
				value is sbyte ||
				value is byte ||
				value is short ||
				value is ushort ||
				value is ulong ||
				value is decimal)
			{
				_builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
				return;
			}
			else
			{
				EncodeObject(value);
			}
		}

		public void EncodeString(string value, bool excludeQuotes = false)
		{
            if (!excludeQuotes)
			    _builder.Append('\"');

			var charArray = value.ToCharArray();
			foreach (var c in charArray)
			{
				switch (c)
				{
					case '"':
						_builder.Append("\\\"");
						break;

					case '\\':
						_builder.Append("\\\\");
						break;

					case '\b':
						_builder.Append("\\b");
						break;

					case '\f':
						_builder.Append("\\f");
						break;

					case '\n':
						_builder.Append("\\n");
						break;

					case '\r':
						_builder.Append("\\r");
						break;

					case '\t':
						_builder.Append("\\t");
						break;

					default:
						var codepoint = Convert.ToInt32(c);
						if ((codepoint >= 32) && (codepoint <= 126))
						{
							_builder.Append(c);
						}
						else
						{
							_builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
						}

						break;
				}
			}

            if (!excludeQuotes)
                _builder.Append('\"');
		}

		/// <summary>
		/// encodes an object to the nson stream. 
		/// </summary>
		public void EncodeObject(object value)
		{
			var type = value.GetType();

			WriteStartObject(type.FullName);

			// if this returns true, we have a reference so we are done
			if (WriteOptionalReferenceData(value))
			{
				WriteEndObject();
				return;
			}

			// check for an override converter and use it if present
			var converter = _settings.GetTypeConverterForType(type);
			if (converter != null && converter.CanWrite)
			{
				converter.WriteNson(this, value);
				if (converter.WantsExclusiveWrite)
				{
					WriteEndObject();
					return;
				}
			}

			foreach (var field in _cacheResolver.GetEncodableFieldsForType(type))
			{
				WriteValueDelimiter();
				EncodeString(field.Name, true);
				AppendColon();
				EncodeValue(field.GetValue(value));
			}

			WriteEndObject();
		}

		void EncodeDictionary(IDictionary value)
		{
			WriteStartDictionary();

			foreach (var e in value.Keys)
			{
				WriteValueDelimiter();
				EncodeString(e.ToString(), true);
				AppendColon();
				EncodeValue(value[e]);
			}

			WriteEndDictionary();
		}

		void EncodeList(IList value)
		{
            Type listItemType = null;
            var listType = value.GetType();
            if (listType.IsArray)
            {
                listItemType = listType.GetElementType();
            }
            else
            {
                foreach (Type interfaceType in listType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        listItemType = listType.GetGenericArguments()[0];
                        break;
                    }
                }
            }

            WriteStartArray();

			foreach (var obj in value)
			{
				WriteValueDelimiter();
				EncodeValue(obj);
			}

			WriteEndArray();
		}

		void EncodeArray(Array value)
		{
			if (value.Rank == 1)
			{
				EncodeList(value);
			}
			else
			{
				var indices = new int[value.Rank];
				EncodeArrayRank(value, 0, indices);
			}
		}

		void EncodeArrayRank(Array value, int rank, int[] indices)
		{
			WriteStartArray();

			var min = value.GetLowerBound(rank);
			var max = value.GetUpperBound(rank);

			if (rank == value.Rank - 1)
			{
				var arrayItemType = value.GetType().GetElementType();

                for (var i = min; i <= max; i++)
				{
					indices[rank] = i;
					WriteValueDelimiter();
					var val = value.GetValue(indices);
					EncodeValue(val);
				}
			}
			else
			{
				for (var i = min; i <= max; i++)
				{
					indices[rank] = i;
					WriteValueDelimiter();
					EncodeArrayRank(value, rank + 1, indices);
				}
			}

			WriteEndArray();
		}

		#region Writers

		void AppendIndent()
		{
			for (var i = 0; i < indent; i++)
				_builder.Append('\t');
		}

		public void AppendColon()
		{
			_builder.Append(':');

			if (_settings.PrettyPrint)
				_builder.Append(' ');
		}

		/// <summary>
		/// uses the NsonSettings and object details to deal with reference tracking. If a reference was found and written
		/// to the JSON stream it will return true and the rest of the object data should not be written.
		/// </summary>
		/// <param name="value">Value.</param>
		public bool WriteOptionalReferenceData(object value)
		{
			if (_settings.PreserveReferencesHandling)
			{
				if (!_referenceTracker.ContainsKey(value))
				{
					_referenceTracker[value] = ++_referenceCounter;

					WriteValueDelimiter();
					EncodeString(NsonConstants.IdPropertyName, true);
					AppendColon();
					EncodeString(_referenceCounter.ToString());
				}
				else
				{
					var id = _referenceTracker[value];

					WriteValueDelimiter();
					EncodeString(NsonConstants.RefPropertyName, true);
					AppendColon();
					EncodeString(id.ToString());

					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// writes a comma when needed. Call this before writing any object/array key-value pairs.
		/// </summary>
		public void WriteValueDelimiter()
		{
			if (_isFirstItemWrittenStack.Peek())
			{
				_builder.Append(',');

				if (_settings.PrettyPrint)
					_builder.Append('\n');
			}
			else
			{
				_isFirstItemWrittenStack.Pop();
				_isFirstItemWrittenStack.Push(true);
			}

			if (_settings.PrettyPrint)
				AppendIndent();
		}

		public void WriteStartObject(string type)
		{
			_isFirstItemWrittenStack.Push(false);
			_builder.Append(type);
			_builder.Append('(');

			if (_settings.PrettyPrint)
			{
				_builder.Append('\n');
				indent++;
			}
		}

		public void WriteEndObject()
		{
			_isFirstItemWrittenStack.Pop();
			if (_settings.PrettyPrint)
			{
				_builder.Append('\n');
				indent--;
				AppendIndent();
			}

			_builder.Append(')');
		}

		public void WriteStartDictionary()
		{
			_isFirstItemWrittenStack.Push(false);
			_builder.Append('{');

			if (_settings.PrettyPrint)
			{
				_builder.Append('\n');
				indent++;
			}
		}

		public void WriteEndDictionary()
		{
			_isFirstItemWrittenStack.Pop();
			if (_settings.PrettyPrint)
			{
				_builder.Append('\n');
				indent--;
				AppendIndent();
			}

			_builder.Append('}');
		}

		public void WriteStartArray()
		{
			_isFirstItemWrittenStack.Push(false);
			_builder.Append('[');

			if (_settings.PrettyPrint)
			{
				_builder.Append('\n');
				indent++;
			}
		}

		public void WriteEndArray()
		{
			_isFirstItemWrittenStack.Pop();
			if (_settings.PrettyPrint)
			{
				_builder.Append('\n');
				indent--;
				AppendIndent();
			}

			_builder.Append(']');
		}

		#endregion

		/// <summary>
		/// returns the JSON string and resets the internal state. Do not call any other methods after calling this!
		/// </summary>
		/// <returns></returns>
		public string NsonString()
		{
			var nson = _builder.ToString();
			_builder.Length = 0;
			return nson;
		}

	}
}
