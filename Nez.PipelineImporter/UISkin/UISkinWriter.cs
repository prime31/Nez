using System;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nez.UI;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Nez.UISkinImporter
{
	[ContentTypeWriter]
	public class UISkinWriter : ContentTypeWriter<UISkinConfig>
	{
		protected override void Write(ContentWriter writer, UISkinConfig data)
		{
			if (data.Colors != null)
			{
				writer.Write(true);
				writer.WriteObject(data.Colors);
			}
			else
			{
				writer.Write(false);
			}

			if (data.TextureAtlases != null)
			{
				writer.Write(true);
				writer.WriteObject(data.TextureAtlases);
			}
			else
			{
				writer.Write(false);
			}

			if (data.LibGdxAtlases != null)
			{
				writer.Write(true);
				writer.WriteObject(data.LibGdxAtlases);
			}
			else
			{
				writer.Write(false);
			}

			if (data.Styles != null)
			{
				writer.Write(true);
				writer.WriteObject(data.Styles);
			}
			else
			{
				writer.Write(false);
			}
		}


		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return typeof(Nez.UI.UISkinConfig).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return typeof(Nez.UI.UISkinConfigReader).AssemblyQualifiedName;
		}
	}
}