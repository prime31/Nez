using System;
using Nez;
using System.Collections.Generic;


namespace MacDumpster
{
	public class ImageEntityProcessor : EntityProcessingSystem
	{
		public ImageEntityProcessor( Matcher matcher ) : base( matcher )
		{}


		public override void process( Entity entity )
		{
			Debug.log( "ImageEntityProcessor -> " + entity.name );
		}

	}
}

