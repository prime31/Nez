using System;
using Nez;

namespace MacTester
{
	public class ImageEntityProcessor : EntityProcessor
	{
		public ImageEntityProcessor(Matcher matcher) : base(matcher)
		{
		}

		public override void update()
		{
			base.update();
			foreach( var entity in _entities )
			{
				Debug.log( "ImageEntityProcessor -> " + entity.name );
			}
		}

	}
}

