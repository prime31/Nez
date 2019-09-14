using System;
using Nez.Particles;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.ParticleDesigner
{
	public class ParticleEmitterConfigReader : ContentTypeReader<ParticleEmitterConfig>
	{
		protected override ParticleEmitterConfig Read(ContentReader reader, ParticleEmitterConfig existingInstance)
		{
			// Create a fresh TextureAtlas instance
			var emitterConfig = new ParticleEmitterConfig
			{
				SourcePosition = reader.ReadVector2(),
				SourcePositionVariance = reader.ReadVector2(),
				Speed = reader.ReadSingle(),
				SpeedVariance = reader.ReadSingle(),
				ParticleLifespan = reader.ReadSingle(),
				ParticleLifespanVariance = reader.ReadSingle(),
				Angle = reader.ReadSingle(),
				AngleVariance = reader.ReadSingle(),
				Gravity = reader.ReadVector2(),
				RadialAcceleration = reader.ReadSingle(),
				RadialAccelVariance = reader.ReadSingle(),
				TangentialAcceleration = reader.ReadSingle(),
				TangentialAccelVariance = reader.ReadSingle(),
				StartColor = reader.ReadColor(),
				StartColorVariance = reader.ReadColor(),
				FinishColor = reader.ReadColor(),
				FinishColorVariance = reader.ReadColor(),
				MaxParticles = reader.ReadUInt32(),
				StartParticleSize = reader.ReadSingle(),
				StartParticleSizeVariance = reader.ReadSingle(),
				FinishParticleSize = reader.ReadSingle(),
				FinishParticleSizeVariance = reader.ReadSingle(),
				Duration = reader.ReadSingle(),
				EmitterType = (ParticleEmitterType)reader.ReadInt32(),

				MaxRadius = reader.ReadSingle(),
				MaxRadiusVariance = reader.ReadSingle(),
				MinRadius = reader.ReadSingle(),
				MinRadiusVariance = reader.ReadSingle(),
				RotatePerSecond = reader.ReadSingle(),
				RotatePerSecondVariance = reader.ReadSingle(),

				RotationStart = reader.ReadSingle(),
				RotationStartVariance = reader.ReadSingle(),
				RotationEnd = reader.ReadSingle(),
				RotationEndVariance = reader.ReadSingle(),
				EmissionRate = reader.ReadSingle(),
				BlendFuncSource = (Blend)reader.ReadInt32(),
				BlendFuncDestination = (Blend)reader.ReadInt32()
			};


			var texture = reader.ReadObject<Texture2D>();
			emitterConfig.Subtexture = new Subtexture(texture);

			return emitterConfig;
		}
	}
}