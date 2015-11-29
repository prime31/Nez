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
		static String[] tuple = new String[4];

		public override LibGdxAtlasFile Import( string filename, ContentImporterContext context )
		{
			if( filename == null )
				throw new ArgumentNullException( "filename" );

			LibGdxAtlasFile f = new LibGdxAtlasFile();

			using( var reader = new StreamReader( filename ) )
			{
				context.Logger.LogMessage( "Deserializing filename: {0}", filename );

				//string content = reader.ReadToEnd();

				LibGdxAtlasPage pageImage = null;
				List<LibGdxAtlasPage> pages = new List<LibGdxAtlasPage>();
				List<LibGdxAtlasRegion> regions = new List<LibGdxAtlasRegion>();

				while( true )
				{
					string line = reader.ReadLine();
					if( line == null )
						break;
					if( line.Trim().Length == 0 )
						pageImage = null;
					else if( pageImage == null )
					{
						string imageName = line;
						context.Logger.LogMessage( "Image name: {0}", imageName );

						float width = 0;
						float height = 0;

						if( readTuple( reader ) == 2 )
						{
							// size is only optional for an atlas packed with an old TexturePacker.
							context.Logger.LogMessage( "" + tuple[0] );
							width = int.Parse( tuple[0] );
							height = int.Parse( tuple[1] );
							readTuple( reader );
						}
						context.Logger.LogMessage( "Width, Height: {0}, {1}", width, height );
						string format = tuple[0];

						readTuple( reader );
						string min = tuple[0];
						string max = tuple[1];

						String direction = readValue( reader );
						bool repeatX = false;
						bool repeatY = false;
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
						bool rotate = Boolean.Parse( readValue( reader ) );

						readTuple( reader );
						int left = int.Parse( tuple[0] );
						int top = int.Parse( tuple[1] );
						context.Logger.LogMessage( "X, Y: {0}, {1}", top, left );

						readTuple( reader );
						int width = int.Parse( tuple[0] );
						int height = int.Parse( tuple[1] );
						context.Logger.LogMessage( "width, height: {0}, {1}", width, height );

						LibGdxAtlasRegion region = new LibGdxAtlasRegion();
						region.page = pageImage.textureFile;
						region.sourceRectangle = new LibGdxAtlasRect();
						region.sourceRectangle.x = left;
						region.sourceRectangle.y = top;
						region.sourceRectangle.w = width;
						region.sourceRectangle.h = height;
						region.name = line;
						region.rotate = rotate;

						if( readTuple( reader ) == 4 )
						{ // split is optional
							//region.splits = new int[] {Integer.parseInt(tuple[0]), Integer.parseInt(tuple[1]),
							//	Integer.parseInt(tuple[2]), Integer.parseInt(tuple[3])};

							if( readTuple( reader ) == 4 )
							{ // pad is optional, but only present with splits
								//	region.pads = new int[] {Integer.parseInt(tuple[0]), Integer.parseInt(tuple[1]),
								//		Integer.parseInt(tuple[2]), Integer.parseInt(tuple[3])};

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

/*
				string pack = content.Trim();
				List<string> lines = pack.Split( '\n' ).ToList();
				string imageName = lines[0];

				// find the "repeat" option and skip unused data
				int repeatLine = (lines[3].IndexOf("repeat:") > -1) ? 3 : 4;
				lines.RemoveRange( 0, repeatLine + 1 );

				int numElementsPerImage = 7;
				int numImages = (int)(lines.Count / numElementsPerImage);

				int curIndex;
				int imageX;
				int imageY;

				int imageWidth;
				int imageHeight;

				List<int> size;
				string tempString;

				string name;
				bool rotated;
				float angle;
				LibGdxAtlasRect rect;
				LibGdxAtlasPoint sourceSize;
				LibGdxAtlasPoint offset;

				for (int i = 0 ; i < numImages ; i++)
				{
					curIndex = i * numElementsPerImage;

					name = lines[curIndex++];
					rotated = (lines[curIndex++].IndexOf("true") >= 0);
					angle = 0;

					tempString = lines[curIndex++];
					size = GetDimensions(tempString, context.Logger);

					imageX = size[0];
					imageY = size[1];

					tempString = lines[curIndex++];
					size = GetDimensions(tempString, context.Logger);

					imageWidth = size[0];
					imageHeight = size[1];

					rect = new LibGdxAtlasRect();
					if (rotated)
					{
						rect.x = imageX;
						rect.y = imageY;
						rect.w = imageHeight;
						rect.h = imageWidth;
						angle = 90;
					}
					else
					{
						rect.x = imageX;
						rect.y = imageY;
						rect.w = imageWidth;
						rect.h = imageHeight;
					}

					tempString = lines[curIndex++];
					GetDimensions(tempString, context.Logger);

					sourceSize = new LibGdxAtlasPoint(size[0], size[1]);

					tempString = lines[curIndex++];
					GetDimensions(tempString, context.Logger);

					offset = new LibGdxAtlasPoint(size[0], size[1]);
					LibGdxAtlasRegion region = new LibGdxAtlasRegion();
					region.sourceRectangle = rect;
					region.sourceSize = sourceSize;
					region.offset = offset;
					region.name = name;
					region.angle = angle;
					f.regions.Add( region );

				}

				f.imageName = imageName;
*/

				f.regions = regions;
				f.pages = pages;
				return f;
			}

		}


		private List<int> GetDimensions( string line, ContentBuildLogger logger )
		{
			int colonPosition = line.IndexOf( ':' );
			int comaPosition = line.IndexOf( ',' );

			List<int> res = new List<int>();
			string x = line.Substring( colonPosition + 1, comaPosition - colonPosition - 1 ).Trim();
			string y = line.Substring( comaPosition + 1, line.Length - comaPosition - 1 ).Trim();
			logger.LogMessage( x );
			logger.LogMessage( y );

			res.Add( int.Parse( x ) );
			res.Add( int.Parse( y ) );

			return res;
		}


		/** Returns the number of tuple values read (1, 2 or 4). */
		static int readTuple( StreamReader reader )
		{
			String line = reader.ReadLine();
			int colon = line.IndexOf( ':' );
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


		static String readValue( StreamReader reader )
		{
			String line = reader.ReadLine();
			int colon = line.IndexOf( ':' );
			if( colon == -1 )
				throw new Exception( "Invalid line: " + line );
			return line.Substring( colon + 1 ).Trim();
		}
	}
}

