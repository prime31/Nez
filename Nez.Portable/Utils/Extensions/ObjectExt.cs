using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Nez.Tweens;


namespace Nez
{
	public static class ObjectExt
	{
		/// <summary>
		/// tweens an int field or property
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ITween<int> Tween(this object self, string memberName, int to, float duration)
		{
			return PropertyTweens.IntPropertyTo(self, memberName, to, duration);
		}


		/// <summary>
		/// tweens a float field or property
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ITween<float> Tween(this object self, string memberName, float to, float duration)
		{
			return PropertyTweens.FloatPropertyTo(self, memberName, to, duration);
		}


		/// <summary>
		/// tweens a Color field or property
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ITween<Color> Tween(this object self, string memberName, Color to, float duration)
		{
			return PropertyTweens.ColorPropertyTo(self, memberName, to, duration);
		}


		/// <summary>
		/// tweens a Vector2 field or property
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ITween<Vector2> Tween(this object self, string memberName, Vector2 to, float duration)
		{
			return PropertyTweens.Vector2PropertyTo(self, memberName, to, duration);
		}


		/// <summary>
		/// tweens a Vector3 field or property
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ITween<Vector3> Tween(this object self, string memberName, Vector3 to, float duration)
		{
			return PropertyTweens.Vector3PropertyTo(self, memberName, to, duration);
		}
	}
}