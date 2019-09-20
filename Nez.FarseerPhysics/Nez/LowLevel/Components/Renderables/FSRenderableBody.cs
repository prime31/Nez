using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Farseer
{
	public abstract class FSRenderableBody : RenderableComponent, IUpdatable
	{
		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					_bounds.CalculateBounds(Transform.Position, _localOffset, Sprite.Center, Transform.Scale,
						Transform.Rotation, Sprite.SourceRect.Width, Sprite.SourceRect.Height);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		public Body Body;

		protected bool _ignoreTransformChanges;
		protected Sprite Sprite;


		protected FSRenderableBody(Sprite sprite)
		{
			Sprite = sprite;
		}


		#region Component overrides

		public override void Initialize()
		{
			var world = Entity.Scene.GetOrCreateSceneComponent<FSWorld>();
			Body = new Body(world, Transform.Position * FSConvert.DisplayToSim, Transform.Rotation);
		}


		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			_areBoundsDirty = true;
			if (_ignoreTransformChanges)
				return;

			if (comp == Transform.Component.Position)
				Body.Position = Transform.Position * FSConvert.DisplayToSim;
			else if (comp == Transform.Component.Rotation)
				Body.Rotation = Transform.Rotation;
		}


		public override void OnRemovedFromEntity()
		{
			if (Body != null)
			{
				Body.World.RemoveBody(Body);
				Body = null;
			}
		}


		void IUpdatable.Update()
		{
			if (!Body.IsAwake)
				return;

			_ignoreTransformChanges = true;
			Transform.Position = FSConvert.SimToDisplay * Body.Position;
			Transform.Rotation = Body.Rotation;
			_ignoreTransformChanges = false;
		}


		public override void Render(Batcher batcher, Camera camera)
		{
			batcher.Draw(Sprite, Transform.Position, Sprite.SourceRect, Color, Transform.Rotation,
				Sprite.Center, Transform.Scale, SpriteEffects.None, _layerDepth);
		}

		#endregion


		public FSRenderableBody SetBodyType(BodyType bodyType)
		{
			Body.BodyType = bodyType;
			return this;
		}


		public static implicit operator Body(FSRenderableBody self)
		{
			return self.Body;
		}
	}
}