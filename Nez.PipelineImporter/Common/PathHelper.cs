using System;
using System.IO;


namespace Nez.PipelineImporter
{
	public static class PathHelper
	{
		/// <summary>
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="UriFormatException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static string makeRelativePath( string fromPath, string toPath )
		{
			var fromUri = new Uri( fromPath );
			var toUri = new Uri( toPath );

			if( fromUri.Scheme != toUri.Scheme )
				return toPath; // path can't be made relative.

			var relativeUri = fromUri.MakeRelativeUri( toUri );
			var relativePath = Uri.UnescapeDataString( relativeUri.ToString() );

			if( toUri.Scheme.Equals( "file", StringComparison.InvariantCultureIgnoreCase ) )
				relativePath = relativePath.Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar );

			return relativePath;
		}


		public static string getAbsolutePath( string relativePath, string basePath )
		{
			if( relativePath == null )
				return null;
			if( basePath == null )
				basePath = Path.GetFullPath( "." ); // quick way of getting current working directory
			else
				basePath = getAbsolutePath( basePath, null ); // to be REALLY sure ;)

			// specific for windows paths starting on \ - they need the drive added to them.
			// I constructed this piece like this for possible Mono support.
			if( !Path.IsPathRooted( relativePath ) || "\\".Equals( Path.GetPathRoot( relativePath ) ) )
			{
				if( relativePath.StartsWith( Path.DirectorySeparatorChar.ToString() ) )
					return Path.GetFullPath( Path.Combine( Path.GetPathRoot( basePath ), relativePath.TrimStart( Path.DirectorySeparatorChar ) ) );
				else
					return Path.GetFullPath( Path.Combine( basePath, relativePath ) );
			}
			else
				return Path.GetFullPath( relativePath ); // resolves any internal "..\" to get the true full path.
		}
	}
}

