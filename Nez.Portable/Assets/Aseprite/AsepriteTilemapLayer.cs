namespace Nez.Aseprite
{
	public sealed class AsepriteTilemapLayer : AsepriteLayer
	{
		public readonly AsepriteTileset Tileset;

		internal AsepriteTilemapLayer(AsepriteTileset tileset, bool isVisible, bool isBackground, bool isReference, int childLevel, AsepriteBlendMode blendMode, int opacity, string name)
			: base(isVisible, isBackground, isReference, childLevel, blendMode, opacity, name)
		{
			Tileset = tileset;
		}
	}
}