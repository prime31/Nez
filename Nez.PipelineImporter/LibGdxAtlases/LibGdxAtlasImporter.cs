using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace Nez.LibGdxAtlases
{
	[ContentImporter( ".atlas", DefaultProcessor = "LibGdxAtlasProcessor", DisplayName = "libGDX Atlas Importer" )]
	public class LibGdxAtlasImporter : ContentImporter<LibGdxAtlasFile>
	{
		static string[] tuple = new string[4];


		public override LibGdxAtlasFile Import( string filename, ContentImporterContext context )
		{
			if( filename == null )
				throw new ArgumentNullException( "filename" );

			var f = new LibGdxAtlasFile();

			using( var reader = new StreamReader( filename ) )
			{
				context.Logger.LogMessage( "Deserializing filename: {0}", filename );

				LibGdxAtlasPage pageImage = null;
				List<LibGdxAtlasPage> pages = new List<LibGdxAtlasPage>();
				List<LibGdxAtlasRegion> regions = new List<LibGdxAtlasRegion>();

				while( true )
				{
					var line = reader.ReadLine();
					if( line == null )
						break;
					if( line.Trim().Length == 0 )
						pageImage = null;
					else if( pageImage == null )
					{
						var imageName = line;
						context.Logger.LogMessage( "---- expecting image name: {0}", imageName );

						var width = 0f;
						var height = 0f;

						if( readTuple( reader ) == 2 )
						{
							// size is only optional for an atlas packed with an old TexturePacker.
							context.Logger.LogMessage( "" + tuple[0] );
							width = int.Parse( tuple[0] );
							height = int.Parse( tuple[1] );
							readTuple( reader );
						}
						context.Logger.LogMessage( "Width, Height: {0}, {1}", width, height );
						var format = tuple[0];

						readTuple( reader );
						var min = tuple[0];
						var max = tuple[1];

						var direction = readValue( reader );
						var repeatX = false;
						var repeatY = false;
						if( direction.Equals( "x" ) )
							repeatX = true;
						else if( direction.Equals( "y" ) )
							repeatY = true;
						else if( direction.Equals( "xy" ) )
						{
							repeatX = true;
							repeatY = true;
						}

						pageImage = new LibGdxAtlasPage( imageName, width, height, false, format, min, max, repeatX, repeatY );
						pages.Add( pageImage );
					}
					else
					{
						var rotate = Boolean.Parse( readValue( reader ) );

						readTuple( reader );
						var left = int.Parse( tuple[0] );
						var top = int.Parse( tuple[1] );
						context.Logger.LogMessage( "X, Y: {0}, {1}", top, left );

						readTuple( reader );
						var width = int.Parse( tuple[0] );
						var height = int.Parse( tuple[1] );
						context.Logger.LogMessage( "width, height: {0}, {1}", width, height );

						var region = new LibGdxAtlasRegion();
						region.page = pageImage.textureFile;
						region.sourceRectangle = new LibGdxAtlasRect();
						region.sourceRectangle.x = left;
						region.sourceRectangle.y = top;
						region.sourceRectangle.w = width;
						region.sourceRectangle.h = height;
						region.name = line;
						region.rotate = rotate;

						if( readTuple( reader ) == 4 )
						{
							// split is optional
							region.splits = new int[] {
								int.Parse( tuple[0] ),
								int.Parse( tuple[1] ),
								int.Parse( tuple[2] ),
								int.Parse( tuple[3] )
							};

							if( readTuple( reader ) == 4 )
							{
								// pad is optional, but only present with splits
								region.pads = new int[] {
									int.Parse( tuple[0] ),
									int.Parse( tuple[1] ),
									int.Parse( tuple[2] ),
									int.Parse( tuple[3] )
								};

								readTuple( reader );
							}
						}

						region.originalSize.x = int.Parse( tuple[0] );
						region.originalSize.y = int.Parse( tuple[1] );
						context.Logger.LogMessage( "Original size: {0}, {1}", region.originalSize.x, region.originalSize.y );

						readTuple( reader );
						region.offset.x = int.Parse( tuple[0] );
						region.offset.y = int.Parse( tuple[1] );
						context.Logger.LogMessage( "Offset: {0}, {1}", region.offset.x, region.offset.y );

						region.index = int.Parse( readValue( reader ) );
						context.Logger.LogMessage( "Index: {0}", region.index );

						//if (flip) region.flip = true;

						regions.Add( region );
					}
				}

				f.regions = regions;
				f.pages = pages;
				return f;
			}

		}


		private List<int> getDimensions( string line, ContentBuildLogger logger )
		{
			var colonPosition = line.IndexOf( ':' );
			var comaPosition = line.IndexOf( ',' );

			var res = new List<int>();
			var x = line.Substring( colonPosition + 1, comaPosition - colonPosition - 1 ).Trim();
			var y = line.Substring( comaPosition + 1, line.Length - comaPosition - 1 ).Trim();
			logger.LogMessage( x );
			logger.LogMessage( y );

			res.Add( int.Parse( x ) );
			res.Add( int.Parse( y ) );

			return res;
		}


		/** Returns the number of tuple values read (1, 2 or 4). */
		static int readTuple( StreamReader reader )
		{
			var line = reader.ReadLine();
			var colon = line.IndexOf( ':' );
			if( colon == -1 )
				throw new Exception( "Invalid line: " + line );
			
			int i = 0, lastMatch = colon + 1;
			for( i = 0; i < 3; i++ )
			{
				int comma = line.IndexOf( ',', lastMatch );
				if( comma == -1 )
					break;
				tuple[i] = line.Substring( lastMatch, comma - lastMatch ).Trim();
				lastMatch = comma + 1;
			}
			tuple[i] = line.Substring( lastMatch ).Trim();

			return i + 1;
		}


		static string readValue( StreamReader reader )
		{
			var line = reader.ReadLine();
			var colon = line.IndexOf( ':' );
			if( colon == -1 )
				throw new Exception( "Invalid line: " + line );
			
			return line.Substring( colon + 1 ).Trim();
		}
	}
}

