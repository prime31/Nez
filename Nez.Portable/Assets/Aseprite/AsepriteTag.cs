using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public sealed class AsepriteTag : IAsepriteUserData
	{
		private Color _oldVersionColor;

		public readonly int From;
		public readonly int To;
		public readonly AsepriteLoopDirection LoopDirection;
		public readonly string Name;

		public Color Color
		{
			get
			{
				if (UserData.HasColor)
				{
					return UserData.Color.Value;
				}

				return _oldVersionColor;
			}
		}

		public AsepriteUserData UserData { get; }

		internal AsepriteTag(int from, int to, AsepriteLoopDirection loopDirection, Color color, string name)
		{
			From = from;
			To = to;
			LoopDirection = loopDirection;
			_oldVersionColor = color;
			Name = name;
			UserData = new AsepriteUserData();
		}
	}
}