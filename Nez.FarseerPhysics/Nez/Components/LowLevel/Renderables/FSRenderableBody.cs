using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Farseer
{
	public abstract class FSRenderableBody : RenderableComponent, IUpdatable
	{
		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds.calculateBounds( transform.position, _localOffset, _subtexture.center, transform.scale, transform.rotation, _subtexture.sourceRect.Width, _subtexture.sourceRect.Height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		public Body body;

		protected bool _ignoreTransformChanges;
		protected Subtexture _subtexture;


		protected FSRenderableBody( Subtexture subtexture )
		{
			_subtexture = subtexture;
		}


		#region Component overrides

		public override void initialize()
		{
			var world = entity.scene.getOrCreateSceneComponent<FSWorld>();
			body = new Body( world, transform.position * FSConvert.displayToSim, transform.rotation );
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			_areBoundsDirty = true;
			if( _ignoreTransformChanges )
				return;

			if( comp == Transform.Component.Position )
				body.position = transform.position * FSConvert.displayToSim;
			else if( comp == Transform.Component.Rotation )
				body.rotation = transform.rotation;
		}


		public override void onRemovedFromEntity()
		{
			if( body != null )
			{
				body.world.removeBody( body );
				body = null;
			}
		}


		void IUpdatable.update()
		{
			if( !body.isAwake )
				return;

			_ignoreTransformChanges = true;
			transform.position = FSConvert.simToDisplay * body.position;
			transform.rotation = body.rotation;
			_ignoreTransformChanges = false;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			graphics.batcher.draw( _subtexture, transform.position, _subtexture.sourceRect, color, transform.rotation, _subtexture.center, transform.scale, SpriteEffects.None, _layerDepth );
		}

		#endregion


		public FSRenderableBody setBodyType( BodyType bodyType )
		{
			body.bodyType = bodyType;
			return this;
		}


		public static implicit operator Body( FSRenderableBody self )
		{
			return self.body;
		}

	}
}
