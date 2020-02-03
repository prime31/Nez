using FarseerPhysics.Common;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSBoxBody : FSPolygonBody
	{
		public FSBoxBody(Sprite sprite) : base(sprite,
			PolygonTools.CreateRectangle(sprite.SourceRect.Width / 2, sprite.SourceRect.Height / 2))
		{
		}


		public FSBoxBody(Texture2D texture) : this(new Sprite(texture))
		{
		}
	}
}