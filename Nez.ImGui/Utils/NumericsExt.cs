using Microsoft.Xna.Framework;
using Num = System.Numerics;


namespace Nez.ImGuiTools
{
	/// <summary>
	/// helpers to convert to/from System.Numberics
	/// </summary>
	public static class NumericsExt
	{
		public static Vector2 ToXNA(this Num.Vector2 self) => new Vector2(self.X, self.Y);

		public static Num.Vector2 ToNumerics(this Vector2 self) => new Num.Vector2(self.X, self.Y);

		public static Num.Vector2 ToNumerics(this Point self) => new Num.Vector2(self.X, self.Y);

		public static Vector3 ToXNA(this Num.Vector3 self) => new Vector3(self.X, self.Y, self.Z);

		public static Num.Vector3 ToNumerics(this Vector3 self) => new Num.Vector3(self.X, self.Y, self.Z);

		public static Vector4 ToXNA(this Num.Vector4 self) => new Vector4(self.X, self.Y, self.Z, self.W);

		public static Num.Vector4 ToNumerics(this Vector4 self) => new Num.Vector4(self.X, self.Y, self.Z, self.W);

		public static Num.Vector4 ToNumerics(this Color self) => new Num.Vector4(self.R / 255.0f, self.G / 255.0f, self.B / 255.0f, self.A / 255.0f);

		public static Color ToXNAColor(this Num.Vector4 self) => new Color(self.X * 1.0f, self.Y * 1.0f, self.Z * 1.0f, self.W * 1.0f);
	}
}