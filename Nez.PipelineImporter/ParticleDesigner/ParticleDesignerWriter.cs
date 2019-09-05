using System;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nez.Particles;
using Nez.ParticleDesigner;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.ParticleDesignerImporter
{
	[ContentTypeWriter]
	public class ParticleDesignerWriter : ContentTypeWriter<ParticleDesignerProcessorResult>
	{
		protected override void Write(ContentWriter writer, ParticleDesignerProcessorResult value)
		{
			var emissionRate = value.ParticleEmitterConfig.MaxParticles / value.ParticleEmitterConfig.ParticleLifeSpan;
			if (float.IsInfinity(emissionRate))
			{
				ParticleDesignerProcessor.Logger.LogMessage(
					"---- emissionRate (maxParticles / particleLifespace) is infinity. Resetting to 10000");
				emissionRate = 10000;
			}

			// Write out all the members
			writer.Write((Vector2) value.ParticleEmitterConfig.SourcePosition);
			writer.Write((Vector2) value.ParticleEmitterConfig.SourcePositionVariance);
			writer.Write(value.ParticleEmitterConfig.Speed.Value);
			writer.Write(value.ParticleEmitterConfig.SpeedVariance.Value);
			writer.Write(value.ParticleEmitterConfig.ParticleLifeSpan.Value);
			writer.Write(value.ParticleEmitterConfig.ParticleLifespanVariance.Value);
			writer.Write(value.ParticleEmitterConfig.Angle.Value);
			writer.Write(value.ParticleEmitterConfig.AngleVariance.Value);
			writer.Write((Vector2) value.ParticleEmitterConfig.Gravity);
			writer.Write(value.ParticleEmitterConfig.RadialAcceleration.Value);
			writer.Write(value.ParticleEmitterConfig.RadialAccelVariance.Value);
			writer.Write(value.ParticleEmitterConfig.TangentialAcceleration.Value);
			writer.Write(value.ParticleEmitterConfig.TangentialAccelVariance.Value);
			writer.Write(value.ParticleEmitterConfig.StartColor);
			writer.Write(value.ParticleEmitterConfig.StartColorVariance);
			writer.Write(value.ParticleEmitterConfig.FinishColor);
			writer.Write(value.ParticleEmitterConfig.FinishColorVariance);
			writer.Write(value.ParticleEmitterConfig.MaxParticles.Value);
			writer.Write(value.ParticleEmitterConfig.StartParticleSize.Value);
			writer.Write(value.ParticleEmitterConfig.StartParticleSizeVariance.Value);
			writer.Write(value.ParticleEmitterConfig.FinishParticleSize.Value);
			writer.Write(value.ParticleEmitterConfig.FinishParticleSizeVariance.Value);
			writer.Write(value.ParticleEmitterConfig.Duration.Value);
			writer.Write(value.ParticleEmitterConfig.EmitterType.Value);

			writer.Write(value.ParticleEmitterConfig.MaxRadius.Value);
			writer.Write(value.ParticleEmitterConfig.MaxRadiusVariance.Value);
			writer.Write(value.ParticleEmitterConfig.MinRadius.Value);
			writer.Write(value.ParticleEmitterConfig.MinRadiusVariance.Value);
			writer.Write(value.ParticleEmitterConfig.RotatePerSecond.Value);
			writer.Write(value.ParticleEmitterConfig.RotatePerSecondVariance.Value);

			writer.Write(value.ParticleEmitterConfig.RotationStart.Value);
			writer.Write(value.ParticleEmitterConfig.RotationStartVariance.Value);
			writer.Write(value.ParticleEmitterConfig.RotationEnd.Value);
			writer.Write(value.ParticleEmitterConfig.RotationEndVariance.Value);
			writer.Write(emissionRate);
			writer.Write((int) BlendForParticleDesignerInt(value.ParticleEmitterConfig.BlendFuncSource));
			writer.Write((int) BlendForParticleDesignerInt(value.ParticleEmitterConfig.BlendFuncDestination));

			// use raw tiff data for now
			writer.WriteObject(value.Texture);

#if USE_RAW_TIFFS
			if( value.textureTiffData != null )
			{
				writer.Write( value.textureTiffData.Length );
				writer.Write( value.textureTiffData );
			}
			else
			{
				writer.Write( 0 );
			}
#endif
		}


		Blend BlendForParticleDesignerInt(int value)
		{
			switch (value)
			{
				case 0:
					return Blend.Zero;
				case 1:
					return Blend.One;
				case 0x0300:
					return Blend.SourceColor;
				case 0x0301:
					return Blend.InverseSourceColor;
				case 0x0302:
					return Blend.SourceAlpha;
				case 0x0303:
					return Blend.InverseSourceAlpha;
				case 0x0304:
					return Blend.DestinationAlpha;
				case 0x0305:
					return Blend.InverseDestinationAlpha;
				case 0x0306:
					return Blend.DestinationColor;
				case 0x0307:
					return Blend.InverseDestinationColor;
				case 0x0308:
					return Blend.SourceAlphaSaturation;
			}

			throw new InvalidContentException("blendForParticleDesignerInt found no match");
		}


		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return typeof(ParticleEmitterConfig).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			// This is the full namespace path and class name of the reader, along with the assembly name which is the project name by default.
			return typeof(ParticleEmitterConfigReader).AssemblyQualifiedName;
		}
	}
}