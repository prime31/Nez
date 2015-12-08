using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Nez.Experimental;
using System.IO;
using Ionic.Zlib;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.ParticleDesignerImporter
{
	[ContentProcessor( DisplayName = "Particle Designer Processor" )]
	public class ParticleDesignerProcessor : ContentProcessor<ParticleEmitterConfig,ParticleEmitter>
	{
		public override ParticleEmitter Process( ParticleEmitterConfig input, ContentProcessorContext context )
		{
//			/* BlendingFactorDest */
//			#define GL_ZERO                                          0
//			#define GL_ONE                                           1
//			#define GL_SRC_COLOR                                     0x0300
//			#define GL_ONE_MINUS_SRC_COLOR                           0x0301
//			#define GL_SRC_ALPHA                                     0x0302
//			#define GL_ONE_MINUS_SRC_ALPHA                           0x0303
//			#define GL_DST_ALPHA                                     0x0304
//			#define GL_ONE_MINUS_DST_ALPHA                           0x0305


//			/* BlendingFactorSrc */
//			/*      GL_ZERO */
//			/*      GL_ONE */
//			#define GL_DST_COLOR                                     0x0306
//			#define GL_ONE_MINUS_DST_COLOR                           0x0307
//			#define GL_SRC_ALPHA_SATURATE                            0x0308
//			/*      GL_SRC_ALPHA */
//			/*      GL_ONE_MINUS_SRC_ALPHA */
//			/*      GL_DST_ALPHA */
//			/*      GL_ONE_MINUS_DST_ALPHA */


			byte[] bytes;
			using( var memoryStream = new MemoryStream( Convert.FromBase64String( input.texture.data ), writable: false ) )
			{
				using( var stream = new GZipStream( memoryStream, CompressionMode.Decompress ) )
				{
					const int size = 4096;
					byte[] buffer = new byte[size];
					using( var memory = new MemoryStream() )
					{
						int count = 0;
						do
						{
							count = stream.Read( buffer, 0, size );
							if( count > 0 )
								memory.Write( buffer, 0, count );

						} while ( count > 0 );

						bytes = memory.ToArray();
					}
				}
			}


			return new ParticleEmitter {
				sourcePosition = input.sourcePosition,
				sourcePositionVariance = input.sourcePositionVariance,
				speed = input.speed,
				speedVariance = input.speedVariance,
				particleLifespan = input.particleLifeSpan,
				particleLifespanVariance = input.particleLifespanVariance,
				angle = input.angle,
				angleVariance = input.angleVariance,
				gravity = input.gravity,
				radialAcceleration = input.radialAcceleration,
				tangentialAcceleration = input.tangentialAcceleration,
				radialAccelVariance = input.radialAccelVariance,
				tangentialAccelVariance = input.tangentialAccelVariance,
				startColor = input.startColor,
				startColorVariance = input.startColorVariance,
				finishColor = input.finishColor,
				finishColorVariance = input.finishColorVariance,
				maxParticles = (uint)input.maxParticles.value,
				startParticleSize = input.startParticleSize,
				startParticleSizeVariance = input.startParticleSizeVariance,
				finishParticleSize = input.finishParticleSize,
				finishParticleSizeVariance = input.finishParticleSizeVariance,
				duration = input.duration,
				emitterType = input.emitterType,
				maxRadius = input.maxRadius,
				maxRadiusVariance = input.maxRadiusVariance,
				minRadius = input.minRadius,
				minRadiusVariance = input.minRadiusVariance,
				rotatePerSecond = input.rotatePerSecond,
				rotatePerSecondVariance = input.rotatePerSecondVariance,
				//blendFuncSource = input.blendFuncSource,
				//blendFuncDestination = input.blendFuncDestination,
				rotationStart = input.rotationStart,
				rotationStartVariance = input.rotationStartVariance,
				rotationEnd = input.rotationEnd,
				rotationEndVariance = input.rotationEndVariance,
				emissionRate = input.maxParticles / input.particleLifeSpan,

				tempImageData = bytes
			};
		}
	}
}

