using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class EntityExt
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setParent( this Entity self, Transform parent )
		{
			self.transform.setParent( parent );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setParent( this Entity self, Entity entity )
		{
			self.transform.setParent( entity.transform );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setPosition( this Entity self, Vector2 position )
		{
			self.transform.setPosition( position );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setPosition( this Entity self, float x, float y )
		{
			self.transform.setPosition( x, y );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setLocalPosition( this Entity self, Vector2 localPosition )
		{
			self.transform.setLocalPosition( localPosition );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setRotation( this Entity self, float radians )
		{
			self.transform.setRotation( radians );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setRotationDegrees( this Entity self, float degrees )
		{
			self.transform.setRotationDegrees( degrees );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setLocalRotation( this Entity self, float radians )
		{
			self.transform.setLocalRotation( radians );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setLocalRotationDegrees( this Entity self, float degrees )
		{
			self.transform.setLocalRotationDegrees( degrees );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setScale( this Entity self, Vector2 scale )
		{
			self.transform.setScale( scale );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setScale( this Entity self, float scale )
		{
			self.transform.setScale( scale );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setLocalScale( this Entity self, Vector2 scale )
		{
			self.transform.setLocalScale( scale );
			return self;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Entity setLocalScale( this Entity self, float scale )
		{
			self.transform.setLocalScale( scale );
			return self;
		}

	}
}
