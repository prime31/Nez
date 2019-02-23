using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;


namespace Nez.Persistance
{
	public sealed class Encoder
	{
		readonly StringBuilder _builder;
		readonly JsonSettings _settings;
		Dictionary<object, int> _referenceTracker = new Dictionary<object, int>();
		int _referenceCounter = 0;
		int indent;


		Encoder( JsonSettings settings )
		{
			this._settings = settings;
			_builder = new StringBuilder();
			indent = 0;
		}

		public static string Encode( object obj, JsonSettings settings )
		{
			var instance = new Encoder( settings );
			instance.EncodeValue( obj );
			return instance._builder.ToString();
		}

		void EncodeValue( object value, bool forceTypeHint = false )
		{
			if( value == null )
			{
				_builder.Append( "null" );
			}
			else if( value is string )
			{
				WriteString( (string)value );
			}
			else if( value is ProxyString )
			{
				WriteString( ( (ProxyString)value ).ToString( CultureInfo.InvariantCulture ) );
			}
			else if( value is char )
			{
				WriteString( value.ToString() );
			}
			else if( value is bool )
			{
				_builder.Append( (bool)value ? "true" : "false" );
			}
			else if( value is Enum )
			{
				WriteString( value.ToString() );
			}
			else if( value is Array )
			{
				EncodeArray( (Array)value );
			}
			else if( value is IList )
			{
				EncodeList( (IList)value );
			}
			else if( value is IDictionary )
			{
				EncodeDictionary( (IDictionary)value );
			}
			else if( value is ProxyArray )
			{
				EncodeProxyArray( (ProxyArray)value );
			}
			else if( value is ProxyObject )
			{
				EncodeProxyObject( (ProxyObject)value );
			}
			else if( value is float ||
				value is double ||
				value is int ||
				value is uint ||
				value is long ||
				value is sbyte ||
				value is byte ||
				value is short ||
				value is ushort ||
				value is ulong ||
				value is decimal ||
				value is ProxyBoolean ||
				value is ProxyNumber )
			{
				_builder.Append( Convert.ToString( value, CultureInfo.InvariantCulture ) );
				return;
			}
			else
			{
				EncodeObject( value, forceTypeHint );
			}
		}

		#region Reflection Helpers

		IEnumerable<FieldInfo> GetEncodableFieldsForType( Type type )
		{
			IEnumerable<FieldInfo> allFields = null;
			if( _settings.EnforceHeirarchyOrderEnabled )
			{
				var types = new Stack<Type>();
				while( type != null )
				{
					types.Push( type );
					type = type.BaseType;
				}

				var fields = new List<FieldInfo>();
				while( types.Count > 0 )
				{
					fields.AddRange( types.Pop().GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) );
				}

				allFields = fields;
			}
			else
			{
				allFields = type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			}

			// cleanse the fields based on our attributes
			foreach( var field in allFields )
			{
				var shouldEncode = field.IsPublic;
				foreach( var attribute in field.GetCustomAttributes( true ) )
				{
					if( Json.excludeAttrType.IsInstanceOfType( attribute ) )
					{
						shouldEncode = false;
					}

					if( Json.includeAttrType.IsInstanceOfType( attribute ) )
					{
						shouldEncode = true;
					}
				}

				if( shouldEncode )
					yield return field;
			}
		}

		IEnumerable<PropertyInfo> GetEncodablePropertiesForType( Type type )
		{
			IEnumerable<PropertyInfo> allProps = null;
			if( _settings.EnforceHeirarchyOrderEnabled )
			{
				var types = new Stack<Type>();
				while( type != null )
				{
					types.Push( type );
					type = type.BaseType;
				}

				var properties = new List<PropertyInfo>();
				while( types.Count > 0 )
				{
					properties.AddRange( types.Pop().GetProperties( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) );
				}

				allProps = properties;
			}
			else
			{
				allProps = type.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			}

			// cleanse the fields based on our attributes
			foreach( var property in allProps )
			{
				if( property.CanRead )
				{
					var shouldEncode = true;
					foreach( var attribute in property.GetCustomAttributes( true ) )
					{
						if( Json.excludeAttrType.IsInstanceOfType( attribute ) )
						{
							shouldEncode = false;
						}

						if( Json.includeAttrType.IsInstanceOfType( attribute ) )
						{
							shouldEncode = true;
						}
					}

					if( shouldEncode )
						yield return property;
				}
			}
		}

		#endregion

		void EncodeObject( object value, bool forceTypeHint )
		{
			forceTypeHint = forceTypeHint || ( _settings.TypeNameHandling == TypeNameHandling.All || _settings.TypeNameHandling == TypeNameHandling.Objects );
			var type = value.GetType();
			WriteStartObject();

			var firstItem = true;
			if( _settings.PreserveReferencesHandling )
			{
				if( !_referenceTracker.ContainsKey( value ) )
				{
					_referenceTracker[value] = ++_referenceCounter;

					WriteValueDelimiter( firstItem );
					WriteString( Json.IdPropertyName );
					AppendColon();
					WriteString( _referenceCounter.ToString() );
					firstItem = false;
				}
				else
				{
					var id = _referenceTracker[value];

					WriteValueDelimiter( firstItem );
					WriteString( Json.RefPropertyName );
					AppendColon();
					WriteString( id.ToString() );
					WriteEndObject();
					return;
				}
			}

			if( forceTypeHint )
			{
				WriteValueDelimiter( firstItem );
				WriteString( Json.TypeHintPropertyName );
				AppendColon();
				WriteString( type.FullName );

				firstItem = false;
			}

			foreach( var field in GetEncodableFieldsForType( type ) )
			{
				WriteValueDelimiter( firstItem );
				WriteString( field.Name );
				AppendColon();
				EncodeValue( field.GetValue( value ) );
				firstItem = false;
			}

			foreach( var property in GetEncodablePropertiesForType( type ) )
			{
				if( property.CanRead )
				{
					WriteValueDelimiter( firstItem );
					WriteString( property.Name );
					AppendColon();
					EncodeValue( property.GetValue( value, null ) );
					firstItem = false;
				}
			}

			WriteEndObject();
		}

		void EncodeProxyArray( ProxyArray value )
		{
			WriteStartArray();

			var firstItem = true;
			foreach( var obj in value )
			{
				WriteValueDelimiter( firstItem );
				EncodeValue( obj );
				firstItem = false;
			}

			WriteEndArray();
		}

		void EncodeProxyObject( ProxyObject value )
		{
			WriteStartObject();

			var firstItem = true;
			foreach( var e in value.Keys )
			{
				WriteValueDelimiter( firstItem );
				WriteString( e );
				AppendColon();
				EncodeValue( value[e] );
				firstItem = false;
			}

			WriteEndObject();
		}

		void EncodeDictionary( IDictionary value )
		{
			WriteStartObject();

			var firstItem = true;
			foreach( var e in value.Keys )
			{
				WriteValueDelimiter( firstItem );
				WriteString( e.ToString() );
				AppendColon();
				EncodeValue( value[e] );
				firstItem = false;
			}

			WriteEndObject();
		}

		void EncodeList( IList value )
		{
			var forceTypeHint = _settings.TypeNameHandling == TypeNameHandling.All || _settings.TypeNameHandling == TypeNameHandling.Arrays;

			Type listItemType = null;
			// auto means we need to know the item type of the list. If our object is not the same as the list type
			// then we need to put the type hint in.
			if( !forceTypeHint && _settings.TypeNameHandling == TypeNameHandling.Auto )
			{
				var listType = value.GetType();
				foreach( Type interfaceType in listType.GetInterfaces() )
				{
					if( interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof( IList<> ) )
					{
						listItemType = listType.GetGenericArguments()[0];
						break;
					}
				}
			}

			WriteStartArray();

			var firstItem = true;
			foreach( var obj in value )
			{
				WriteValueDelimiter( firstItem );
				forceTypeHint = forceTypeHint || ( listItemType != null && listItemType != obj.GetType() );
				EncodeValue( obj, forceTypeHint );
				firstItem = false;
			}

			WriteEndArray();
		}

		void EncodeArray( Array value )
		{
			if( value.Rank == 1 )
			{
				EncodeList( value );
			}
			else
			{
				var indices = new int[value.Rank];
				EncodeArrayRank( value, 0, indices );
			}
		}

		void EncodeArrayRank( Array value, int rank, int[] indices )
		{
			WriteStartArray();

			var min = value.GetLowerBound( rank );
			var max = value.GetUpperBound( rank );

			if( rank == value.Rank - 1 )
			{
				var forceTypeHint = _settings.TypeNameHandling == TypeNameHandling.All || _settings.TypeNameHandling == TypeNameHandling.Arrays;

				Type arrayItemType = null;
				if( _settings.TypeNameHandling == TypeNameHandling.Auto || _settings.TypeNameHandling == TypeNameHandling.Arrays )
				{
					arrayItemType = value.GetType().GetElementType();
				}

				for( var i = min; i <= max; i++ )
				{
					indices[rank] = i;
					WriteValueDelimiter( i == min );
					var val = value.GetValue( indices );
					forceTypeHint = forceTypeHint || ( arrayItemType != null && arrayItemType != val.GetType() );
					EncodeValue( val, forceTypeHint );
				}
			}
			else
			{
				for( var i = min; i <= max; i++ )
				{
					indices[rank] = i;
					WriteValueDelimiter( i == min );
					EncodeArrayRank( value, rank + 1, indices );
				}
			}

			WriteEndArray();
		}


		#region Writers

		void AppendIndent()
		{
			for( var i = 0; i < indent; i++ )
			{
				_builder.Append( '\t' );
			}
		}

		void AppendColon()
		{
			_builder.Append( ':' );

			if( _settings.PrettyPrint )
			{
				_builder.Append( ' ' );
			}
		}

		void WriteValueDelimiter( bool firstItem )
		{
			if( !firstItem )
			{
				_builder.Append( ',' );

				if( _settings.PrettyPrint )
				{
					_builder.Append( '\n' );
				}
			}

			if( _settings.PrettyPrint )
			{
				AppendIndent();
			}
		}

		void WriteStartObject()
		{
			_builder.Append( '{' );

			if( _settings.PrettyPrint )
			{
				_builder.Append( '\n' );
				indent++;
			}
		}

		void WriteEndObject()
		{
			if( _settings.PrettyPrint )
			{
				_builder.Append( '\n' );
				indent--;
				AppendIndent();
			}

			_builder.Append( '}' );
		}

		void WriteStartArray()
		{
			_builder.Append( '[' );

			if( _settings.PrettyPrint )
			{
				_builder.Append( '\n' );
				indent++;
			}
		}

		void WriteEndArray()
		{
			if( _settings.PrettyPrint )
			{
				_builder.Append( '\n' );
				indent--;
				AppendIndent();
			}

			_builder.Append( ']' );
		}

		void WriteString( string value )
		{
			_builder.Append( '\"' );

			var charArray = value.ToCharArray();
			foreach( var c in charArray )
			{
				switch( c )
				{
					case '"':
						_builder.Append( "\\\"" );
						break;

					case '\\':
						_builder.Append( "\\\\" );
						break;

					case '\b':
						_builder.Append( "\\b" );
						break;

					case '\f':
						_builder.Append( "\\f" );
						break;

					case '\n':
						_builder.Append( "\\n" );
						break;

					case '\r':
						_builder.Append( "\\r" );
						break;

					case '\t':
						_builder.Append( "\\t" );
						break;

					default:
						var codepoint = Convert.ToInt32( c );
						if( ( codepoint >= 32 ) && ( codepoint <= 126 ) )
						{
							_builder.Append( c );
						}
						else
						{
							_builder.Append( "\\u" + Convert.ToString( codepoint, 16 ).PadLeft( 4, '0' ) );
						}

						break;
				}
			}

			_builder.Append( '\"' );
		}

		#endregion

	}
}
