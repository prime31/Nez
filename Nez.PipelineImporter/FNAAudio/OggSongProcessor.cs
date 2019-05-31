using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace Nez.OggSongImporter
{
	[ContentProcessor( DisplayName = "Ogg Song - MonoGame" )]
	public class OggSongProcessor : ContentProcessor<AudioContent, SongContent>
	{
		public ConversionQuality Quality { get; set; } = ConversionQuality.Best;
		private static readonly ConstructorInfo _songContentConstructor = typeof( SongContent ).GetConstructor( BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof( string ), typeof( TimeSpan ) }, null );
		private static readonly SHA1Managed _sha1Managed = new SHA1Managed();


		public override SongContent Process( AudioContent input, ContentProcessorContext context )
		{
			var outputFilename = Path.ChangeExtension( context.OutputFilename, "ogg" );
			var songContentFilename = PathHelper.GetRelativePath( Path.GetDirectoryName( context.OutputFilename ) + Path.DirectorySeparatorChar, outputFilename );

			context.AddOutputFile( outputFilename );

			// Use the ogg-file as-is
			if( Path.GetExtension( input.FileName ) == "ogg" )
			{
				File.Copy( input.FileName, outputFilename );

				return (SongContent)_songContentConstructor.Invoke( new object[]
				{
					songContentFilename,
					input.Duration
				} );
			}

			// Prepare some useful paths and checks
			var hashFile = Path.ChangeExtension( input.FileName, "hash" );
			var oggFile = Path.ChangeExtension( input.FileName, "ogg" );
			var oggFileExists = File.Exists( oggFile );

			// Compare a previous hash, if there is one.
			var currentHash = CalculateSHA1( input.FileName );
			string previousHash = null;

			if( File.Exists( hashFile ) && oggFileExists )
			{
				previousHash = File.ReadAllText( hashFile );
			}
			else
			{
				File.WriteAllText( hashFile, currentHash );
			}

			// Determine if we can re-use a previously generated ogg-file
			if( oggFileExists && previousHash == currentHash )
			{
				File.Copy( oggFile, outputFilename );
			}
			else
			{
				var conversionQuality = AudioProfile.ForPlatform( TargetPlatform.DesktopGL )
					.ConvertStreamingAudio( TargetPlatform.DesktopGL, Quality, input, ref outputFilename );
				if( Quality != conversionQuality )
				{
					context.Logger.LogMessage( "Failed to convert using \"{0}\" quality, used \"{1}\" quality", Quality, conversionQuality );
				}

				if( oggFileExists )
				{
					File.Delete( oggFile );
				}

				File.Copy( outputFilename, oggFile );
			}

			return (SongContent)_songContentConstructor.Invoke( new object[]
			{
				songContentFilename,
				input.Duration
			} );
		}

		static string CalculateSHA1( string fileName )
		{
			using( var stream = new FileStream( fileName, FileMode.Open ) )
			{
				var hash = _sha1Managed.ComputeHash( stream );
				return string.Join( string.Empty, hash.Select( b => b.ToString( "X2" ) ).ToArray() ).ToLower();
			}
		}
	}
}
