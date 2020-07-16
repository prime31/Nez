#if FNA
using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// implements some methods available in MonoGame that do not exist in FNA/XNA to make transitioning a codebase from MG/XNA to FNA
	/// a little bit easier.
	/// </summary>
	public static class MonoGameCompat
	{
		#region GraphicsDevice

		public static void DrawIndexedPrimitives( this GraphicsDevice self, PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount )
		{
			#if DEBUG
			Core.drawCalls++;
			#endif
			self.DrawIndexedPrimitives( primitiveType, baseVertex, 0, primitiveCount * 2, startIndex, primitiveCount );
		}


		/// <summary>
		/// it is recommended to use GetRenderTargets() to avoid the extra Array.Copy when using FNA
		/// </summary>
		/// <returns>The render targets.</returns>
		/// <param name="self">Self.</param>
		/// <param name="outTargets">Out targets.</param>
		public static void GetRenderTargets( this GraphicsDevice self, RenderTargetBinding[] outTargets )
		{
			var currentRenderTargets = self.GetRenderTargets();
			System.Diagnostics.Debug.Assert( outTargets.Length == currentRenderTargets.Length, "Invalid outTargets array length!" );
			Array.Copy( currentRenderTargets, outTargets, currentRenderTargets.Length );
		}

		#endregion


		#region Point

		/// <summary>
		/// Gets a <see cref="Vector2"/> representation for this object.
		/// </summary>
		/// <returns>A <see cref="Vector2"/> representation for this object.</returns>
		public static Vector2 ToVector2( this Point self )
		{
			return new Vector2( (float)self.X, (float)self.Y );
		}

		#endregion


		#region Vector2

		/// <summary>
		/// Gets a <see cref="Point"/> representation for this object.
		/// </summary>
		/// <returns>A <see cref="Point"/> representation for this object.</returns>
		public static Point ToPoint( this Vector2 self )
		{
			return new Point( (int)self.X, (int)self.Y );
		}

		#endregion


		#region Rectangle

		public static bool Contains( this Rectangle rect, Vector2 value )
		{
			return ( ( ( ( rect.X <= value.X ) && ( value.X < ( rect.X + rect.Width ) ) ) && ( rect.Y <= value.Y ) ) && ( value.Y < ( rect.Y + rect.Height ) ) );
		}

		#endregion

	}
}


namespace Microsoft.Xna.Framework.Graphics
{
	[StructLayout( LayoutKind.Sequential, Pack = 1 )]
	public struct VertexPosition : IVertexType
	{
		public Vector3 Position;

		public static readonly VertexDeclaration VertexDeclaration;

		public VertexPosition( Vector3 position )
		{
			Position = position;
		}

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return VertexDeclaration; }
		}

		public override int GetHashCode()
		{
			return Position.GetHashCode();
		}

		public override string ToString()
		{
			return "{{Position:" + Position + "}}";
		}

		public static bool operator ==( VertexPosition left, VertexPosition right )
		{
			return left.Position == right.Position;
		}

		public static bool operator !=( VertexPosition left, VertexPosition right )
		{
			return !( left == right );
		}

		public override bool Equals( object obj )
		{
			if( obj == null )
			{
				return false;
			}
			if( obj.GetType() != GetType() )
			{
				return false;
			}
			return this == (VertexPosition)obj;
		}

		static VertexPosition()
		{
			VertexElement[] elements =
 { new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ) };
			VertexDeclaration declaration = new VertexDeclaration( elements );
			VertexDeclaration = declaration;
		}
	}
}
#endif