using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSCircleBody : FSRenderableBody
	{
		public FSCircleBody( Subtexture subtexture ) : base( subtexture )
		{ }


		public FSCircleBody( Texture2D texture ) : this( new Subtexture( texture ) )
		{ }


		public override void initialize()
		{
			base.initialize();
			body.attachCircle( _subtexture.sourceRect.Width / 2, 1 );
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			base.onEntityTransformChanged( comp );
			if( _ignoreTransformChanges )
				return;

			// we only care about scale. base handles pos/rot
			if( comp == Transform.Component.Scale )
				body.fixtureList[0].shape.radius = _subtexture.sourceRect.Width * transform.scale.X * 0.5f * FSConvert.displayToSim;
		}

	}
}
