using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSCircleBody : FSRenderableBody
	{
		public FSCircleBody( World world, Subtexture subtexture, float density, Vector2 position = default( Vector2 ), BodyType bodyType = BodyType.Static )
			: base( world, subtexture, position, bodyType )
		{
			Farseer.FixtureFactory.attachCircle( _subtexture.sourceRect.Width / 2, density, body );
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			base.onEntityTransformChanged( comp );
			if( _ignoreTransformChanges )
				return;

			// we only care about scale. base handles pos/rot
			if( comp == Transform.Component.Scale )
				body.FixtureList[0].Shape.Radius = _subtexture.sourceRect.Width * transform.scale.X * 0.5f * ConvertUnits.displayToSim;
		}

	}
}
