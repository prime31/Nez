using System;
using Nez.Particles;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Nez.Textures;


namespace Nez.ParticleDesigner
{
	public class ParticleEmitterConfigReader : ContentTypeReader<ParticleEmitterConfig>
	{
		protected override ParticleEmitterConfig Read( ContentReader reader, ParticleEmitterConfig existingInstance )
		{
			// Create a fresh TextureAtlas instance
			var emitterConfig = new ParticleEmitterConfig();

			emitterConfig.sourcePosition = reader.ReadVector2();
			emitterConfig.sourcePositionVariance = reader.ReadVector2();
			emitterConfig.speed = reader.ReadSingle();
			emitterConfig.speedVariance = reader.ReadSingle();
			emitterConfig.particleLifespan = reader.ReadSingle();
			emitterConfig.particleLifespanVariance = reader.ReadSingle();
			emitterConfig.angle = reader.ReadSingle();
			emitterConfig.angleVariance = reader.ReadSingle();
			emitterConfig.gravity = reader.ReadVector2();
			emitterConfig.radialAcceleration = reader.ReadSingle();
			emitterConfig.radialAccelVariance = reader.ReadSingle();
			emitterConfig.tangentialAcceleration = reader.ReadSingle();
			emitterConfig.tangentialAccelVariance = reader.ReadSingle();
			emitterConfig.startColor = reader.ReadColor();
			emitterConfig.startColorVariance = reader.ReadColor();
			emitterConfig.finishColor = reader.ReadColor();
			emitterConfig.finishColorVariance = reader.ReadColor();
			emitterConfig.maxParticles = reader.ReadUInt32();
			emitterConfig.startParticleSize = reader.ReadSingle();
			emitterConfig.startParticleSizeVariance = reader.ReadSingle();
			emitterConfig.finishParticleSize = reader.ReadSingle();
			emitterConfig.finishParticleSizeVariance = reader.ReadSingle();
			emitterConfig.duration = reader.ReadSingle();
			emitterConfig.emitterType = (ParticleEmitterType)reader.ReadInt32();

			emitterConfig.maxRadius = reader.ReadSingle();
			emitterConfig.maxRadiusVariance = reader.ReadSingle();
			emitterConfig.minRadius = reader.ReadSingle();
			emitterConfig.minRadiusVariance = reader.ReadSingle();
			emitterConfig.rotatePerSecond = reader.ReadSingle();
			emitterConfig.rotatePerSecondVariance = reader.ReadSingle();

			emitterConfig.rotationStart = reader.ReadSingle();
			emitterConfig.rotationStartVariance = reader.ReadSingle();
			emitterConfig.rotationEnd = reader.ReadSingle();
			emitterConfig.rotationEndVariance = reader.ReadSingle();
			emitterConfig.emissionRate = reader.ReadSingle();
			emitterConfig.blendFuncSource = (Blend)reader.ReadInt32();
			emitterConfig.blendFuncDestination = (Blend)reader.ReadInt32();


			var texture = reader.ReadObject<Texture2D>();
			emitterConfig.subtexture = new Nez.Textures.Subtexture( texture );

			#if USE_RAW_TIFFS
			// raw tiffs from a byte[] were originally used. Leaving this here for now just in case textures dont end up working
			var tiffSize = reader.ReadInt32();
			if( tiffSize > 0 )
			{
				var bytes = reader.ReadBytes( tiffSize );
				using( var stream = new MemoryStream( bytes ) )
				{
					var tex = Texture2D.FromStream( Core.graphicsDevice, stream );
					emitterConfig.subtexture = new Subtexture( tex );
				}
			}
			#endif


			return emitterConfig;
		}
	}
}

