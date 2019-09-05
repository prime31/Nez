using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class EntityExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetParent(this Entity self, Transform parent)
		{
			self.Transform.SetParent(parent);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetParent(this Entity self, Entity entity)
		{
			self.Transform.SetParent(entity.Transform);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetPosition(this Entity self, Vector2 position)
		{
			self.Transform.SetPosition(position);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetPosition(this Entity self, float x, float y)
		{
			self.Transform.SetPosition(x, y);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetLocalPosition(this Entity self, Vector2 localPosition)
		{
			self.Transform.SetLocalPosition(localPosition);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetRotation(this Entity self, float radians)
		{
			self.Transform.SetRotation(radians);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetRotationDegrees(this Entity self, float degrees)
		{
			self.Transform.SetRotationDegrees(degrees);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetLocalRotation(this Entity self, float radians)
		{
			self.Transform.SetLocalRotation(radians);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetLocalRotationDegrees(this Entity self, float degrees)
		{
			self.Transform.SetLocalRotationDegrees(degrees);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetScale(this Entity self, Vector2 scale)
		{
			self.Transform.SetScale(scale);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetScale(this Entity self, float scale)
		{
			self.Transform.SetScale(scale);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetLocalScale(this Entity self, Vector2 scale)
		{
			self.Transform.SetLocalScale(scale);
			return self;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity SetLocalScale(this Entity self, float scale)
		{
			self.Transform.SetLocalScale(scale);
			return self;
		}
	}
}