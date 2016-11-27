using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Nez
{
	public static class ComponentExt
	{
		#region Entity Component management
		
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T addComponent<T>( this Component self, T component ) where T : Component
		{
			return self.entity.addComponent( component );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T addComponent<T>( this Component self ) where T : Component, new()
		{
			return self.entity.addComponent<T>();
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T getComponent<T>( this Component self ) where T : Component
		{
			return self.entity.getComponent<T>();
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void getComponents<T>( this Component self, List<T> componentList ) where T : class
		{
			self.entity.getComponents<T>( componentList );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static List<T> getComponents<T>( this Component self ) where T : Component
		{
			return self.entity.getComponents<T>();
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool removeComponent<T>( this Component self ) where T : Component
		{
			return self.entity.removeComponent<T>();
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void removeComponent( this Component self, Component component )
		{
			self.entity.removeComponent( component );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void removeComponent( this Component self )
		{
			self.entity.removeComponent( self );
		}

		#endregion

	}
}

