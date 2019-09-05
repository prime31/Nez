using FarseerPhysics.Common;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSBoxBody : FSPolygonBody
	{
		public FSBoxBody(Subtexture subtexture) : base(subtexture,
			PolygonTools.CreateRectangle(subtexture.SourceRect.Width / 2, subtexture.SourceRect.Height / 2))
		{
		}


		public FSBoxBody(Texture2D texture) : this(new Subtexture(texture))
		{
		}
	}
}