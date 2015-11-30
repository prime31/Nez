using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;


namespace Nez.ParticleSystemSettings
{
	/// <summary>
	/// Particle system settings processor. Create an XML file based on the ParticleType XML template (you can use the XmlTemplateMakerProcessor
	/// to create the template easily). At runtime you can load it up like so:
	/// 
	/// var particleType = content.Load<ParticleType>( "ParticleSystemSettings/XmlFileName" );
	/// particleType.loadParticleTexture( contentManager );
	/// </summary>
	[ContentProcessor( DisplayName = "Particle System Settings Processor" )]
	public class ParticleSystemSettingsProcessor : ContentProcessor<ParticleType,ParticleType>
	{
		public override ParticleType Process( ParticleType input, ContentProcessorContext context )
		{
			return input;
		}
	}
}
