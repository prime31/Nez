using FarseerPhysics.Dynamics;


namespace Nez.Farseer
{
	/// <summary>
	/// simple Component that will sync the position/rotation of the physics Body with the Transform. The GenericBody.transform.position
	/// will always match Body.Position. Note that scale is not considered here.
	/// </summary>
	public class FSGenericBody : Component, IUpdatable
	{
		public Body Body;

		bool _ignoreTransformChanges;


		public FSGenericBody()
		{
		}


		/// <summary>
		/// creates with a preexisting Body. Be aware that the Transform.position will be updated to match the Body.position.
		/// </summary>
		/// <param name="body">Body.</param>
		public FSGenericBody(Body body)
		{
			this.Body = body;
		}


		public override void Initialize()
		{
			var world = Entity.Scene.GetOrCreateSceneComponent<FSWorld>();

			// always sync position ASAP in case any joints are added without global constraints
			if (Body != null)
				((IUpdatable) this).Update();
			else
				Body = new Body(world, Transform.Position * FSConvert.DisplayToSim, Transform.Rotation);
		}


		public override void OnEntityTransformChanged(Transform.Component comp)
		{
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


		public FSGenericBody SetBodyType(BodyType bodyType)
		{
			Body.BodyType = bodyType;
			return this;
		}


		public static implicit operator Body(FSGenericBody self)
		{
			return self.Body;
		}
	}
}