using System;
using System.Collections;

namespace Nez.Persistence
{
	public interface IJsonEncoder
	{
		void EncodeValue( object value, bool forceTypeHint = false );
		void EncodeObject( object value, bool forceTypeHint );
		void EncodeDictionary( IDictionary value );
		void EncodeList( IList value );
		void EncodeArray( Array value );
		void EncodeArrayRank( Array value, int rank, int[] indices );

		void AppendIndent();
		void AppendColon();

		bool WriteOptionalReferenceData( object value );
		void WriteOptionalTypeHint( Type type, bool forceTypeHint );
		void WriteValueDelimiter();
		void WriteStartObject();
		void WriteEndObject();
		void WriteStartArray();
		void WriteEndArray();
		void WriteString( string value );
	}
}
