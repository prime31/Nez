using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
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


		protected FSRenderableBody( World world, Subtexture subtexture, Vector2 position = default( Vector2 ), BodyType bodyType = BodyType.Static )
		{
			_subtexture = subtexture;
			body = new Body( world, position * ConvertUnits.displayToSim, 0, bodyType );
		}


		public override void onAddedToEntity()
		{
			// if scale is not 1 or rotation is not 0 then trigger a scale/rotation change for the Shape
			if( transform.scale.X != 1 )
				onEntityTransformChanged( Transform.Component.Scale );
			if( transform.rotation != 0 )
				onEntityTransformChanged( Transform.Component.Rotation );
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			_areBoundsDirty = true;
			if( _ignoreTransformChanges )
				return;

			if( comp == Transform.Component.Position )
				body.Position = transform.position * ConvertUnits.displayToSim;
			else if( comp == Transform.Component.Rotation )
				body.Rotation = transform.rotation;
		}


		public override void onRemovedFromEntity()
		{
			if( body != null )
			{
				body.World.RemoveBody( body );
				body = null;
			}
		}


		void IUpdatable.update()
		{
			if( !body.Awake )
				return;
			
			_ignoreTransformChanges = true;
			transform.position = ConvertUnits.simToDisplay * body.Position;
			transform.rotation = body.Rotation;
			_ignoreTransformChanges = false;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			graphics.batcher.draw( _subtexture, transform.position, _subtexture.sourceRect, color, transform.rotation, _subtexture.center, transform.scale, SpriteEffects.None, _layerDepth );
		}


		public static implicit operator Body( FSRenderableBody self )
		{
			return self.body;
		}

	}
}
