namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a layer in an Aseprite file that tilemap cels are placed on.
	/// </summary>
	public sealed class AsepriteTilemapLayer : AsepriteLayer
	{
		/// <summary>
		/// The tileset that is used by all tilemap cels on this layer.
		/// </summary>
		public readonly AsepriteTileset Tileset;

		internal AsepriteTilemapLayer(AsepriteTileset tileset, bool isVisible, bool isBackground, bool isReference, int childLevel, AsepriteBlendMode blendMode, int opacity, string name)
			: base(isVisible, isBackground, isReference, childLevel, blendMode, opacity, name)
		{
			Tileset = tileset;
		}
	}
}