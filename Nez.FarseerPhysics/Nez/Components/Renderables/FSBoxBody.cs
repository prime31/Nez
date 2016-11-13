using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSBoxBody : FSPolygonBody
	{
		public FSBoxBody( World world, Subtexture subtexture, float density, Vector2 position = default( Vector2 ), BodyType bodyType = BodyType.Static )
			: base( world, subtexture, PolygonTools.CreateRectangle( subtexture.sourceRect.Width / 2, subtexture.sourceRect.Height / 2 ), density, position, bodyType )
		{}
	}
}
