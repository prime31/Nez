using System;
using System.Xml.Serialization;


namespace Nez.ParticleDesignerImporter
{
	[XmlRoot("particleEmitterConfig")]
	public class ParticleDesignerEmitterConfig
	{
		[XmlElement("sourcePosition")] public ParticleDesignerVector2 SourcePosition;

		[XmlElement("sourcePositionVariance")] public ParticleDesignerVector2 SourcePositionVariance;

		[XmlElement("speed")] public ParticleDesignerFloatValue Speed;

		[XmlElement("speedVariance")] public ParticleDesignerFloatValue SpeedVariance;

		[XmlElement("particleLifeSpan")] public ParticleDesignerFloatValue ParticleLifeSpan;

		[XmlElement("particleLifespanVariance")]
		public ParticleDesignerFloatValue ParticleLifespanVariance;

		[XmlElement("angle")] public ParticleDesignerFloatValue Angle;

		[XmlElement("angleVariance")] public ParticleDesignerFloatValue AngleVariance;

		[XmlElement("gravity")] public ParticleDesignerVector2 Gravity;

		[XmlElement("radialAcceleration")] public ParticleDesignerFloatValue RadialAcceleration;

		[XmlElement("tangentialAcceleration")] public ParticleDesignerFloatValue TangentialAcceleration;

		[XmlElement("radialAccelVariance")] public ParticleDesignerFloatValue RadialAccelVariance;

		[XmlElement("tangentialAccelVariance")]
		public ParticleDesignerFloatValue TangentialAccelVariance;

		[XmlElement("startColor")] public ParticleDesignerColor StartColor;

		[XmlElement("startColorVariance")] public ParticleDesignerColor StartColorVariance;

		[XmlElement("finishColor")] public ParticleDesignerColor FinishColor;

		[XmlElement("finishColorVariance")] public ParticleDesignerColor FinishColorVariance;

		[XmlElement("maxParticles")] public ParticleDesignerIntValue MaxParticles;

		[XmlElement("startParticleSize")] public ParticleDesignerFloatValue StartParticleSize;

		[XmlElement("startParticleSizeVariance")]
		public ParticleDesignerFloatValue StartParticleSizeVariance;

		[XmlElement("finishParticleSize")] public ParticleDesignerFloatValue FinishParticleSize;

		[XmlElement("finishParticleSizeVariance")]
		public ParticleDesignerFloatValue FinishParticleSizeVariance;

		[XmlElement("duration")] public ParticleDesignerFloatValue Duration;

		[XmlElement("emitterType")] public ParticleDesignerIntValue EmitterType;

		[XmlElement("maxRadius")] public ParticleDesignerFloatValue MaxRadius;

		[XmlElement("maxRadiusVariance")] public ParticleDesignerFloatValue MaxRadiusVariance;

		[XmlElement("minRadius")] public ParticleDesignerFloatValue MinRadius;

		[XmlElement("minRadiusVariance")] public ParticleDesignerFloatValue MinRadiusVariance;

		[XmlElement("rotatePerSecond")] public ParticleDesignerFloatValue RotatePerSecond;

		[XmlElement("rotatePerSecondVariance")]
		public ParticleDesignerFloatValue RotatePerSecondVariance;

		[XmlElement("blendFuncSource")] public ParticleDesignerIntValue BlendFuncSource;

		[XmlElement("blendFuncDestination")] public ParticleDesignerIntValue BlendFuncDestination;

		[XmlElement("rotationStart")] public ParticleDesignerFloatValue RotationStart;

		[XmlElement("rotationStartVariance")] public ParticleDesignerFloatValue RotationStartVariance;

		[XmlElement("rotationEnd")] public ParticleDesignerFloatValue RotationEnd;

		[XmlElement("rotationEndVariance")] public ParticleDesignerFloatValue RotationEndVariance;

		[XmlElement("texture")] public ParticleDesignerTexture Texture;
	}
}