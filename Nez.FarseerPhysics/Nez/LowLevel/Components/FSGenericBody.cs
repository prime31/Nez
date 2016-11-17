using FarseerPhysics.Dynamics;


namespace Nez.Farseer
{
	/// <summary>
	/// simple Component that will sync the position/rotation of the physics Body with the Transform. The GenericBody.transform.position
	/// will always match Body.Position. Note that scale is not considered here.
	/// </summary>
	public class FSGenericBody : Component, IUpdatable
	{
		public Body body;

		bool _ignoreTransformChanges;


		public FSGenericBody()
		{ }


		/// <summary>
		/// creates with a preexisting Body. Be aware that the Transform.position will be updated to match the Body.position.
		/// </summary>
		/// <param name="body">Body.</param>
		public FSGenericBody( Body body )
		{
			this.body = body;
		}


		public override void initialize()
		{
			var world = entity.scene.getOrCreateSceneComponent<FSWorld>();

			// always sync position ASAP in case any joints are added without global constraints
			if( body != null )
				( (IUpdatable)this ).update();
			else
				body = new Body( world, transform.position * FSConvert.displayToSim, transform.rotation );
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
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


		public FSGenericBody setBodyType( BodyType bodyType )
		{
			body.bodyType = bodyType;
			return this;
		}


		public static implicit operator Body( FSGenericBody self )
		{
			return self.body;
		}

	}
}
