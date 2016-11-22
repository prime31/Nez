using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// convenience base class for 3D objects. It reuses and wraps the Transform in Vector3s for easy access and provides a world
	/// transform for rendering.
	/// </summary>
	public abstract class Renderable3D : RenderableComponent
	{
		/// <summary>
		/// by default, uses a magic number of 1.5 * the scale of the object. This will work fine for objects ~1 unit in width/height.
		/// Any other odd sizes should override this appropriately.
		/// </summary>
		/// <value>The bounds.</value>
		public override RectangleF bounds
		{
			get
			{
				var sizeX = 1.5f * scale.X;
				var sizeY = 1.5f * scale.Y;
				var x = ( position.X - sizeX / 2 );
				var y = ( position.Y - sizeY / 2 );

				return new RectangleF( x, y, sizeX, sizeY );
			}
		}

		/// <summary>
		/// wraps Transform.position along with a private Z position
		/// </summary>
		/// <value>The position.</value>
		public Vector3 position
		{
			get { return new Vector3( transform.position, _positionZ ); }
			set
			{
				_positionZ = value.Z;
				transform.setPosition( value.X, value.Y );
			}
		}

		/// <summary>
		/// the scale of the object. 80 by default. You will need to adjust this depending on your Scene's backbuffer size.
		/// </summary>
		public Vector3 scale = new Vector3( 80f );

		/// <summary>
		/// wraps Transform.rotation for the Z rotation along with a private X and Y rotation.
		/// </summary>
		/// <value>The rotation.</value>
		public Vector3 rotation
		{
			get { return new Vector3( _rotationXY, transform.rotation ); }
			set
			{
				_rotationXY.X = value.X;
				_rotationXY.Y = value.Y;
				transform.setRotation( value.Z );
			}
		}

		/// <summary>
		/// rotation in degrees
		/// </summary>
		/// <value>The rotation degrees.</value>
		public Vector3 rotationDegrees
		{
			get { return new Vector3( _rotationXY, transform.rotation ) * Mathf.rad2Deg; }
			set { rotation = value *= Mathf.deg2Rad; }
		}

		/// <summary>
		/// Matrix that represents the world transform. Useful for rendering.
		/// </summary>
		/// <value>The world matrix.</value>
		public Matrix worldMatrix
		{
			get
			{
				// prep our rotations
				var rot = rotation;
				var rotationMatrix = Matrix.CreateRotationX( rot.X );
				rotationMatrix *= Matrix.CreateRotationY( rot.Y );
				rotationMatrix *= Matrix.CreateRotationZ( rot.Z );

				// remember to invert the sign of the y position!
				var pos = position;
				return rotationMatrix * Matrix.CreateScale( scale ) * Matrix.CreateTranslation( pos.X, -pos.Y, pos.Z );
			}
		}


		float _positionZ;
		Vector2 _rotationXY;
	}
}
