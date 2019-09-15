using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Particles;

namespace Nez.ParticleDesigner
{
	public static class ParticleEmitterConfigLoader
	{
		/// <summary>
		/// parses a ParticleDesigner pex file or one exported from the Nez.Samples exporter or from Particle Editor:
		/// http://onebyonedesign.com/flash/particleeditor/
		/// </summary>
		public static ParticleEmitterConfig Load(string name)
		{
			using (var stream = TitleContainer.OpenStream(name))
			{
				using (var reader = XmlReader.Create(stream))
				{
					return Load(XDocument.Load(reader), Path.GetDirectoryName(name));
				}
			}
		}

		static ParticleEmitterConfig Load(XDocument xDoc, string rootDir)
		{
			var root = xDoc.Element("particleEmitterConfig");

			var config = new ParticleEmitterConfig
			{
				SourcePosition = GetVectorElement(root, "sourcePosition"),
				SourcePositionVariance = GetVectorElement(root, "sourcePositionVariance"),
				Speed = GetFloatElement(root, "speed"),
				SpeedVariance = GetFloatElement(root, "speedVariance"),
				ParticleLifespan = GetFloatElement(root, "particleLifeSpan"),
				ParticleLifespanVariance = GetFloatElement(root, "particleLifespanVariance"),
				Angle = GetFloatElement(root, "angle"),
				AngleVariance = GetFloatElement(root, "angleVariance"),
				Gravity = GetVectorElement(root, "gravity"),
				RadialAcceleration = GetFloatElement(root, "radialAcceleration"),
				RadialAccelVariance = GetFloatElement(root, "radialAccelVariance"),
				TangentialAcceleration = GetFloatElement(root, "tangentialAcceleration"),
				TangentialAccelVariance = GetFloatElement(root, "tangentialAccelVariance"),
				StartColor = GetColorElement(root, "startColor"),
				StartColorVariance = GetColorElement(root, "startColorVariance"),
				FinishColor = GetColorElement(root, "finishColor"),
				FinishColorVariance = GetColorElement(root, "finishColorVariance"),
				MaxParticles = (uint)GetIntElement(root, "maxParticles"),
				StartParticleSize = GetFloatElement(root, "startParticleSize"),
				StartParticleSizeVariance = GetFloatElement(root, "startParticleSizeVariance"),
				FinishParticleSize = GetFloatElement(root, "finishParticleSize"),
				FinishParticleSizeVariance = root.Element("finishParticleSizeVariance") != null ? GetFloatElement(root, "finishParticleSizeVariance") : GetFloatElement(root, "FinishParticleSizeVariance"),
				Duration = GetFloatElement(root, "duration"),
				EmitterType = (ParticleEmitterType)GetIntElement(root, "emitterType"),
				MaxRadius = GetFloatElement(root, "maxRadius"),
				MaxRadiusVariance = GetFloatElement(root, "maxRadiusVariance"),
				MinRadius = GetFloatElement(root, "minRadius"),
				MinRadiusVariance = GetFloatElement(root, "minRadiusVariance"),
				RotatePerSecond = GetFloatElement(root, "rotatePerSecond"),
				RotatePerSecondVariance = GetFloatElement(root, "rotatePerSecondVariance"),
				BlendFuncSource = GetBlendElement(root, "blendFuncSource"),
				BlendFuncDestination = GetBlendElement(root, "blendFuncDestination"),
				RotationStart = GetFloatElement(root, "rotationStart"),
				RotationStartVariance = GetFloatElement(root, "rotationStartVariance"),
				RotationEnd = GetFloatElement(root, "rotationEnd"),
				RotationEndVariance = GetFloatElement(root, "rotationEndVariance")
			};

			config.EmissionRate = config.MaxParticles / config.ParticleLifespan;
			if (float.IsInfinity(config.EmissionRate))
			{
				Debug.Error("---- particle system EmissionRate (MaxParticles / ParticleLifespace) is infinity. Resetting to 10000");
				config.EmissionRate = 10000;
			}

			var textureElement = root.Element("texture");
			var data = textureElement.Attribute("data");

			// texture could be either a separate file or base64 encoded tiff
			if (data != null)
			{
				using (var memoryStream = new MemoryStream(Convert.FromBase64String((string)data), writable: false))
				{
					using (var stream = new GZipStream(memoryStream, CompressionMode.Decompress))
					{
						using (var mem = new MemoryStream())
						{
							stream.CopyTo(mem);

							var bitmap = System.Drawing.Image.FromStream(mem) as System.Drawing.Bitmap;
							var colors = new Color[bitmap.Width * bitmap.Height];

							for (var x = 0; x < bitmap.Width; x++)
							{
								for (var y = 0; y < bitmap.Height; y++)
								{
									var drawColor = bitmap.GetPixel(x, y);
									colors[x + y * bitmap.Width] = new Color(drawColor.R, drawColor.G, drawColor.B, drawColor.A);
								}
							}

							var texture = new Texture2D(Core.GraphicsDevice, bitmap.Width, bitmap.Height);
							texture.SetData(colors);
							config.Sprite = new Textures.Sprite(texture);
						}
					}
				}
			}
			else
			{
				var path = Path.Combine(rootDir, (string)textureElement.Attribute("name"));
				using (var stream = TitleContainer.OpenStream(path))
				{
					var texture = Texture2D.FromStream(Core.GraphicsDevice, stream);
					config.Sprite = new Textures.Sprite(texture);
				}
			}

			return config;
		}

		static float GetFloatElement(XElement root, string name) => (float)root.Element(name).Attribute("value");

		/// <summary>
		/// for some reason, some pex exporters export ints like maxParticles as floats. This mess guards against that.
		/// </summary>
		static int GetIntElement(XElement root, string name) => Mathf.RoundToInt(GetFloatElement(root, name));

		static Vector2 GetVectorElement(XElement root, string name)
		{
			var ele = root.Element(name);
			return new Vector2((float)ele.Attribute("x"), (float)ele.Attribute("y"));
		}

		static Color GetColorElement(XElement root, string name)
		{
			var ele = root.Element(name);
			return new Color((float)ele.Attribute("red"), (float)ele.Attribute("green"), (float)ele.Attribute("blue"), (float)ele.Attribute("alpha"));
		}

		static Blend GetBlendElement(XElement root, string name)
		{
			var value = GetIntElement(root, name);
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

			throw new Exception("pex file has invalid blend");
		}
	}
}
