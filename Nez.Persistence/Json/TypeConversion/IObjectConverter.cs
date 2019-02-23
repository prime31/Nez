using System;
namespace Nez.Persistence
{
	public interface IObjectConverter
	{
		object DecodeType( Variant data, Type type );
		object DecodeObject( Type type, ProxyObject data );
		object DecodeList( Type type, Variant data );
		object DecodeDictionary( Type type, Variant data );
		object DecodeArray( Type elementType, Variant data );
		void DecodeMultiRankArray( Type elementType, ProxyArray arrayData, Array array, int arrayRank, int[] indices );
	}
}
