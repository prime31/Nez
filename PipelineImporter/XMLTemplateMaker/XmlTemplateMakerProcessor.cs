using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.Text;


namespace Nez.XmlTemplateMaker
{
	/// <summary>
	/// Simple little importer that's sole purpose is to print out a template XML file for any class. The template XML can then be used to
	/// create custom XML-to-object importors in just a few lines of code. See the ParticleSystemSettingsProcessor for an example.
	/// 
	/// The XML file passed to this processor should just be a System.string with the namespace.class of the type that you want a tempalte for.
	/// The template will be dumped to the Pipeline console but note that it will have utf-16 instead of utf-8 so you need to change that.
	/// 
	/// Lots of attributes are available for dealing with the XML parsing. You can find a great document on the attributes available
	/// here: http://blogs.msdn.com/b/shawnhar/archive/2008/08/12/everything-you-ever-wanted-to-know-about-intermediateserializer.aspx
	/// </summary>
	[ContentProcessor( DisplayName = "XML Template Maker Processor" )]
	public class XmlTemplateMakerProcessor : ContentProcessor<string,object>
	{
		public override object Process( string inputClass, ContentProcessorContext context )
		{
			//var inputType = Type.GetType( inputClass );
			var inputType = findTypeForClass( inputClass, context );

			var xmlSettings = new XmlWriterSettings();
			xmlSettings.Indent = true;
			var obj = Activator.CreateInstance( inputType );

			var outputString = new StringBuilder();
			using( var xmlWriter = XmlWriter.Create( outputString, xmlSettings ) )
				IntermediateSerializer.Serialize( xmlWriter, obj, null );

			context.Logger.LogMessage( "\n------- BEGIN XML TEMPLATE -------\n{0}\n------- END XML TEMPLATE -------\n", outputString );

			throw new Exception( "------ DISREGARD THIS EXCEPTION. IT IS THROWN ONLY SO THE XNB DOESNT GET WRITTEN TO THE PROJECT FILE" );
		}


		Type findTypeForClass( string inputClass, ContentProcessorContext context )
		{
			foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
			{
				foreach( var type in assembly.GetTypes() )
				{
					if( type.FullName == inputClass )
						return type;
				}
			}

			throw new Exception( "Could not locate the Type for the class " + inputClass );
		}
	}
}

