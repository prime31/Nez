using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// convenience base class for 3D objects. It reuses and wraps the Transform in Vector3s for easy access and provides a world
	/// transform for rendering. Note that bounds is very dumb and you should override it with something more intelligible.
	/// </summary>
	public abstract class Renderable3D : RenderableComponent
	{
		public override RectangleF bounds
		{
			get
			{
				var sphere = new BoundingSphere( Vector3.Zero, 999999 );
				var sizeX = sphere.Radius * 2 * scale.X;
				var sizeY = sphere.Radius * 2 * scale.Y;
				var x = ( position.X + sphere.Center.X - sizeX / 2 );
				var y = ( position.Y + sphere.Center.Y - sizeY / 2 );

				return new RectangleF( x, y, sizeX, sizeY );
			}
		}

		public Vector3 position
		{
			get { return new Vector3( transform.position, _positionZ ); }
			set
			{
				_positionZ = value.Z;
				transform.setPosition( value.X, value.Y );
			}
		}

		public Vector3 scale
		{
			get { return new Vector3( transform.scale, _scaleZ ); }
			set
			{
				_scaleZ = value.Z;
				transform.setScale( new Vector2( value.X, value.Y ) );
			}
		}

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

		public Vector3 rotationDegrees
		{
			get { return new Vector3( _rotationXY, transform.rotation ) * 57.295779513082320876798154814105f; }
			set { rotation = value *= 0.017453292519943295769236907684886f; }
		}

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
		float _scaleZ;
		Vector2 _rotationXY;


		public override void onAddedToEntity()
		{
			scale = Vector3.One * 80;
		}

	}
}
