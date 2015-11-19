using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;


namespace Nez.Content.Pipeline.BitmapFonts
{
	[ContentProcessor( DisplayName = "BMFont Processor" )]
	public class BitmapFontProcessor : ContentProcessor<BitmapFontFile, BitmapFontProcessorResult>
	{
		int _someInt = 3;

		[DisplayName( "Test int" )]
		[Description( "Amount of subdivisions for decomposing cubic bézier curves into line segments." )]
		[DefaultValue( 3 )]
		public int IntTest
		{
			get { return _someInt; }
			set { _someInt = value; }
		}


		bool someBool = false;

		[DisplayName( "Test bool" )]
		[Description( "Decompose paths into convex polygons." )]
		[DefaultValue( false )]
		public bool BoolTest
		{
			get { return someBool; }
			set { someBool = value; }
		}


		float _float = 5f;

		[DisplayName( "Test float" )]
		[Description( "Decompose paths into convex polygons." )]
		[DefaultValue( 5f )]
		public float FloatTest
		{
			get { return _float; }
			set { _float = value; }
		}


		string _someString = "hi";

		[DisplayName( "Test string" )]
		[Description( "Decompose paths into convex polygons." )]
		[DefaultValue( "hi" )]
		public string StringTest
		{
			get { return _someString; }
			set { _someString = value; }
		}


		public override BitmapFontProcessorResult Process( BitmapFontFile bitmapFontFile, ContentProcessorContext context )
		{
			try
			{
				context.Logger.LogMessage( "Processing BMFont" );
				var result = new BitmapFontProcessorResult( bitmapFontFile );

				foreach( var fontPage in bitmapFontFile.Pages )
				{
					var assetName = Path.GetFileNameWithoutExtension( fontPage.File );
					context.Logger.LogMessage( "Expecting texture asset: {0}", assetName );
					result.textureAssets.Add( assetName );
				}

				return result;

			}
			catch( Exception ex )
			{
				context.Logger.LogMessage( "Error {0}", ex );
				throw;
			}
		}
	}
}