using System;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework;


namespace Nez.PipelineImporter
{
	public static class ContentWriterExt
	{
		public static void Write( this ContentWriter writer, Rectangle rect )
		{
			writer.Write( rect.X );
			writer.Write( rect.Y );
			writer.Write( rect.Width );
			writer.Write( rect.Height );
		}
	}
}

