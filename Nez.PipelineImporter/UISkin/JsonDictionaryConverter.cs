using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace Nez.UISkinImporter
{
	class JsonDictionaryConverter : CustomCreationConverter<IDictionary<string, object>>
	{

		public override IDictionary<string, object> Create( Type objectType )
		{
			return new Dictionary<string, object>();
		}

		public override bool CanConvert( Type objectType )
		{
			return objectType == typeof( object ) || base.CanConvert( objectType );
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			if( reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.Null )
				return base.ReadJson( reader, objectType, existingValue, serializer );

			return serializer.Deserialize( reader );
		}
	}
}
