namespace Nez.Verlet
{
	public class Ragdoll : Composite
	{
		public Ragdoll( float x, float y, float bodyHeight )
		{
			var headLength = bodyHeight / 7.5f;

			var head = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			head.radius = headLength * 0.75f;
			head.mass = 4;
			var shoulder = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			shoulder.mass = 26; // shoulder to torso
			addConstraint( new DistanceConstraint( head, shoulder, 1f, 5 / 4 * headLength ) );
			//head.attachTo( shoulder, 5 / 4 * headLength, 1, bodyHeight * 2, true );

			var elbowLeft = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			var elbowRight = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			elbowLeft.mass = 2; // upper arm mass
			elbowRight.mass = 2;
			addConstraint( new DistanceConstraint( elbowLeft, shoulder, 1, headLength * 3 / 2 ) );
			addConstraint( new DistanceConstraint( elbowRight, shoulder, 1, headLength * 3 / 2 ) );
			//elbowLeft.attachTo( shoulder, headLength * 3 / 2, 1, bodyHeight * 2, true );
			//elbowRight.attachTo( shoulder, headLength * 3 / 2, 1, bodyHeight * 2, true );

			var handLeft = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			var handRight = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			handLeft.mass = 2;
			handRight.mass = 2;
			addConstraint( new DistanceConstraint( handLeft, elbowLeft, 1, headLength * 2 ) );
			addConstraint( new DistanceConstraint( handRight, elbowRight, 1, headLength * 2 ) );
			//handLeft.attachTo( elbowLeft, headLength * 2, 1, bodyHeight * 2, true );
			//handRight.attachTo( elbowRight, headLength * 2, 1, bodyHeight * 2, true );

			var pelvis = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			pelvis.mass = 15; // pelvis to lower torso
			addConstraint( new DistanceConstraint( pelvis, shoulder, 0.8f, headLength * 3.5f ) );
			//pelvis.attachTo( shoulder, headLength * 3.5f, 0.8f, bodyHeight * 2f, true );
			// this restraint keeps the head from tilting in extremely uncomfortable positions
			addConstraint( new DistanceConstraint( pelvis, head, 0.02f, bodyHeight * 2 ) )
				.setCollidesWithColliders( false );
			//pelvis.attachTo( head, headLength * 4.75f, 0.02f, bodyHeight * 2, false );

			var kneeLeft = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			var kneeRight = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			kneeLeft.mass = 10;
			kneeRight.mass = 10;
			addConstraint( new DistanceConstraint( kneeLeft, pelvis, 1, headLength * 2 ) );
			addConstraint( new DistanceConstraint( kneeRight, pelvis, 1, headLength * 2 ) );
			//kneeLeft.attachTo( pelvis, headLength * 2, 1, bodyHeight * 2, true );
			//kneeRight.attachTo( pelvis, headLength * 2, 1, bodyHeight * 2, true );

			var footLeft = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			var footRight = addParticle( new Particle( x + Random.range( -5, 5 ), y + Random.range( -5, 5 ) ) );
			footLeft.mass = 5; // calf + foot
			footRight.mass = 5;
			addConstraint( new DistanceConstraint( footLeft, kneeLeft, 1, headLength * 2 ) );
			addConstraint( new DistanceConstraint( footRight, kneeRight, 1, headLength * 2 ) );
			//footLeft.attachTo( kneeLeft, headLength * 2, 1, bodyHeight * 2, true );
			//footRight.attachTo( kneeRight, headLength * 2, 1, bodyHeight * 2, true );

			// these constraints resist flexing the legs too far up towards the body
			addConstraint( new DistanceConstraint( footLeft, shoulder, 0.001f, bodyHeight * 2 ) )
				.setCollidesWithColliders( false );
			addConstraint( new DistanceConstraint( footRight, shoulder, 0.001f, bodyHeight * 2 ) )
				.setCollidesWithColliders( false );
			//footLeft.attachTo( shoulder, headLength * 7.5f, 0.001f, bodyHeight * 2, false );
			//footRight.attachTo( shoulder, headLength * 7.5f, 0.001f, bodyHeight * 2, false );

			//var headCircle = new Ball( headLength * 0.75f );
			//headCircle.attachToPointMass( head );
		}
	}
}
