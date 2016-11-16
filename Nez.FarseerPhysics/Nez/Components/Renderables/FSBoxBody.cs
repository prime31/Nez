using FarseerPhysics.Common;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSBoxBody : FSPolygonBody
	{
		public FSBoxBody( Subtexture subtexture ) : base( subtexture, PolygonTools.createRectangle( subtexture.sourceRect.Width / 2, subtexture.sourceRect.Height / 2 ) )
		{}
	}
}
