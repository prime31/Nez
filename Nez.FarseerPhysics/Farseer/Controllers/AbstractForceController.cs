using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Controllers
{
	public abstract class AbstractForceController : Controller
	{
		#region DecayModes enum

		/// <summary>
		/// Modes for Decay. Actual Decay must be implemented in inheriting 
		/// classes
		/// </summary>
		public enum DecayModes
		{
			None,
			Step,
			Linear,
			InverseSquare,
			Curve
		}

		#endregion

		#region ForceTypes enum

		/// <summary>
		/// Forcetypes are used in the decay math to properly get the distance.
		/// They are also used to draw a representation in DebugView
		/// </summary>
		public enum ForceTypes
		{
			Point,
			Line,
			Area
		}

		#endregion

		#region TimingModes enum

		/// <summary>
		/// Timing Modes
		/// Switched: Standard on/off mode using the baseclass enabled property
		/// Triggered: When the Trigger() method is called the force is active 
		/// for a specified Impulse Length
		/// Curve: Still to be defined. The basic idea is having a Trigger 
		/// combined with a curve for the strength
		/// </summary>
		public enum TimingModes
		{
			Switched,
			Triggered,
			Curve
		}

		#endregion

		/// <summary>
		/// Curve to be used for Decay in Curve mode
		/// </summary>
		public Curve decayCurve;

		/// <summary>
		/// The Forcetype of the instance
		/// </summary>
		public ForceTypes forceType;

		/// <summary>
		/// Provided for reuse to provide Variation functionality in 
		/// inheriting classes
		/// </summary>
		protected Random randomize;

		/// <summary>
		/// Curve used by Curve Mode as an animated multiplier for the force 
		/// strength.
		/// Only positions between 0 and 1 are considered as that range is 
		/// stretched to have ImpulseLength.
		/// </summary>
		public Curve strengthCurve;


		/// <summary>
		/// Constructor
		/// </summary>
		protected AbstractForceController() : base( ControllerType.AbstractForceController )
		{
			enabled = true;

			strength = 1.0f;
			position = new Vector2( 0, 0 );
			maximumSpeed = 100.0f;
			timingMode = TimingModes.Switched;
			impulseTime = 0.0f;
			impulseLength = 1.0f;
			triggered = false;
			strengthCurve = new Curve();
			variation = 0.0f;
			randomize = new Random( 1234 );
			decayMode = DecayModes.None;
			decayCurve = new Curve();
			decayStart = 0.0f;
			decayEnd = 0.0f;

			strengthCurve.Keys.Add( new CurveKey( 0, 5 ) );
			strengthCurve.Keys.Add( new CurveKey( 0.1f, 5 ) );
			strengthCurve.Keys.Add( new CurveKey( 0.2f, -4 ) );
			strengthCurve.Keys.Add( new CurveKey( 1f, 0 ) );
		}

		/// <summary>
		/// Overloaded Contstructor with supplying Timing Mode
		/// </summary>
		/// <param name="mode"></param>
		public AbstractForceController( TimingModes mode )
			: base( ControllerType.AbstractForceController )
		{
			timingMode = mode;
			switch( mode )
			{
				case TimingModes.Switched:
					enabled = true;
					break;
				case TimingModes.Triggered:
					enabled = false;
					break;
				case TimingModes.Curve:
					enabled = false;
					break;
			}
		}

		/// <summary>
		/// Global Strength of the force to be applied
		/// </summary>
		public float strength;

		/// <summary>
		/// Position of the Force. Can be ignored (left at (0,0) for forces
		/// that are not position-dependent
		/// </summary>
		public Vector2 position { get; set; }

		/// <summary>
		/// Maximum speed of the bodies. Bodies that are travelling faster are
		/// supposed to be ignored
		/// </summary>
		public float maximumSpeed;

		/// <summary>
		/// Maximum Force to be applied. As opposed to Maximum Speed this is 
		/// independent of the velocity of
		/// the affected body
		/// </summary>
		public float maximumForce;

		/// <summary>
		/// Timing Mode of the force instance
		/// </summary>
		public TimingModes timingMode;

		/// <summary>
		/// Time of the current impulse. Incremented in update till 
		/// ImpulseLength is reached
		/// </summary>
		public float impulseTime { get; private set; }

		/// <summary>
		/// Length of a triggered impulse. Used in both Triggered and Curve Mode
		/// </summary>
		public float impulseLength;

		/// <summary>
		/// Indicating if we are currently during an Impulse 
		/// (Triggered and Curve Mode)
		/// </summary>
		public bool triggered { get; private set; }

		/// <summary>
		/// Variation of the force applied to each body affected
		/// !! Must be used in inheriting classes properly !!
		/// </summary>
		public float variation;

		/// <summary>
		/// See DecayModes
		/// </summary>
		public DecayModes decayMode;

		/// <summary>
		/// Start of the distance based Decay. To set a non decaying area
		/// </summary>
		public float decayStart;

		/// <summary>
		/// Maximum distance a force should be applied
		/// </summary>
		public float decayEnd;


		/// <summary>
		/// Calculate the Decay for a given body. Meant to ease force 
		/// development and stick to the DRY principle and provide unified and 
		/// predictable decay math.
		/// </summary>
		/// <param name="body">The body to calculate decay for</param>
		/// <returns>A multiplier to multiply the force with to add decay 
		/// support in inheriting classes</returns>
		protected float getDecayMultiplier( Body body )
		{
			//TODO: Consider ForceType in distance calculation!
			float distance = ( body.position - position ).Length();
			switch( decayMode )
			{
				case DecayModes.None:
					{
						return 1.0f;
					}
				case DecayModes.Step:
					{
						if( distance < decayEnd )
							return 1.0f;
						else
							return 0.0f;
					}
				case DecayModes.Linear:
					{
						if( distance < decayStart )
							return 1.0f;
						if( distance > decayEnd )
							return 0.0f;
						return ( decayEnd - decayStart / distance - decayStart );
					}
				case DecayModes.InverseSquare:
					{
						if( distance < decayStart )
							return 1.0f;
						else
							return 1.0f / ( ( distance - decayStart ) * ( distance - decayStart ) );
					}
				case DecayModes.Curve:
					{
						if( distance < decayStart )
							return 1.0f;
						else
							return decayCurve.Evaluate( distance - decayStart );
					}
				default:
					return 1.0f;
			}
		}

		/// <summary>
		/// Triggers the trigger modes (Trigger and Curve)
		/// </summary>
		public void trigger()
		{
			triggered = true;
			impulseTime = 0;
		}

		/// <summary>
		/// Inherited from Controller
		/// Depending on the TimingMode perform timing logic and call ApplyForce()
		/// </summary>
		/// <param name="dt"></param>
		public override void update( float dt )
		{
			switch( timingMode )
			{
				case TimingModes.Switched:
					{
						if( enabled )
						{
							applyForce( dt, strength );
						}
						break;
					}
				case TimingModes.Triggered:
					{
						if( enabled && triggered )
						{
							if( impulseTime < impulseLength )
							{
								applyForce( dt, strength );
								impulseTime += dt;
							}
							else
							{
								triggered = false;
							}
						}
						break;
					}
				case TimingModes.Curve:
					{
						if( enabled && triggered )
						{
							if( impulseTime < impulseLength )
							{
								applyForce( dt, strength * strengthCurve.Evaluate( impulseTime ) );
								impulseTime += dt;
							}
							else
							{
								triggered = false;
							}
						}
						break;
					}
			}
		}

		/// <summary>
		/// Apply the force supplying strength (wich is modified in Update() 
		/// according to the TimingMode
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="strength">The strength</param>
		public abstract void applyForce( float dt, float strength );
	
	}
}