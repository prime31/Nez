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
		protected override ParticleEmitterConfig Read(ContentReader reader, ParticleEmitterConfig existingInstance)
		{
			// Create a fresh TextureAtlas instance
			var emitterConfig = new ParticleEmitterConfig();

			emitterConfig.SourcePosition = reader.ReadVector2();
			emitterConfig.SourcePositionVariance = reader.ReadVector2();
			emitterConfig.Speed = reader.ReadSingle();
			emitterConfig.SpeedVariance = reader.ReadSingle();
			emitterConfig.ParticleLifespan = reader.ReadSingle();
			emitterConfig.ParticleLifespanVariance = reader.ReadSingle();
			emitterConfig.Angle = reader.ReadSingle();
			emitterConfig.AngleVariance = reader.ReadSingle();
			emitterConfig.Gravity = reader.ReadVector2();
			emitterConfig.RadialAcceleration = reader.ReadSingle();
			emitterConfig.RadialAccelVariance = reader.ReadSingle();
			emitterConfig.TangentialAcceleration = reader.ReadSingle();
			emitterConfig.TangentialAccelVariance = reader.ReadSingle();
			emitterConfig.StartColor = reader.ReadColor();
			emitterConfig.StartColorVariance = reader.ReadColor();
			emitterConfig.FinishColor = reader.ReadColor();
			emitterConfig.FinishColorVariance = reader.ReadColor();
			emitterConfig.MaxParticles = reader.ReadUInt32();
			emitterConfig.StartParticleSize = reader.ReadSingle();
			emitterConfig.StartParticleSizeVariance = reader.ReadSingle();
			emitterConfig.FinishParticleSize = reader.ReadSingle();
			emitterConfig.FinishParticleSizeVariance = reader.ReadSingle();
			emitterConfig.Duration = reader.ReadSingle();
			emitterConfig.EmitterType = (ParticleEmitterType) reader.ReadInt32();

			emitterConfig.MaxRadius = reader.ReadSingle();
			emitterConfig.MaxRadiusVariance = reader.ReadSingle();
			emitterConfig.MinRadius = reader.ReadSingle();
			emitterConfig.MinRadiusVariance = reader.ReadSingle();
			emitterConfig.RotatePerSecond = reader.ReadSingle();
			emitterConfig.RotatePerSecondVariance = reader.ReadSingle();

			emitterConfig.RotationStart = reader.ReadSingle();
			emitterConfig.RotationStartVariance = reader.ReadSingle();
			emitterConfig.RotationEnd = reader.ReadSingle();
			emitterConfig.RotationEndVariance = reader.ReadSingle();
			emitterConfig.EmissionRate = reader.ReadSingle();
			emitterConfig.BlendFuncSource = (Blend) reader.ReadInt32();
			emitterConfig.BlendFuncDestination = (Blend) reader.ReadInt32();


			var texture = reader.ReadObject<Texture2D>();
			emitterConfig.Subtexture = new Nez.Textures.Subtexture(texture);

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