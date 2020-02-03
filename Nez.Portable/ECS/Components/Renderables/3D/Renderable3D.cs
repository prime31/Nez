using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// convenience base class for 3D objects. It reuses and wraps the Transform in Vector3s for easy access and provides a world
	/// transform for rendering.
	/// </summary>
	public abstract class Renderable3D : RenderableComponent
	{
		float _positionZ;
		Vector2 _rotationXY;

		/// <summary>
		/// by default, uses a magic number of 1.5 * the scale of the object. This will work fine for objects ~1 unit in width/height.
		/// Any other odd sizes should override this appropriately.
		/// </summary>
		/// <value>The bounds.</value>
		public override RectangleF Bounds
		{
			get
			{
				var sizeX = 1.5f * Scale.X;
				var sizeY = 1.5f * Scale.Y;
				var x = (Position.X - sizeX / 2);
				var y = (Position.Y - sizeY / 2);

				return new RectangleF(x, y, sizeX, sizeY);
			}
		}

		/// <summary>
		/// wraps Transform.position along with a private Z position
		/// </summary>
		/// <value>The position.</value>
		public Vector3 Position
		{
			get => new Vector3(Transform.Position, _positionZ);
			set
			{
				_positionZ = value.Z;
				Transform.SetPosition(value.X, value.Y);
			}
		}

		/// <summary>
		/// the scale of the object. 80 by default. You will need to adjust this depending on your Scene's backbuffer size.
		/// </summary>
		public Vector3 Scale = new Vector3(80f);

		/// <summary>
		/// wraps Transform.rotation for the Z rotation along with a private X and Y rotation.
		/// </summary>
		/// <value>The rotation.</value>
		public Vector3 Rotation
		{
			get => new Vector3(_rotationXY, Transform.Rotation);
			set
			{
				_rotationXY.X = value.X;
				_rotationXY.Y = value.Y;
				Transform.SetRotation(value.Z);
			}
		}

		/// <summary>
		/// rotation in degrees
		/// </summary>
		/// <value>The rotation degrees.</value>
		public Vector3 RotationDegrees
		{
			get => new Vector3(_rotationXY, Transform.Rotation) * Mathf.Rad2Deg;
			set => Rotation = value *= Mathf.Deg2Rad;
		}

		/// <summary>
		/// Matrix that represents the world transform. Useful for rendering.
		/// </summary>
		/// <value>The world matrix.</value>
		public Matrix WorldMatrix
		{
			get
			{
				// prep our rotations
				var rot = Rotation;
				var rotationMatrix = Matrix.CreateRotationX(rot.X);
				rotationMatrix *= Matrix.CreateRotationY(rot.Y);
				rotationMatrix *= Matrix.CreateRotationZ(rot.Z);

				// remember to invert the sign of the y position!
				var pos = Position;
				return rotationMatrix * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(pos.X, -pos.Y, pos.Z);
			}
		}
	}
}