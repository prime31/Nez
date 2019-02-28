using System;
using System.Collections.Generic;

namespace Nez.ImGuiTools
{
	public static class InspectorCache
	{
		static Type[] _componentSubclasses;
		
		/// <summary>
		/// gets all the Component subclasses that have a parameterless constructor
		/// </summary>
		/// <returns></returns>
		public static Type[] getAllComponentTypes()
		{
			if( _componentSubclasses == null )
			{
				var subclasses = ReflectionUtils.getAllTypesAssignableFrom( typeof( Component ), true );
				// sort so the Colliders are on the bottom
				subclasses.Sort( (t, u) =>
				{
					var tIsCollider = typeof( Collider ).IsAssignableFrom( t );
					var uIsCollider = typeof( Collider ).IsAssignableFrom( u );

					if( tIsCollider && uIsCollider )
						return t.Name.CompareTo( u.Name );
					if( !tIsCollider && !uIsCollider )
					{
						var tIsNez = t.Namespace.StartsWith( "Nez" );
						var uIsNez = u.Namespace.StartsWith( "Nez" );

						if( tIsNez && uIsNez || !tIsNez && !uIsNez )
							return t.Name.CompareTo( u.Name );
						
						if( tIsNez && !uIsNez )
							return 1;
						return -1;
					}
					if( tIsCollider && !uIsCollider )
						return 1;
					return -1;
				} );
				_componentSubclasses = subclasses.ToArray();
			}
			return _componentSubclasses;
		}
   
	}
}