using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Tiled
{
	public abstract class TiledLayer
	{
		public Vector2 Offset;
		public string Name;
		public Dictionary<string, string> Properties;
		public bool Visible = true;
		public float Opacity;


		protected TiledLayer(string name)
		{
			this.Name = name;
			Properties = new Dictionary<string, string>();
		}


		public abstract void Draw(Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds);

		public abstract void Draw(Batcher batcher, Vector2 position, Vector2 scale, float layerDepth,
		                          RectangleF cameraClipBounds);
	}
}