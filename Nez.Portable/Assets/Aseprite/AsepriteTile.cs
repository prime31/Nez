namespace Nez.Aseprite
{
	public sealed class AsepriteTile
	{
		public readonly uint ID;
		public readonly uint XFlip;
		public readonly uint YFlip;
		public readonly uint Rotate90;

		internal AsepriteTile(uint id, uint xFlip, uint yFlip, uint rotate)
		{
			ID = id;
			XFlip = xFlip;
			YFlip = yFlip;
			Rotate90 = rotate;
		}
	}
}