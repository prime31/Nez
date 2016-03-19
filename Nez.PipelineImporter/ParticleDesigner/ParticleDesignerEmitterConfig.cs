using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	[XmlRoot( "particleEmitterConfig" )]
	public class ParticleDesignerEmitterConfig
	{
		[XmlElement]
		public ParticleDesignerVector2 sourcePosition;

		[XmlElement]
		public ParticleDesignerVector2 sourcePositionVariance;

		[XmlElement]
		public ParticleDesignerFloatValue speed;

		[XmlElement]
		public ParticleDesignerFloatValue speedVariance;

		[XmlElement]
		public ParticleDesignerFloatValue particleLifeSpan;

		[XmlElement]
		public ParticleDesignerFloatValue particleLifespanVariance;

		[XmlElement]
		public ParticleDesignerFloatValue angle;

		[XmlElement]
		public ParticleDesignerFloatValue angleVariance;

		[XmlElement]
		public ParticleDesignerVector2 gravity;

		[XmlElement]
		public ParticleDesignerFloatValue radialAcceleration;

		[XmlElement]
		public ParticleDesignerFloatValue tangentialAcceleration;

		[XmlElement]
		public ParticleDesignerFloatValue radialAccelVariance;

		[XmlElement]
		public ParticleDesignerFloatValue tangentialAccelVariance;

		[XmlElement]
		public ParticleDesignerColor startColor;

		[XmlElement]
		public ParticleDesignerColor startColorVariance;

		[XmlElement]
		public ParticleDesignerColor finishColor;

		[XmlElement]
		public ParticleDesignerColor finishColorVariance;

		[XmlElement]
		public ParticleDesignerIntValue maxParticles;

		[XmlElement]
		public ParticleDesignerFloatValue startParticleSize;

		[XmlElement]
		public ParticleDesignerFloatValue startParticleSizeVariance;

		[XmlElement]
		public ParticleDesignerFloatValue finishParticleSize;

		[XmlElement]
		public ParticleDesignerFloatValue finishParticleSizeVariance;

		[XmlElement]
		public ParticleDesignerFloatValue duration;

		[XmlElement]
		public ParticleDesignerIntValue emitterType;

		[XmlElement]
		public ParticleDesignerFloatValue maxRadius;

		[XmlElement]
		public ParticleDesignerFloatValue maxRadiusVariance;

		[XmlElement]
		public ParticleDesignerFloatValue minRadius;

		[XmlElement]
		public ParticleDesignerFloatValue minRadiusVariance;

		[XmlElement]
		public ParticleDesignerFloatValue rotatePerSecond;

		[XmlElement]
		public ParticleDesignerFloatValue rotatePerSecondVariance;

		[XmlElement]
		public ParticleDesignerIntValue blendFuncSource;

		[XmlElement]
		public ParticleDesignerIntValue blendFuncDestination;

		[XmlElement]
		public ParticleDesignerFloatValue rotationStart;

		[XmlElement]
		public ParticleDesignerFloatValue rotationStartVariance;

		[XmlElement]
		public ParticleDesignerFloatValue rotationEnd;

		[XmlElement]
		public ParticleDesignerFloatValue rotationEndVariance;

		[XmlElement]
		public ParticleDesignerTexture texture;
	}
}