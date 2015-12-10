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
		protected override void Write( ContentWriter writer, ParticleDesignerProcessorResult value )
		{
			var emissionRate = value.particleEmitterConfig.maxParticles / value.particleEmitterConfig.particleLifeSpan;
			if( float.IsInfinity( emissionRate ) )
			{
				ParticleDesignerProcessor.logger.LogMessage( "---- emissionRate (maxParticles / particleLifespace) is infinity. Resetting to 10000" );
				emissionRate = 10000;
			}

			// Write out all the members
			writer.Write( (Vector2)value.particleEmitterConfig.sourcePosition );
			writer.Write( (Vector2)value.particleEmitterConfig.sourcePositionVariance );
			writer.Write( value.particleEmitterConfig.speed.value );
			writer.Write( value.particleEmitterConfig.speedVariance.value );
			writer.Write( value.particleEmitterConfig.particleLifeSpan.value );
			writer.Write( value.particleEmitterConfig.particleLifespanVariance.value );
			writer.Write( value.particleEmitterConfig.angle.value );
			writer.Write( value.particleEmitterConfig.angleVariance.value );
			writer.Write( (Vector2)value.particleEmitterConfig.gravity );
			writer.Write( value.particleEmitterConfig.radialAcceleration.value );
			writer.Write( value.particleEmitterConfig.radialAccelVariance.value );
			writer.Write( value.particleEmitterConfig.tangentialAcceleration.value );
			writer.Write( value.particleEmitterConfig.tangentialAccelVariance.value );
			writer.Write( value.particleEmitterConfig.startColor );
			writer.Write( value.particleEmitterConfig.startColorVariance );
			writer.Write( value.particleEmitterConfig.finishColor );
			writer.Write( value.particleEmitterConfig.finishColorVariance );
			writer.Write( value.particleEmitterConfig.maxParticles.value );
			writer.Write( value.particleEmitterConfig.startParticleSize.value );
			writer.Write( value.particleEmitterConfig.startParticleSizeVariance.value );
			writer.Write( value.particleEmitterConfig.finishParticleSize.value );
			writer.Write( value.particleEmitterConfig.finishParticleSizeVariance.value );
			writer.Write( value.particleEmitterConfig.duration.value );
			writer.Write( value.particleEmitterConfig.emitterType.value );

			writer.Write( value.particleEmitterConfig.maxRadius.value );
			writer.Write( value.particleEmitterConfig.maxRadiusVariance.value );
			writer.Write( value.particleEmitterConfig.minRadius.value );
			writer.Write( value.particleEmitterConfig.minRadiusVariance.value );
			writer.Write( value.particleEmitterConfig.rotatePerSecond.value );
			writer.Write( value.particleEmitterConfig.rotatePerSecondVariance.value );

			writer.Write( value.particleEmitterConfig.rotationStart.value );
			writer.Write( value.particleEmitterConfig.rotationStartVariance.value );
			writer.Write( value.particleEmitterConfig.rotationEnd.value );
			writer.Write( value.particleEmitterConfig.rotationEndVariance.value );
			writer.Write( emissionRate );
			writer.Write( (int)blendForParticleDesignerInt( value.particleEmitterConfig.blendFuncSource ) );
			writer.Write( (int)blendForParticleDesignerInt( value.particleEmitterConfig.blendFuncDestination ) );

			// use raw tiff data for now
			writer.WriteObject( value.texture );

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


		Blend blendForParticleDesignerInt( int value )
		{
			switch( value )
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

			throw new InvalidContentException( "blendForParticleDesignerInt found no match" );
		}


		public override string GetRuntimeType( TargetPlatform targetPlatform )
		{
			return typeof( ParticleEmitterConfig ).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader( TargetPlatform targetPlatform )
		{
			// This is the full namespace path and class name of the reader, along with the assembly name which is the project name by default.
			return typeof( ParticleEmitterConfigReader ).AssemblyQualifiedName;
		}
	}
}

