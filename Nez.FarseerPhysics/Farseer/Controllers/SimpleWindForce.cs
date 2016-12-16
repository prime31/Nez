using Microsoft.Xna.Framework;


namespace FarseerPhysics.Controllers
{
	/// <summary>
	/// Reference implementation for forces based on AbstractForceController
	/// It supports all features provided by the base class and illustrates proper
	/// usage as an easy to understand example.
	/// As a side-effect it is a nice and easy to use wind force for your projects
	/// </summary>
	public class SimpleWindForce : AbstractForceController
	{
		/// <summary>
		/// Direction of the windforce
		/// </summary>
		public Vector2 direction;

		/// <summary>
		/// The amount of Direction randomization. Allowed range is 0-1.
		/// </summary>
		public float divergence;

		/// <summary>
		/// Ignore the position and apply the force. If off only in the "front" (relative to position and direction)
		/// will be affected
		/// </summary>
		public bool ignorePosition;


		public override void applyForce( float dt, float strength )
		{
			foreach( var body in world.bodyList )
			{
				//TODO: Consider Force Type
				float decayMultiplier = getDecayMultiplier( body );
				if( decayMultiplier != 0 )
				{
					Vector2 forceVector;
					if( forceType == ForceTypes.Point )
					{
						forceVector = body.position - position;
					}
					else
					{
						Nez.Vector2Ext.normalize( ref direction );
						forceVector = direction;

						if( forceVector.Length() == 0 )
							forceVector = new Vector2( 0, 1 );
					}

					//TODO: Consider Divergence:
					//forceVector = Vector2.Transform(forceVector, Matrix.CreateRotationZ((MathHelper.Pi - MathHelper.Pi/2) * (float)Randomize.NextDouble()));

					// Calculate random Variation
					if( variation != 0 )
					{
						var strengthVariation = (float)randomize.NextDouble() * MathHelper.Clamp( variation, 0, 1 );
						Nez.Vector2Ext.normalize( ref forceVector );
						body.applyForce( forceVector * strength * decayMultiplier * strengthVariation );
					}
					else
					{
						Nez.Vector2Ext.normalize( ref forceVector );
						body.applyForce( forceVector * strength * decayMultiplier );
					}
				}
			}
		}

	}
}