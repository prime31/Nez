using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Nez.TextureAtlasGenerator
{
	/// <summary>
	/// Build-time type used to hold the output data from the SpriteSheetProcessor.
	/// This is serialized into XNB format, then at runtime, the ContentManager
	/// loads the data into a SpriteSheet object.
	/// </summary>
	public class TextureAtlasContent
	{
		/// <summary>
		/// Single texture contains many separate sprite images.
		/// </summary>
		public Texture2DContent texture = new Texture2DContent();

		/// <summary>
		/// Remember where in the texture each sprite has been placed.
		/// </summary>
		public List<Rectangle> spriteRectangles = new List<Rectangle>();

		/// <summary>
		/// Store the original sprite filenames, so we can look up sprites by name.
		/// </summary>
		public Dictionary<string,int> spriteNames = new Dictionary<string,int>();

		/// <summary>
		/// stores the folder name (ie animation name) and a point. The point.x represents the index of the first frame
		/// and point.y is the last frame. The indexes are for the spriteRectangles array
		/// </summary>
		public Dictionary<string,Point> spriteAnimationDetails = new Dictionary<string,Point>();

		/// <summary>
		/// optional nine-slice padding for a sprite
		/// </summary>
		public Dictionary<string,int[]> nineSliceSplits = new Dictionary<string,int[]>();

		public int animationFPS;
	}
}
