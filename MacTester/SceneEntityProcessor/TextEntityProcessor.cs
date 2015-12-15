using System;
using Nez;


namespace MacTester
{
	public class TextEntityProcessor : EntityProcessor
	{
		public TextEntityProcessor(Matcher matcher) : base(matcher)
		{
		}

		public override void update()
		{
			base.update();
			foreach( var entity in _entities )
			{
				Debug.log( "TextEntityProcessor -> " + entity.name );
			}
		}
	}
}

