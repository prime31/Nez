using System.IO;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;

namespace Nez.Persistence
{
	/// <summary>
	/// responsible for taking a nson string and decoding it into objects, either generic or strongly typed
	/// </summary>
	public sealed class NsonDecoder : IDisposable
	{
		#region Fields and Props

		static readonly char[] floatingPointCharacters = { '.', 'e' };
		static readonly Type afterDecodeAttrType = typeof(AfterDecodeAttribute);

		const string kWhiteSpace = " \t\n\r";
		const string kWordBreak = " \t\n\r{}[](),:\"";

		// we reuse this sucker so it sticks around as a static
		static readonly StringBuilder _builder = new StringBuilder();

		// cache of all types that have been found.persistant accross calls.
		static readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

		readonly NsonCacheResolver _cacheResolver = new NsonCacheResolver();
		readonly NsonSettings _settings;


		enum Token
		{
			None,
            ClassName,
			OpenBrace,
			CloseBrace,
			OpenBracket,
			CloseBracket,
            OpenParen,
            CloseParen,
			Colon,
			Comma,
			String,
            PropertyKey,
			Number,
			True,
			False,
			Null
		}

        string _lastToken;
		StringReader _nson;

		char PeekChar
		{
			get
			{
				var peek = _nson.Peek();
				return peek == -1 ? '\0' : Convert.ToChar(peek);
			}
		}

		char NextChar => Convert.ToChar(_nson.Read());

		#endregion


		#region static methods

		/// <summary>
		/// decodes <paramref name="nsonString"/> into an object of type T
		/// </summary>
		/// <returns>The nson.</returns>
		/// <param name="nsonString">Nson string.</param>
		/// <param name="settings">Settings.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T FromNson<T>(string nsonString, NsonSettings settings = null)
		{
			return (T)FromNson(nsonString, typeof(T), settings);
		}

		/// <summary>
		/// decodes <paramref name="nsonString"/> into standard system and generic types
		/// </summary>
		/// <returns>The nson.</returns>
		/// <param name="nsonString">Nson string.</param>
		/// <param name="settings">Settings.</param>
		public static object FromNson(string nsonString, NsonSettings settings = null)
		{
			using (var instance = new NsonDecoder(nsonString, settings))
			{
				return instance.DecodeValueUntyped(instance.GetNextToken());
			}
		}

		/// <summary>
		/// decodes <paramref name="nsonString"/> into an object of type <paramref name="type"/>
		/// </summary>
		/// <returns>The nson.</returns>
		/// <param name="nsonString">Nson string.</param>
		/// <param name="type">Type.</param>
		/// <param name="settings">Settings.</param>
		public static object FromNson(string nsonString, Type type, NsonSettings settings = null)
		{
			using (var instance = new NsonDecoder(nsonString, settings))
			{
				return instance.DecodeValue(instance.GetNextToken(), type);
			}
		}

		/// <summary>
		/// overwrites the data on <paramref name="obj"/> with the data serialized from JSON. This will only
		/// work for custom objects, Dictionarys and Lists.
		/// </summary>
		/// <param name="nsonString">Nson string.</param>
		/// <param name="obj">Object.</param>
		/// <param name="settings">Settings.</param>
		public static void FromNsonOverwrite(string nsonString, object obj, NsonSettings settings = null)
		{
			using (var instance = new NsonDecoder(nsonString, settings))
			{
				var type = obj.GetType();
				if (obj is IDictionary)
				{
					instance.DecodeDictionary(type, obj);
				}
				else if (obj is ICollection)
				{
					instance.DecodeList(type, obj);
				}
				else
				{
					try
					{
						instance.DecodeObject(type, obj);
					}
					catch (Exception e)
					{
						throw new DecodeException($"{nameof(FromNsonOverwrite)} only support Dictionary, List and custom objects. Use {nameof(FromNson)} for other types", e);
					}
				}
			}
		}

		/// <summary>
		/// uses a type cache to lookup an objects Type by string
		/// </summary>
		/// <returns>The type.</returns>
		/// <param name="fullName">Full name.</param>
		static Type FindType(string fullName)
		{
			if (fullName == null)
				return null;

			if (_typeCache.TryGetValue(fullName, out var type))
				return type;

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				type = assembly.GetType(fullName);
				if (type != null)
				{
					_typeCache.Add(fullName, type);
					return type;
				}
			}

			return null;
		}

		#endregion


		NsonDecoder(string nsonString, NsonSettings settings = null)
		{
			_nson = new StringReader(nsonString);
			_settings = settings;
		}

		void IDisposable.Dispose()
		{
			_nson.Dispose();
			_nson = null;
		}


		#region Parsing utility methods

		void ConsumeWhiteSpace()
		{
			while (kWhiteSpace.IndexOf(PeekChar) != -1)
			{
				_nson.Read();

				if (_nson.Peek() == -1)
					break;
			}
		}

		string GetNextWord()
		{
			while (kWordBreak.IndexOf(PeekChar) == -1)
			{
				_builder.Append(NextChar);

				if (_nson.Peek() == -1)
					break;
			}

			var word = _builder.ToString();
			_builder.Length = 0;
			return word;
		}

		Token GetNextToken()
		{
			ConsumeWhiteSpace();

			if (_nson.Peek() == -1)
				return Token.None;

			switch (PeekChar)
			{
				case '{':
					return Token.OpenBrace;
				case '}':
					_nson.Read();
					return Token.CloseBrace;
                case '[':
                    return Token.OpenBracket;
                case ']':
                    _nson.Read();
                    return Token.CloseBracket;
                case '(':
                    return Token.OpenParen;
                case ')':
                    _nson.Read();
                    return Token.CloseParen;
                case ',':
					_nson.Read();
					return Token.Comma;
				case '"':
					return Token.String;
				case ':':
					return Token.Colon;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '-':
					return Token.Number;
			}

            var word = GetNextWord();
            switch (word)
			{
				case "false":
					return Token.False;
				case "true":
					return Token.True;
				case "null":
					return Token.Null;
                default:
                    _lastToken = word;
                    if (PeekChar == '(')
                        return Token.ClassName;
                    else
                        return Token.PropertyKey;
			}
		}

		/// <summary>
		/// parses the string into a long, ulong, decimal or double. If parsing fails returns 0.
		/// </summary>
		/// <returns>The number.</returns>
		/// <param name="value">Value.</param>
		IConvertible ParseNumber(string value)
		{
			if (value.IndexOfAny(floatingPointCharacters) == -1)
			{
				if (value[0] == '-')
				{
					if (long.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long parsedValue))
					{
						return parsedValue;
					}
				}
				else
				{
					if (ulong.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out ulong parsedValue))
					{
						return parsedValue;
					}
				}
			}

			if (decimal.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal decimalValue))
			{
				// Check for decimal underflow.
				if (decimalValue == decimal.Zero)
				{
					if (double.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out double parsedValue))
					{
						if (Math.Abs(parsedValue) > double.Epsilon)
						{
							return parsedValue;
						}
					}
				}

				return decimalValue;
			}

			if (double.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out double doubleValue))
			{
				return doubleValue;
			}

			return 0;
		}

		/// <summary>
		/// takes in either seconds or milliseoncds since the epoch and converts it to a DateTime
		/// </summary>
		/// <returns>The time from epoch time.</returns>
		/// <param name="number">Number.</param>
		DateTime DateTimeFromEpochTime(long number)
		{
			const long minSeconds = -62135596800;
			const long maxSeconds = 253402300799;

			if (number < minSeconds || number > maxSeconds)
			{
				return DateTimeOffset.FromUnixTimeMilliseconds(number).UtcDateTime;
			}
			return DateTimeOffset.FromUnixTimeSeconds(number).UtcDateTime;
		}

		#endregion

		string DecodeString()
		{
			// ditch opening quote if we have one
            if (PeekChar == '"')
                _nson.Read();

            var parsing = true;
			while (parsing)
			{
				if (_nson.Peek() == -1)
				{
					parsing = false;
					break;
				}

				var c = NextChar;
				switch (c)
				{
					case '"':
						parsing = false;
						break;
					case '\\':
						if (_nson.Peek() == -1)
						{
							parsing = false;
							break;
						}

						c = NextChar;
						switch (c)
						{
							case '"':
							case '\\':
							case '/':
								_builder.Append(c);
								break;
							case 'b':
								_builder.Append('\b');
								break;
							case 'f':
								_builder.Append('\f');
								break;
							case 'n':
								_builder.Append('\n');
								break;
							case 'r':
								_builder.Append('\r');
								break;
							case 't':
								_builder.Append('\t');
								break;
							case 'u':
								var hex = new StringBuilder();
								for (var i = 0; i < 4; i++)
									hex.Append(NextChar);

								_builder.Append((char)Convert.ToInt32(hex.ToString(), 16));
								break;
							default:
								throw new DecodeException( @"Illegal character following escape character: " + c );
						}
						break;

					default:
						_builder.Append(c);
						break;
				}
			}

			var str = _builder.ToString();
			_builder.Length = 0;
			return str;
		}

		/// <summary>
		/// decodes and returns the next value in the JSON string
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="token">Token.</param>
		/// <param name="type">Type.</param>
		object DecodeValue(Token token, Type type)
		{
			// handle Nullables. If the type is Nullable use the underlying type
			type = Nullable.GetUnderlyingType(type) ?? type;

			switch (token)
			{
				case Token.String:
					var str = DecodeString();
					if (type == typeof(DateTime))
						return DateTime.ParseExact(str, NsonConstants.iso8601Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
					return type.IsEnum ? Enum.Parse(type, str) : str;
				case Token.Number:
					if (type == typeof(DateTime))
						return DateTimeFromEpochTime((long)Convert.ChangeType(ParseNumber(GetNextWord()), typeof(long)));
					return Convert.ChangeType(ParseNumber(GetNextWord()), type);
                case Token.ClassName:
                    return DecodeObject(type);
				case Token.OpenBrace:
					// either a Dictionary or an object
					if (typeof(IDictionary).IsAssignableFrom(type))
						return DecodeDictionary(type);
					return DecodeObject(type);
				case Token.OpenBracket:
					// Array, MultiRank Array or a List
					return DecodeArrayOrList(type);
				case Token.True:
					return true;
				case Token.False:
					return false;
				case Token.Null:
					return null;
				default:
					return null;
			}
		}

		/// <summary>
		/// helper that lets us grab the data without a Type. This can occur when we have a key that we cant
		/// find on an object via reflection. We still need to parse the JSON but in an untyped way,
		/// </summary>
		/// <returns>The value untyped.</returns>
		/// <param name="token">Token.</param>
		object DecodeValueUntyped(Token token)
		{
			switch (token)
			{
				case Token.String:
					return DecodeString();
				case Token.Number:
					return ParseNumber(GetNextWord());
                case Token.ClassName:
                    return DecodeObject(null);
				case Token.OpenBrace:
					return DecodeDictionary(typeof(Dictionary<string, object>));
				case Token.OpenBracket:
					return DecodeArrayOrList(typeof(List<object>));
				case Token.True:
					return true;
				case Token.False:
					return false;
				case Token.Null:
					return null;
				default:
					return null;
			}
		}

		/// <summary>
		/// grabs the Field/PropertyInfo of <paramref name="obj"/> for <paramref name="key"/> and sets it
		/// with the next element in the JSON string.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <param name="key">Key.</param>
		void SetNextValueOnObject(ref object obj, string key)
		{
			var field = _cacheResolver.GetField(obj.GetType(), key);
			if (field != null)
			{
				if (_cacheResolver.IsMemberInfoEncodeableOrDecodeable(field, field.IsPublic))
				{
					var value = DecodeValue(GetNextToken(), field.FieldType);
					if (obj.GetType().IsValueType)
					{
						// obj is a struct.
						var instanceRef = obj;
						field.SetValue(instanceRef, value);
						obj = instanceRef;
						return;
					}

					// obj is a class.
					field.SetValue(obj, value);
					return;
				}
			}

			// we still need to eat the value even if we didnt set it so the parser is in the right spot
			var orhpanedValue = DecodeValueUntyped(GetNextToken());

			// check for a NsonConverter and use it if we have one
			var converter = _settings?.GetTypeConverterForType(obj.GetType());
			if (converter != null && converter.CanRead)
				converter.OnFoundCustomData(ref obj, key, orhpanedValue);
		}

		object DecodeArrayOrList(Type type, object obj = null)
		{
			if (type.IsArray)
			{
				if (type.GetArrayRank() == 1)
					return DecodeArray(type);

				// we have no idea how many elements are in the array so we have to use a List
				var elementType = type.GetElementType();
				var listType = typeof(List<>);
				var constructedListType = listType.MakeGenericType(elementType);

				// weeeeeee! Nest lists of lists for the array rank - 1
				for (var i = 0; i < type.GetArrayRank() - 1; i++)
				{
					constructedListType = listType.MakeGenericType(constructedListType);
				}

				var arrayData = DecodeList(constructedListType);

				var arrayRank = type.GetArrayRank();
				var rankLengths = new int[arrayRank];
				if (CanBeMultiRankArray(arrayData, 0, rankLengths))
				{
					var array = Array.CreateInstance(elementType, rankLengths);
					try
					{
						DecodeMultiRankArray(elementType, arrayData, array, 1, rankLengths);
					}
					catch (Exception e)
					{
						throw new DecodeException("Error decoding multidimensional array. Did you try to decode into an array of incompatible rank or element type?", e);
					}

					return Convert.ChangeType(array, type);
				}
				throw new DecodeException("Error decoding multidimensional array; JSON data doesn't seem fit this structure.");
			}

			if (typeof(IList).IsAssignableFrom(type))
			{
				return DecodeList(type);
			}

			throw new DecodeException("Error decoding array. Could not figure out what to do. Gave up.");
		}

		IList DecodeArray(Type type, object obj = null)
		{
			// we have no idea how many elements are in the array so we have to use a List temporarily
			var elementType = type.GetElementType();
			var listType = typeof(List<>);
			var constructedListType = listType.MakeGenericType(elementType);

			var list = DecodeList(constructedListType);
			var array = Array.CreateInstance(elementType, list.Count);
			list.CopyTo(array, 0);

			return array;
		}

		void DecodeMultiRankArray(Type elementType, IList arrayData, Array array, int arrayRank, int[] rankLengths)
		{
			var count = arrayData.Count;
			for (var i = 0; i < count; i++)
			{
				rankLengths[arrayRank - 1] = i;
				if (arrayRank < array.Rank)
				{
					DecodeMultiRankArray(elementType, arrayData[i] as IList, array, arrayRank + 1, rankLengths);
				}
				else
				{
					array.SetValue(arrayData[i], rankLengths);
				}
			}
		}

		bool CanBeMultiRankArray(IList list, int rank, int[] rankLengths)
		{
			var count = list.Count;
			rankLengths[rank] = count;

			if (rank == rankLengths.Length - 1)
			{
				return true;
			}

			var firstItem = list[0] as IList;
			if (firstItem == null)
			{
				return false;
			}

			var firstItemCount = firstItem.Count;

			for (var i = 1; i < count; i++)
			{
				var item = list[i] as IList;
				if (item == null)
				{
					return false;
				}

				if (item.Count != firstItemCount)
				{
					return false;
				}

				if (!CanBeMultiRankArray(item, rank + 1, rankLengths))
				{
					return false;
				}
			}

			return true;
		}

		IList DecodeList(Type type, object obj = null)
		{
			var innerType = type.GetGenericArguments()[0];

			var list = obj as IList;
			if (list == null)
			{
				list = (IList)_cacheResolver.CreateInstance(typeof(List<>).MakeGenericType(innerType));
			}

			// Ditch opening bracket.
			_nson.Read();

			// [
			var parsing = true;
			while (parsing)
			{
				var nextToken = GetNextToken();
				switch (nextToken)
				{
					case Token.None:
						return null;
					case Token.Comma:
						continue;
                    case Token.ClassName:
                        list.Add(DecodeObject());
                        break;
					case Token.OpenBracket:
						// Array, MultiRank Array or a List
						list.Add(DecodeArrayOrList(innerType));
						break;
					case Token.CloseBracket:
						parsing = false;
						break;
					default:
						list.Add(DecodeValue(nextToken, innerType));
						break;
				}
			}

			return list;
		}

		IDictionary DecodeDictionary(Type type, object obj = null)
		{
			var keyType = type.GetGenericArguments()[0];
			var valueType = type.GetGenericArguments()[1];

			var dict = obj as IDictionary;
			if (dict == null)
				dict = (IDictionary)_cacheResolver.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));

			// Ditch opening bracket
            if (PeekChar == '{')
			    _nson.Read();

			while (true)
			{
				switch (GetNextToken())
				{
					case Token.None:
						return null;
					case Token.Comma:
						continue;
                    case Token.CloseParen:
                    case Token.CloseBrace:
						return dict;
					default:
						{
                            string key = null;
                            if (_lastToken != null)
                            {
                                key = _lastToken;
                                _lastToken = null;
                            }
                            else
                            {
                                // Key
                                key = DecodeString();
                                if (string.IsNullOrEmpty(key))
                                    return null;
                            }

							// :
							if (GetNextToken() != Token.Colon)
								return null;

							_nson.Read();

							var k = keyType.IsEnum ? Enum.Parse(keyType, key) : Convert.ChangeType(key, keyType);
							dict[k] = valueType == typeof(object) ? DecodeValueUntyped(GetNextToken()) : dict[k] = DecodeValue(GetNextToken(), valueType);
							break;
						}
				}
			}
		}

		object DecodeObject(Type type = null, object obj = null)
		{
			// Ditch opening paren
			_nson.Read();

			string id = null;

            if (obj == null && _lastToken != null)
            {
                var makeType = FindType(_lastToken);
                if (makeType == null)
                    throw new TypeLoadException($"Could not find type '{_lastToken}' in any loaded assemblies");

                _lastToken = null;
                type = makeType;

                // handle the NsonObjectFactory if we have one
                var converter = _settings?.GetObjectFactoryForType(makeType);
                if (converter != null)
                {
                    // fetch the remaining data into a Dictionary and pass it off to the factory
                    var dict = new Dictionary<string, object>();
                    DecodeDictionary(dict.GetType(), dict);
                    obj = converter.CreateObject(type, dict);
                    return obj;
                }

                obj = _cacheResolver.CreateInstance(makeType);
            }

            // {
            while (true)
			{
				switch (GetNextToken())
				{
					case Token.None:
					case Token.Null:
						return null;
					case Token.Comma:
						continue;
					case Token.CloseParen:
						// Invoke methods tagged with [AfterDecode] attribute.
						foreach (var method in type.GetMethods(NsonConstants.instanceBindingFlags))
						{
							if (method.IsDefined(afterDecodeAttrType) && method.GetParameters().Length == 0)
								method.Invoke(obj, null);
						}
						return obj;
					default:
						{
                            string key = null;
                            if (_lastToken != null)
                            {
                                key = _lastToken;
                                _lastToken = null;
                            }
                            else
                            {
                                // Key
                                key = DecodeString();
                                if (string.IsNullOrEmpty(key))
                                    return null;
                            }

                            // :
                            if (GetNextToken() != Token.Colon)
								return null;

							_nson.Read();

							// check for @id or @ref
							if (key == NsonConstants.IdPropertyName)
							{
								var dump = GetNextToken();
								id = DecodeString();
								break;
							}

							if (key == NsonConstants.RefPropertyName)
							{
								var dump = GetNextToken();
								var refObj = _cacheResolver.ResolveReference(DecodeString());

								// we could break and let the next iteration catch the ) but then the AfterDecode method
								// will be called multiple times so instead we eat the token.
								dump = GetNextToken();
								return refObj;
							}

							if (id != null)
							{
								_cacheResolver.TrackReference(id, obj);
								id = null;
							}

							SetNextValueOnObject(ref obj, key);

							break;
						} // end default
				} // end switch
			}
		}

	}
}
