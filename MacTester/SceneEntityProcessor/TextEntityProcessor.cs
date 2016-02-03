using System;
using Nez;


namespace MacDumpster
{
	public class TextEntityProcessor : EntityProcessingSystem
	{
		public TextEntityProcessor( Matcher matcher ) : base( matcher )
		{}


		public override void process( Entity entity )
		{
			Debug.log( "TextEntityProcessor -> " + entity.name );
		}
	}
}

