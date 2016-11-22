using FarseerPhysics.Common;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSBoxBody : FSPolygonBody
	{
		public FSBoxBody( Subtexture subtexture ) : base( subtexture, PolygonTools.createRectangle( subtexture.sourceRect.Width / 2, subtexture.sourceRect.Height / 2 ) )
		{ }


		public FSBoxBody( Texture2D texture ) : this( new Subtexture( texture ) )
		{ }
	}
}
