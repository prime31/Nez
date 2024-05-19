using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Note that this is not a full, multi-iteration physics system! This can be used for simple, arcade style physics.
	/// Based on http://elancev.name/oliver/2D%20polygon.htm#tut5
	/// </summary>
	public class ArcadeRigidbody : Component, IUpdatable
	{
		/// <summary>
		/// mass of this rigidbody. A 0 mass will make this an immovable object.
		/// </summary>
		/// <value>The mass.</value>
		public float Mass
		{
			get => _mass;
			set => SetMass(value);
		}

		/// <summary>
		/// 0 - 1 range where 0 is no bounce and 1 is perfect reflection
		/// </summary>
		public float Elasticity
		{
			get => _elasticity;
			set => SetElasticity(value);
		}

		/// <summary>
		/// 0 - 1 range. 0 means no friction, 1 means the object will stop dead on
		/// </summary>
		public float Friction
		{
			get => _friction;
			set => SetFriction(value);
		}

		/// <summary>
		/// 0 - 9 range. When a collision occurs and it has risidual motion along the surface of collision if its square magnitude is less
		/// than glue friction will be set to the maximum for the collision resolution.
		/// </summary>
		public float Glue
		{
			get => _glue;
			set => SetGlue(value);
		}

		/// <summary>
		/// if true, Physics.gravity will be taken into account each frame
		/// </summary>
		public bool ShouldUseGravity = true;

		/// <summary>
		/// velocity of this rigidbody
		/// </summary>
		public Vector2 Velocity;

		/// <summary>
		/// rigidbodies with a mass of 0 are considered immovable. Changing velocity and collisions will have no effect on them.
		/// </summary>
		/// <value><c>true</c> if is immovable; otherwise, <c>false</c>.</value>
		public bool IsImmovable => _mass < 0.0001f;

		float _mass = 10f;
		float _elasticity = 0.5f;
		float _friction = 0.5f;
		float _glue = 0.01f;
		float _inverseMass;
		Collider _collider;


		public ArcadeRigidbody()
		{
			_inverseMass = 1 / _mass;
		}


		#region Fluent setters

		/// <summary>
		/// mass of this rigidbody. A 0 mass will make this an immovable object.
		/// </summary>
		/// <returns>The mass.</returns>
		/// <param name="mass">Mass.</param>
		public ArcadeRigidbody SetMass(float mass)
		{
			_mass = Mathf.Clamp(mass, 0, float.MaxValue);

			if (_mass > 0.0001f)
				_inverseMass = 1 / _mass;
			else
				_inverseMass = 0f;
			return this;
		}

		/// <summary>
		/// 0 - 1 range where 0 is no bounce and 1 is perfect reflection
		/// </summary>
		/// <returns>The elasticity.</returns>
		/// <param name="value">Value.</param>
		public ArcadeRigidbody SetElasticity(float value)
		{
			_elasticity = Mathf.Clamp01(value);
			return this;
		}

		/// <summary>
		/// 0 - 1 range. 0 means no friction, 1 means the object will stop dead on
		/// </summary>
		/// <returns>The friction.</returns>
		/// <param name="value">Value.</param>
		public ArcadeRigidbody SetFriction(float value)
		{
			_friction = Mathf.Clamp01(value);
			return this;
		}

		/// <summary>
		/// 0 - 9 range. When a collision occurs and it has risidual motion along the surface of collision if its square magnitude is less
		/// than glue friction will be set to the maximum for the collision resolution.
		/// </summary>
		/// <returns>The glue.</returns>
		/// <param name="value">Value.</param>
		public ArcadeRigidbody SetGlue(float value)
		{
			_glue = Mathf.Clamp(value, 0, 10);
			return this;
		}

		/// <summary>
		/// velocity of this rigidbody
		/// </summary>
		/// <returns>The velocity.</returns>
		/// <param name="velocity">Velocity.</param>
		public ArcadeRigidbody SetVelocity(Vector2 velocity)
		{
			Velocity = velocity;
			return this;
		}

		#endregion


		/// <summary>
		/// add an instant force impulse to the rigidbody using its mass. force is an acceleration in pixels per second per second. The
		/// force is multiplied by 100000 to make the values more reasonable to use.
		/// </summary>
		/// <param name="force">Force.</param>
		public void AddImpulse(Vector2 force)
		{
			if (!IsImmovable)
				Velocity += force * 100000 * (_inverseMass * Time.DeltaTime * Time.DeltaTime);
		}

		public override void OnAddedToEntity()
		{
			_collider = Entity.GetComponent<Collider>();
			Debug.WarnIf(_collider == null, "ArcadeRigidbody has no Collider. ArcadeRigidbody requires a Collider!");
		}

		public virtual void Update()
		{
			if (IsImmovable || _collider == null)
			{
				Velocity = Vector2.Zero;
				return;
			}

			if (ShouldUseGravity)
				Velocity += Physics.Gravity * Time.DeltaTime;

			Entity.Transform.Position += Velocity * Time.DeltaTime;

			CollisionResult collisionResult;

			// fetch anything that we might collide with at our new position
			var neighbors = Physics.BoxcastBroadphaseExcludingSelf(_collider, _collider.CollidesWithLayers);
			foreach (var neighbor in neighbors)
			{
				// if the neighbor collider is of the same entity, ignore it
				if (neighbor.Entity == Entity)
				{
					continue;
				}

				if (!_collider.IsTrigger && !neighbor.IsTrigger)
					if (_collider.CollidesWith(neighbor, out collisionResult))
					{
						// if the neighbor has an ArcadeRigidbody we handle full collision response. If not, we calculate things based on the
						// neighbor being immovable.
						var neighborRigidbody = neighbor.Entity.GetComponent<ArcadeRigidbody>();
						if (neighborRigidbody != null)
						{
							ProcessOverlap(neighborRigidbody, ref collisionResult.MinimumTranslationVector);
							ProcessCollision(neighborRigidbody, ref collisionResult.MinimumTranslationVector);
						}
						else
						{
							// neighbor has no ArcadeRigidbody so we assume its immovable and only move ourself
							Entity.Transform.Position -= collisionResult.MinimumTranslationVector;
							var relativeVelocity = Velocity;
							CalculateResponseVelocity(ref relativeVelocity, ref collisionResult.MinimumTranslationVector,
								out relativeVelocity);
							Velocity += relativeVelocity;
						}
				}
			}
		}

		/// <summary>
		/// separates two overlapping rigidbodies. Handles the case of either being immovable as well.
		/// </summary>
		/// <param name="other">Other.</param>
		/// <param name="minimumTranslationVector"></param>
		void ProcessOverlap(ArcadeRigidbody other, ref Vector2 minimumTranslationVector)
		{
			if (IsImmovable)
			{
				other.Entity.Transform.Position += minimumTranslationVector;
			}
			else if (other.IsImmovable)
			{
				Entity.Transform.Position -= minimumTranslationVector;
			}
			else
			{
				Entity.Transform.Position -= minimumTranslationVector * 0.5f;
				other.Entity.Transform.Position += minimumTranslationVector * 0.5f;
			}
		}

		/// <summary>
		/// handles the collision of two non-overlapping rigidbodies. New velocities will be assigned to each rigidbody as appropriate.
		/// </summary>
		/// <param name="other">Other.</param>
		/// <param name="inverseMTV">Inverse MT.</param>
		void ProcessCollision(ArcadeRigidbody other, ref Vector2 minimumTranslationVector)
		{
			// we compute a response for the two colliding objects. The calculations are based on the relative velocity of the objects
			// which gets reflected along the collided surface normal. Then a part of the response gets added to each object based on mass.
			var relativeVelocity = Velocity - other.Velocity;

			CalculateResponseVelocity(ref relativeVelocity, ref minimumTranslationVector, out relativeVelocity);

			// now we use the masses to linearly scale the response on both rigidbodies
			var totalInverseMass = _inverseMass + other._inverseMass;
			var ourResponseFraction = _inverseMass / totalInverseMass;
			var otherResponseFraction = other._inverseMass / totalInverseMass;

			Velocity += relativeVelocity * ourResponseFraction;
			other.Velocity -= relativeVelocity * otherResponseFraction;
		}

		/// <summary>
		/// given the relative velocity between the two objects and the MTV this method modifies the relativeVelocity to make it a collision
		/// response.
		/// </summary>
		/// <param name="relativeVelocity">Relative velocity.</param>
		/// <param name="minimumTranslationVector">Minimum translation vector.</param>
		void CalculateResponseVelocity(ref Vector2 relativeVelocity, ref Vector2 minimumTranslationVector,
									   out Vector2 responseVelocity)
		{
			// first, we get the normalized MTV in the opposite direction: the surface normal
			var inverseMTV = minimumTranslationVector * -1f;
			Vector2 normal;
			Vector2.Normalize(ref inverseMTV, out normal);

			// the velocity is decomposed along the normal of the collision and the plane of collision.
			// The elasticity will affect the response along the normal (normalVelocityComponent) and the friction will affect
			// the tangential component of the velocity (tangentialVelocityComponent)
			float n;
			Vector2.Dot(ref relativeVelocity, ref normal, out n);

			var normalVelocityComponent = normal * n;
			var tangentialVelocityComponent = relativeVelocity - normalVelocityComponent;

			if (n > 0.0f)
				normalVelocityComponent = Vector2.Zero;

			// if the squared magnitude of the tangential component is less than glue then we bump up the friction to the max
			var coefficientOfFriction = _friction;
			if (tangentialVelocityComponent.LengthSquared() < _glue)
				coefficientOfFriction = 1.01f;

			// elasticity affects the normal component of the velocity and friction affects the tangential component
			responseVelocity = -(1.0f + _elasticity) * normalVelocityComponent -
							   coefficientOfFriction * tangentialVelocityComponent;
		}
	}
}
