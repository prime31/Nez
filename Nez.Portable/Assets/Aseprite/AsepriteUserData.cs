using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public sealed class AsepriteUserData
	{
		public bool HasText
		{
			get
			{
				return string.IsNullOrEmpty(Text);
			}
		}

		public bool HasColor
		{
			get
			{
				return Color != null;
			}
		}

		public string Text;
		public Color? Color;

		internal AsepriteUserData() { }
	}
}