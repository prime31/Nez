using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Nez
{
	public static class ComponentExt
	{
		#region Entity Component management

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T AddComponent<T>(this Component self, T component) where T : Component
		{
			return self.Entity.AddComponent(component);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T AddComponent<T>(this Component self) where T : Component, new()
		{
			return self.Entity.AddComponent<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetComponent<T>(this Component self) where T : class
		{
			return self.Entity.GetComponent<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasComponent<T>(this Component self) where T : class => self.Entity.HasComponent<T>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetComponents<T>(this Component self, List<T> componentList) where T : class
		{
			self.Entity.GetComponents<T>(componentList);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<T> GetComponents<T>(this Component self) where T : class
		{
			return self.Entity.GetComponents<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveComponent<T>(this Component self) where T : Component
		{
			return self.Entity.RemoveComponent<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveComponent(this Component self, Component component)
		{
			self.Entity.RemoveComponent(component);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveComponent(this Component self)
		{
			self.Entity.RemoveComponent(self);
		}

		#endregion
	}
}
