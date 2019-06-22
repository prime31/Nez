using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	[XmlRoot( "particleEmitterConfig" )]
	public class ParticleDesignerEmitterConfig
	{
		[XmlElement]
		public ParticleDesignerVector2 SourcePosition;

		[XmlElement]
		public ParticleDesignerVector2 SourcePositionVariance;

		[XmlElement]
		public ParticleDesignerFloatValue Speed;

		[XmlElement]
		public ParticleDesignerFloatValue SpeedVariance;

		[XmlElement]
		public ParticleDesignerFloatValue ParticleLifeSpan;

		[XmlElement]
		public ParticleDesignerFloatValue ParticleLifespanVariance;

		[XmlElement]
		public ParticleDesignerFloatValue Angle;

		[XmlElement]
		public ParticleDesignerFloatValue AngleVariance;

		[XmlElement]
		public ParticleDesignerVector2 Gravity;

		[XmlElement]
		public ParticleDesignerFloatValue RadialAcceleration;

		[XmlElement]
		public ParticleDesignerFloatValue TangentialAcceleration;

		[XmlElement]
		public ParticleDesignerFloatValue RadialAccelVariance;

		[XmlElement]
		public ParticleDesignerFloatValue TangentialAccelVariance;

		[XmlElement]
		public ParticleDesignerColor StartColor;

		[XmlElement]
		public ParticleDesignerColor StartColorVariance;

		[XmlElement]
		public ParticleDesignerColor FinishColor;

		[XmlElement]
		public ParticleDesignerColor FinishColorVariance;

		[XmlElement]
		public ParticleDesignerIntValue MaxParticles;

		[XmlElement]
		public ParticleDesignerFloatValue StartParticleSize;

		[XmlElement]
		public ParticleDesignerFloatValue StartParticleSizeVariance;

		[XmlElement]
		public ParticleDesignerFloatValue FinishParticleSize;

		[XmlElement]
		public ParticleDesignerFloatValue FinishParticleSizeVariance;

		[XmlElement]
		public ParticleDesignerFloatValue Duration;

		[XmlElement]
		public ParticleDesignerIntValue EmitterType;

		[XmlElement]
		public ParticleDesignerFloatValue MaxRadius;

		[XmlElement]
		public ParticleDesignerFloatValue MaxRadiusVariance;

		[XmlElement]
		public ParticleDesignerFloatValue MinRadius;

		[XmlElement]
		public ParticleDesignerFloatValue MinRadiusVariance;

		[XmlElement]
		public ParticleDesignerFloatValue RotatePerSecond;

		[XmlElement]
		public ParticleDesignerFloatValue RotatePerSecondVariance;

		[XmlElement]
		public ParticleDesignerIntValue BlendFuncSource;

		[XmlElement]
		public ParticleDesignerIntValue BlendFuncDestination;

		[XmlElement]
		public ParticleDesignerFloatValue RotationStart;

		[XmlElement]
		public ParticleDesignerFloatValue RotationStartVariance;

		[XmlElement]
		public ParticleDesignerFloatValue RotationEnd;

		[XmlElement]
		public ParticleDesignerFloatValue RotationEndVariance;

		[XmlElement]
		public ParticleDesignerTexture Texture;
	}
}