namespace Nez.Verlet
{
	public class Ragdoll : Composite
	{
		public Ragdoll(float x, float y, float bodyHeight)
		{
			var headLength = bodyHeight / 7.5f;

			var head = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			head.Radius = headLength * 0.75f;
			head.Mass = 4;
			var shoulder = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			shoulder.Mass = 26; // shoulder to torso
			AddConstraint(new DistanceConstraint(head, shoulder, 1f, 5 / 4 * headLength));

			//head.attachTo( shoulder, 5 / 4 * headLength, 1, bodyHeight * 2, true );

			var elbowLeft = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			var elbowRight = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			elbowLeft.Mass = 2; // upper arm mass
			elbowRight.Mass = 2;
			AddConstraint(new DistanceConstraint(elbowLeft, shoulder, 1, headLength * 3 / 2));
			AddConstraint(new DistanceConstraint(elbowRight, shoulder, 1, headLength * 3 / 2));

			//elbowLeft.attachTo( shoulder, headLength * 3 / 2, 1, bodyHeight * 2, true );
			//elbowRight.attachTo( shoulder, headLength * 3 / 2, 1, bodyHeight * 2, true );

			var handLeft = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			var handRight = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			handLeft.Mass = 2;
			handRight.Mass = 2;
			AddConstraint(new DistanceConstraint(handLeft, elbowLeft, 1, headLength * 2));
			AddConstraint(new DistanceConstraint(handRight, elbowRight, 1, headLength * 2));

			//handLeft.attachTo( elbowLeft, headLength * 2, 1, bodyHeight * 2, true );
			//handRight.attachTo( elbowRight, headLength * 2, 1, bodyHeight * 2, true );

			var pelvis = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			pelvis.Mass = 15; // pelvis to lower torso
			AddConstraint(new DistanceConstraint(pelvis, shoulder, 0.8f, headLength * 3.5f));

			//pelvis.attachTo( shoulder, headLength * 3.5f, 0.8f, bodyHeight * 2f, true );
			// this restraint keeps the head from tilting in extremely uncomfortable positions
			AddConstraint(new DistanceConstraint(pelvis, head, 0.02f, bodyHeight * 2))
				.SetCollidesWithColliders(false);

			//pelvis.attachTo( head, headLength * 4.75f, 0.02f, bodyHeight * 2, false );

			var kneeLeft = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			var kneeRight = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			kneeLeft.Mass = 10;
			kneeRight.Mass = 10;
			AddConstraint(new DistanceConstraint(kneeLeft, pelvis, 1, headLength * 2));
			AddConstraint(new DistanceConstraint(kneeRight, pelvis, 1, headLength * 2));

			//kneeLeft.attachTo( pelvis, headLength * 2, 1, bodyHeight * 2, true );
			//kneeRight.attachTo( pelvis, headLength * 2, 1, bodyHeight * 2, true );

			var footLeft = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			var footRight = AddParticle(new Particle(x + Random.Range(-5, 5), y + Random.Range(-5, 5)));
			footLeft.Mass = 5; // calf + foot
			footRight.Mass = 5;
			AddConstraint(new DistanceConstraint(footLeft, kneeLeft, 1, headLength * 2));
			AddConstraint(new DistanceConstraint(footRight, kneeRight, 1, headLength * 2));

			//footLeft.attachTo( kneeLeft, headLength * 2, 1, bodyHeight * 2, true );
			//footRight.attachTo( kneeRight, headLength * 2, 1, bodyHeight * 2, true );

			// these constraints resist flexing the legs too far up towards the body
			AddConstraint(new DistanceConstraint(footLeft, shoulder, 0.001f, bodyHeight * 2))
				.SetCollidesWithColliders(false);
			AddConstraint(new DistanceConstraint(footRight, shoulder, 0.001f, bodyHeight * 2))
				.SetCollidesWithColliders(false);

			//footLeft.attachTo( shoulder, headLength * 7.5f, 0.001f, bodyHeight * 2, false );
			//footRight.attachTo( shoulder, headLength * 7.5f, 0.001f, bodyHeight * 2, false );

			//var headCircle = new Ball( headLength * 0.75f );
			//headCircle.attachToPointMass( head );
		}
	}
}