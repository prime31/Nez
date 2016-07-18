using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Represents the right-handed 3x3 floating point matrix, which can store translation, scale and rotation information.
	/// </summary>
	[DebuggerDisplay( "{DebugDisplayString,nq}" )]
	public struct Matrix2D : IEquatable<Matrix2D>
	{
		#region Public Fields

		public float M11;
		public float M12;
		public float M13;
		public float M14;
		public float M21;
		public float M22;
		public float M23;
		public float M24;

		/// <summary>
		/// A third row and first column value.
		/// </summary>
		[DataMember]
		public float M31;

		/// <summary>
		/// A third row and second column value.
		/// </summary>
		[DataMember]
		public float M32;

		/// <summary>
		/// A third row and third column value.
		/// </summary>
		[DataMember]
		public float M33;

		/// <summary>
		/// A third row and fourth column value.
		/// </summary>
		[DataMember]
		public float M34;

		/// <summary>
		/// A fourth row and first column value.
		/// </summary>
		[DataMember]
		public float M41;

		/// <summary>
		/// A fourth row and second column value.
		/// </summary>
		[DataMember]
		public float M42;

		/// <summary>
		/// A fourth row and third column value.
		/// </summary>
		[DataMember]
		public float M43;

		/// <summary>
		/// A fourth row and fourth column value.
		/// </summary>
		[DataMember]
		public float M44;

		#endregion


		#region Indexers

		public float this[int index]
		{
			get
			{
				switch( index )
				{
					case 0: return M11;
					case 1: return M12;
					case 2: return M13;
					case 3: return M14;
					case 4: return M21;
					case 5: return M22;
					case 6: return M23;
					case 7: return M24;
					case 8: return M31;
					case 9: return M32;
					case 10: return M33;
					case 11: return M34;
					case 12: return M41;
					case 13: return M42;
					case 14: return M43;
					case 15: return M44;
				}
				throw new ArgumentOutOfRangeException();
			}

			set
			{
				switch( index )
				{
					case 0: M11 = value; break;
					case 1: M12 = value; break;
					case 2: M13 = value; break;
					case 3: M14 = value; break;
					case 4: M21 = value; break;
					case 5: M22 = value; break;
					case 6: M23 = value; break;
					case 7: M24 = value; break;
					case 8: M31 = value; break;
					case 9: M32 = value; break;
					case 10: M33 = value; break;
					case 11: M34 = value; break;
					case 12: M41 = value; break;
					case 13: M42 = value; break;
					case 14: M43 = value; break;
					case 15: M44 = value; break;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public float this[int row, int column]
		{
			get
			{
				return this[( row * 4 ) + column];
			}

			set
			{
				this[( row * 4 ) + column] = value;
			}
		}

		#endregion


		#region Private Members

		private static Matrix2D identity = new Matrix2D( 1f, 0f, 0f, 0f,
													0f, 1f, 0f, 0f,
													0f, 0f, 1f, 0f,
													0f, 0f, 0f, 1f );
		#endregion


		#region Public Properties

		/// <summary>
		/// The backward vector formed from the third row M31, M32, M33 elements.
		/// </summary>
		public Vector2 Backward
		{
			get
			{
				return new Vector2( this.M31, this.M32 );
			}
			set
			{
				this.M31 = value.X;
				this.M32 = value.Y;
			}
		}

		/// <summary>
		/// The down vector formed from the second row -M21, -M22, -M23 elements.
		/// </summary>
		public Vector2 Down
		{
			get
			{
				return new Vector2( -this.M21, -this.M22 );
			}
			set
			{
				this.M21 = -value.X;
				this.M22 = -value.Y;
			}
		}

		/// <summary>
		/// The forward vector formed from the third row -M31, -M32, -M33 elements.
		/// </summary>
		public Vector2 Forward
		{
			get
			{
				return new Vector2( -this.M31, -this.M32 );
			}
			set
			{
				this.M31 = -value.X;
				this.M32 = -value.Y;
			}
		}

		/// <summary>
		/// Returns the identity matrix.
		/// </summary>
		public static Matrix2D Identity
		{
			get { return identity; }
		}

		/// <summary>
		/// The left vector formed from the first row -M11, -M12, -M13 elements.
		/// </summary>
		public Vector2 Left
		{
			get
			{
				return new Vector2( -this.M11, -this.M12 );
			}
			set
			{
				this.M11 = -value.X;
				this.M12 = -value.Y;
			}
		}

		/// <summary>
		/// The right vector formed from the first row M11, M12, M13 elements.
		/// </summary>
		public Vector2 Right
		{
			get
			{
				return new Vector2( this.M11, this.M12 );
			}
			set
			{
				this.M11 = value.X;
				this.M12 = value.Y;
			}
		}


		/// <summary>
		/// Position stored in this matrix.
		/// </summary>
		public Vector2 Translation
		{
			get
			{
				return new Vector2( this.M41, this.M42 );
			}
			set
			{
				this.M41 = value.X;
				this.M42 = value.Y;
			}
		}

		/// <summary>
		/// Scale stored in this matrix.
		/// </summary>
		public Vector2 Scale
		{
			get
			{
				return new Vector2( this.M11, this.M22 );
			}
			set
			{
				this.M11 = value.X;
				this.M22 = value.Y;
			}
		}

		/// <summary>
		/// The upper vector formed from the second row M21, M22, M23 elements.
		/// </summary>
		public Vector2 Up
		{
			get
			{
				return new Vector2( this.M21, this.M22 );
			}
			set
			{
				this.M21 = value.X;
				this.M22 = value.Y;
			}
		}

		#endregion


		#region Public Constructors

		/// <summary>
		/// Constructs a matrix.
		/// </summary>
		public Matrix2D( float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31,
					  float m32, float m33, float m34, float m41, float m42, float m43, float m44 )
		{
			this.M11 = m11;
			this.M12 = m12;
			this.M13 = m13;
			this.M14 = m14;
			this.M21 = m21;
			this.M22 = m22;
			this.M23 = m23;
			this.M24 = m24;
			this.M31 = m31;
			this.M32 = m32;
			this.M33 = m33;
			this.M34 = m34;
			this.M41 = m41;
			this.M42 = m42;
			this.M43 = m43;
			this.M44 = m44;
		}

		/// <summary>
		/// Constructs a matrix.
		/// </summary>
		/// <param name="row1">A first row of the created matrix.</param>
		/// <param name="row2">A second row of the created matrix.</param>
		/// <param name="row3">A third row of the created matrix.</param>
		/// <param name="row4">A fourth row of the created matrix.</param>
		public Matrix2D( Vector4 row1, Vector4 row2, Vector4 row3, Vector4 row4 )
		{
			this.M11 = row1.X;
			this.M12 = row1.Y;
			this.M13 = row1.Z;
			this.M14 = row1.W;
			this.M21 = row2.X;
			this.M22 = row2.Y;
			this.M23 = row2.Z;
			this.M24 = row2.W;
			this.M31 = row3.X;
			this.M32 = row3.Y;
			this.M33 = row3.Z;
			this.M34 = row3.W;
			this.M41 = row4.X;
			this.M42 = row4.Y;
			this.M43 = row4.Z;
			this.M44 = row4.W;
		}

		#endregion


		#region Public Methods

		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> which contains sum of two matrixes.
		/// </summary>
		/// <param name="matrix1">The first matrix to add.</param>
		/// <param name="matrix2">The second matrix to add.</param>
		/// <returns>The result of the matrix addition.</returns>
		public static Matrix2D Add( Matrix2D matrix1, Matrix2D matrix2 )
		{
			matrix1.M11 += matrix2.M11;
			matrix1.M12 += matrix2.M12;
			matrix1.M13 += matrix2.M13;
			matrix1.M14 += matrix2.M14;
			matrix1.M21 += matrix2.M21;
			matrix1.M22 += matrix2.M22;
			matrix1.M23 += matrix2.M23;
			matrix1.M24 += matrix2.M24;
			matrix1.M31 += matrix2.M31;
			matrix1.M32 += matrix2.M32;
			matrix1.M33 += matrix2.M33;
			matrix1.M34 += matrix2.M34;
			matrix1.M41 += matrix2.M41;
			matrix1.M42 += matrix2.M42;
			matrix1.M43 += matrix2.M43;
			matrix1.M44 += matrix2.M44;
			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> which contains sum of two matrixes.
		/// </summary>
		/// <param name="matrix1">The first matrix to add.</param>
		/// <param name="matrix2">The second matrix to add.</param>
		/// <param name="result">The result of the matrix addition as an output parameter.</param>
		public static void Add( ref Matrix2D matrix1, ref Matrix2D matrix2, out Matrix2D result )
		{
			result.M11 = matrix1.M11 + matrix2.M11;
			result.M12 = matrix1.M12 + matrix2.M12;
			result.M13 = matrix1.M13 + matrix2.M13;
			result.M14 = matrix1.M14 + matrix2.M14;
			result.M21 = matrix1.M21 + matrix2.M21;
			result.M22 = matrix1.M22 + matrix2.M22;
			result.M23 = matrix1.M23 + matrix2.M23;
			result.M24 = matrix1.M24 + matrix2.M24;
			result.M31 = matrix1.M31 + matrix2.M31;
			result.M32 = matrix1.M32 + matrix2.M32;
			result.M33 = matrix1.M33 + matrix2.M33;
			result.M34 = matrix1.M34 + matrix2.M34;
			result.M41 = matrix1.M41 + matrix2.M41;
			result.M42 = matrix1.M42 + matrix2.M42;
			result.M43 = matrix1.M43 + matrix2.M43;
			result.M44 = matrix1.M44 + matrix2.M44;

		}


		/// <summary>
		/// Creates a new projection <see cref="Matrix2D"/> for orthographic view.
		/// </summary>
		/// <param name="width">Width of the viewing volume.</param>
		/// <param name="height">Height of the viewing volume.</param>
		/// <param name="zNearPlane">Depth of the near plane.</param>
		/// <param name="zFarPlane">Depth of the far plane.</param>
		/// <returns>The new projection <see cref="Matrix2D"/> for orthographic view.</returns>
		public static Matrix2D CreateOrthographic( float width, float height, float zNearPlane, float zFarPlane )
		{
			Matrix2D matrix;
			CreateOrthographic( width, height, zNearPlane, zFarPlane, out matrix );
			return matrix;
		}


		/// <summary>
		/// Creates a new projection <see cref="Matrix2D"/> for orthographic view.
		/// </summary>
		/// <param name="width">Width of the viewing volume.</param>
		/// <param name="height">Height of the viewing volume.</param>
		/// <param name="zNearPlane">Depth of the near plane.</param>
		/// <param name="zFarPlane">Depth of the far plane.</param>
		/// <param name="result">The new projection <see cref="Matrix2D"/> for orthographic view as an output parameter.</param>
		public static void CreateOrthographic( float width, float height, float zNearPlane, float zFarPlane, out Matrix2D result )
		{
			result.M11 = 2f / width;
			result.M12 = result.M13 = result.M14 = 0f;
			result.M22 = 2f / height;
			result.M21 = result.M23 = result.M24 = 0f;
			result.M33 = 1f / ( zNearPlane - zFarPlane );
			result.M31 = result.M32 = result.M34 = 0f;
			result.M41 = result.M42 = 0f;
			result.M43 = zNearPlane / ( zNearPlane - zFarPlane );
			result.M44 = 1f;
		}


		/// <summary>
		/// Creates a new projection <see cref="Matrix2D"/> for customized orthographic view.
		/// </summary>
		/// <param name="left">Lower x-value at the near plane.</param>
		/// <param name="right">Upper x-value at the near plane.</param>
		/// <param name="bottom">Lower y-coordinate at the near plane.</param>
		/// <param name="top">Upper y-value at the near plane.</param>
		/// <param name="zNearPlane">Depth of the near plane.</param>
		/// <param name="zFarPlane">Depth of the far plane.</param>
		/// <returns>The new projection <see cref="Matrix2D"/> for customized orthographic view.</returns>
		public static Matrix2D CreateOrthographicOffCenter( float left, float right, float bottom, float top, float zNearPlane, float zFarPlane )
		{
			Matrix2D matrix;
			CreateOrthographicOffCenter( left, right, bottom, top, zNearPlane, zFarPlane, out matrix );
			return matrix;
		}


		/// <summary>
		/// Creates a new projection <see cref="Matrix2D"/> for customized orthographic view.
		/// </summary>
		/// <param name="viewingVolume">The viewing volume.</param>
		/// <param name="zNearPlane">Depth of the near plane.</param>
		/// <param name="zFarPlane">Depth of the far plane.</param>
		/// <returns>The new projection <see cref="Matrix2D"/> for customized orthographic view.</returns>
		public static Matrix2D CreateOrthographicOffCenter( Rectangle viewingVolume, float zNearPlane, float zFarPlane )
		{
			Matrix2D matrix;
			CreateOrthographicOffCenter( viewingVolume.Left, viewingVolume.Right, viewingVolume.Bottom, viewingVolume.Top, zNearPlane, zFarPlane, out matrix );
			return matrix;
		}


		/// <summary>
		/// Creates a new projection <see cref="Matrix2D"/> for customized orthographic view.
		/// </summary>
		/// <param name="left">Lower x-value at the near plane.</param>
		/// <param name="right">Upper x-value at the near plane.</param>
		/// <param name="bottom">Lower y-coordinate at the near plane.</param>
		/// <param name="top">Upper y-value at the near plane.</param>
		/// <param name="zNearPlane">Depth of the near plane.</param>
		/// <param name="zFarPlane">Depth of the far plane.</param>
		/// <param name="result">The new projection <see cref="Matrix2D"/> for customized orthographic view as an output parameter.</param>
		public static void CreateOrthographicOffCenter( float left, float right, float bottom, float top, float zNearPlane, float zFarPlane, out Matrix2D result )
		{
			result.M11 = (float)( 2.0 / ( (double)right - (double)left ) );
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = (float)( 2.0 / ( (double)top - (double)bottom ) );
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = (float)( 1.0 / ( (double)zNearPlane - (double)zFarPlane ) );
			result.M34 = 0.0f;
			result.M41 = (float)( ( (double)left + (double)right ) / ( (double)left - (double)right ) );
			result.M42 = (float)( ( (double)top + (double)bottom ) / ( (double)bottom - (double)top ) );
			result.M43 = (float)( (double)zNearPlane / ( (double)zNearPlane - (double)zFarPlane ) );
			result.M44 = 1.0f;
		}


		/// <summary>
		/// Creates a new rotation <see cref="Matrix2D"/> around Z axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>The rotation <see cref="Matrix2D"/> around Z axis.</returns>
		public static Matrix2D CreateRotationZ( float radians )
		{
			Matrix2D result;
			CreateRotationZ( radians, out result );
			return result;
		}


		/// <summary>
		/// Creates a new rotation <see cref="Matrix2D"/> around Z axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix2D"/> around Z axis as an output parameter.</param>
		public static void CreateRotationZ( float radians, out Matrix2D result )
		{
			result = Matrix2D.Identity;

			var val1 = (float)Math.Cos( radians );
			var val2 = (float)Math.Sin( radians );

			result.M11 = val1;
			result.M12 = val2;
			result.M21 = -val2;
			result.M22 = val1;
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="scale">Scale value for all three axises.</param>
		/// <returns>The scaling <see cref="Matrix2D"/>.</returns>
		public static Matrix2D CreateScale( float scale )
		{
			Matrix2D result;
			CreateScale( scale, scale, scale, out result );
			return result;
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="scale">Scale value for all three axises.</param>
		/// <param name="result">The scaling <see cref="Matrix2D"/> as an output parameter.</param>
		public static void CreateScale( float scale, out Matrix2D result )
		{
			CreateScale( scale, scale, scale, out result );
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xScale">Scale value for X axis.</param>
		/// <param name="yScale">Scale value for Y axis.</param>
		/// <param name="zScale">Scale value for Z axis.</param>
		/// <returns>The scaling <see cref="Matrix2D"/>.</returns>
		public static Matrix2D CreateScale( float xScale, float yScale, float zScale )
		{
			Matrix2D result;
			CreateScale( xScale, yScale, zScale, out result );
			return result;
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xScale">Scale value for X axis.</param>
		/// <param name="yScale">Scale value for Y axis.</param>
		/// <param name="zScale">Scale value for Z axis.</param>
		/// <param name="result">The scaling <see cref="Matrix2D"/> as an output parameter.</param>
		public static void CreateScale( float xScale, float yScale, float zScale, out Matrix2D result )
		{
			result.M11 = xScale;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = yScale;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = zScale;
			result.M34 = 0;
			result.M41 = 0;
			result.M42 = 0;
			result.M43 = 0;
			result.M44 = 1;
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="scales"><see cref="Vector3"/> representing x,y and z scale values.</param>
		/// <returns>The scaling <see cref="Matrix2D"/>.</returns>
		public static Matrix2D CreateScale( Vector3 scales )
		{
			Matrix2D result;
			CreateScale( ref scales, out result );
			return result;
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="scales"><see cref="Vector3"/> representing x,y and z scale values.</param>
		/// <param name="result">The scaling <see cref="Matrix2D"/> as an output parameter.</param>
		public static void CreateScale( ref Vector3 scales, out Matrix2D result )
		{
			result.M11 = scales.X;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = scales.Y;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = scales.Z;
			result.M34 = 0;
			result.M41 = 0;
			result.M42 = 0;
			result.M43 = 0;
			result.M44 = 1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that flattens geometry into a specified <see cref="Plane"/> as if casting a shadow from a specified light source. 
		/// </summary>
		/// <param name="lightDirection">A vector specifying the direction from which the light that will cast the shadow is coming.</param>
		/// <param name="plane">The plane onto which the new matrix should flatten geometry so as to cast a shadow.</param>
		/// <returns>A <see cref="Matrix2D"/> that can be used to flatten geometry onto the specified plane from the specified direction. </returns>
		public static Matrix2D CreateShadow( Vector3 lightDirection, Plane plane )
		{
			Matrix2D result;
			CreateShadow( ref lightDirection, ref plane, out result );
			return result;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that flattens geometry into a specified <see cref="Plane"/> as if casting a shadow from a specified light source. 
		/// </summary>
		/// <param name="lightDirection">A vector specifying the direction from which the light that will cast the shadow is coming.</param>
		/// <param name="plane">The plane onto which the new matrix should flatten geometry so as to cast a shadow.</param>
		/// <param name="result">A <see cref="Matrix2D"/> that can be used to flatten geometry onto the specified plane from the specified direction as an output parameter.</param>
		public static void CreateShadow( ref Vector3 lightDirection, ref Plane plane, out Matrix2D result )
		{
			float dot = ( plane.Normal.X * lightDirection.X ) + ( plane.Normal.Y * lightDirection.Y ) + ( plane.Normal.Z * lightDirection.Z );
			float x = -plane.Normal.X;
			float y = -plane.Normal.Y;
			float z = -plane.Normal.Z;
			float d = -plane.D;

			result.M11 = ( x * lightDirection.X ) + dot;
			result.M12 = x * lightDirection.Y;
			result.M13 = x * lightDirection.Z;
			result.M14 = 0;
			result.M21 = y * lightDirection.X;
			result.M22 = ( y * lightDirection.Y ) + dot;
			result.M23 = y * lightDirection.Z;
			result.M24 = 0;
			result.M31 = z * lightDirection.X;
			result.M32 = z * lightDirection.Y;
			result.M33 = ( z * lightDirection.Z ) + dot;
			result.M34 = 0;
			result.M41 = d * lightDirection.X;
			result.M42 = d * lightDirection.Y;
			result.M43 = d * lightDirection.Z;
			result.M44 = dot;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <param name="zPosition">Z coordinate of translation.</param>
		/// <returns>The translation <see cref="Matrix2D"/>.</returns>
		public static Matrix2D CreateTranslation( float xPosition, float yPosition, float zPosition )
		{
			Matrix2D result;
			CreateTranslation( xPosition, yPosition, zPosition, out result );
			return result;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <param name="result">The translation <see cref="Matrix2D"/> as an output parameter.</param>
		public static void CreateTranslation( ref Vector3 position, out Matrix2D result )
		{
			result.M11 = 1;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = 1;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = 1;
			result.M34 = 0;
			result.M41 = position.X;
			result.M42 = position.Y;
			result.M43 = position.Z;
			result.M44 = 1;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <returns>The translation <see cref="Matrix2D"/>.</returns>
		public static Matrix2D CreateTranslation( Vector3 position )
		{
			Matrix2D result;
			CreateTranslation( ref position, out result );
			return result;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <param name="zPosition">Z coordinate of translation.</param>
		/// <param name="result">The translation <see cref="Matrix2D"/> as an output parameter.</param>
		public static void CreateTranslation( float xPosition, float yPosition, float zPosition, out Matrix2D result )
		{
			result.M11 = 1;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = 1;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = 1;
			result.M34 = 0;
			result.M41 = xPosition;
			result.M42 = yPosition;
			result.M43 = zPosition;
			result.M44 = 1;
		}


		/// <summary>
		/// Returns a determinant of this <see cref="Matrix2D"/>.
		/// </summary>
		/// <returns>Determinant of this <see cref="Matrix2D"/></returns>
		/// <remarks>See more about determinant here - http://en.wikipedia.org/wiki/Determinant.
		/// </remarks>
		public float Determinant()
		{
			float num22 = this.M11;
			float num21 = this.M12;
			float num20 = this.M13;
			float num19 = this.M14;
			float num12 = this.M21;
			float num11 = this.M22;
			float num10 = this.M23;
			float num9 = this.M24;
			float num8 = this.M31;
			float num7 = this.M32;
			float num6 = this.M33;
			float num5 = this.M34;
			float num4 = this.M41;
			float num3 = this.M42;
			float num2 = this.M43;
			float num = this.M44;
			float num18 = ( num6 * num ) - ( num5 * num2 );
			float num17 = ( num7 * num ) - ( num5 * num3 );
			float num16 = ( num7 * num2 ) - ( num6 * num3 );
			float num15 = ( num8 * num ) - ( num5 * num4 );
			float num14 = ( num8 * num2 ) - ( num6 * num4 );
			float num13 = ( num8 * num3 ) - ( num7 * num4 );
			return ( ( ( ( num22 * ( ( ( num11 * num18 ) - ( num10 * num17 ) ) + ( num9 * num16 ) ) ) - ( num21 * ( ( ( num12 * num18 ) - ( num10 * num15 ) ) + ( num9 * num14 ) ) ) ) + ( num20 * ( ( ( num12 * num17 ) - ( num11 * num15 ) ) + ( num9 * num13 ) ) ) ) - ( num19 * ( ( ( num12 * num16 ) - ( num11 * num14 ) ) + ( num10 * num13 ) ) ) );
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by the elements of another matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix2D"/>.</param>
		/// <returns>The result of dividing the matrix.</returns>
		public static Matrix2D Divide( Matrix2D matrix1, Matrix2D matrix2 )
		{
			matrix1.M11 = matrix1.M11 / matrix2.M11;
			matrix1.M12 = matrix1.M12 / matrix2.M12;
			matrix1.M13 = matrix1.M13 / matrix2.M13;
			matrix1.M14 = matrix1.M14 / matrix2.M14;
			matrix1.M21 = matrix1.M21 / matrix2.M21;
			matrix1.M22 = matrix1.M22 / matrix2.M22;
			matrix1.M23 = matrix1.M23 / matrix2.M23;
			matrix1.M24 = matrix1.M24 / matrix2.M24;
			matrix1.M31 = matrix1.M31 / matrix2.M31;
			matrix1.M32 = matrix1.M32 / matrix2.M32;
			matrix1.M33 = matrix1.M33 / matrix2.M33;
			matrix1.M34 = matrix1.M34 / matrix2.M34;
			matrix1.M41 = matrix1.M41 / matrix2.M41;
			matrix1.M42 = matrix1.M42 / matrix2.M42;
			matrix1.M43 = matrix1.M43 / matrix2.M43;
			matrix1.M44 = matrix1.M44 / matrix2.M44;
			return matrix1;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by the elements of another matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix2D"/>.</param>
		/// <param name="result">The result of dividing the matrix as an output parameter.</param>
		public static void Divide( ref Matrix2D matrix1, ref Matrix2D matrix2, out Matrix2D result )
		{
			result.M11 = matrix1.M11 / matrix2.M11;
			result.M12 = matrix1.M12 / matrix2.M12;
			result.M13 = matrix1.M13 / matrix2.M13;
			result.M14 = matrix1.M14 / matrix2.M14;
			result.M21 = matrix1.M21 / matrix2.M21;
			result.M22 = matrix1.M22 / matrix2.M22;
			result.M23 = matrix1.M23 / matrix2.M23;
			result.M24 = matrix1.M24 / matrix2.M24;
			result.M31 = matrix1.M31 / matrix2.M31;
			result.M32 = matrix1.M32 / matrix2.M32;
			result.M33 = matrix1.M33 / matrix2.M33;
			result.M34 = matrix1.M34 / matrix2.M34;
			result.M41 = matrix1.M41 / matrix2.M41;
			result.M42 = matrix1.M42 / matrix2.M42;
			result.M43 = matrix1.M43 / matrix2.M43;
			result.M44 = matrix1.M44 / matrix2.M44;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <returns>The result of dividing a matrix by a scalar.</returns>
		public static Matrix2D Divide( Matrix2D matrix1, float divider )
		{
			float num = 1f / divider;
			matrix1.M11 = matrix1.M11 * num;
			matrix1.M12 = matrix1.M12 * num;
			matrix1.M13 = matrix1.M13 * num;
			matrix1.M14 = matrix1.M14 * num;
			matrix1.M21 = matrix1.M21 * num;
			matrix1.M22 = matrix1.M22 * num;
			matrix1.M23 = matrix1.M23 * num;
			matrix1.M24 = matrix1.M24 * num;
			matrix1.M31 = matrix1.M31 * num;
			matrix1.M32 = matrix1.M32 * num;
			matrix1.M33 = matrix1.M33 * num;
			matrix1.M34 = matrix1.M34 * num;
			matrix1.M41 = matrix1.M41 * num;
			matrix1.M42 = matrix1.M42 * num;
			matrix1.M43 = matrix1.M43 * num;
			matrix1.M44 = matrix1.M44 * num;
			return matrix1;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <param name="result">The result of dividing a matrix by a scalar as an output parameter.</param>
		public static void Divide( ref Matrix2D matrix1, float divider, out Matrix2D result )
		{
			float num = 1f / divider;
			result.M11 = matrix1.M11 * num;
			result.M12 = matrix1.M12 * num;
			result.M13 = matrix1.M13 * num;
			result.M14 = matrix1.M14 * num;
			result.M21 = matrix1.M21 * num;
			result.M22 = matrix1.M22 * num;
			result.M23 = matrix1.M23 * num;
			result.M24 = matrix1.M24 * num;
			result.M31 = matrix1.M31 * num;
			result.M32 = matrix1.M32 * num;
			result.M33 = matrix1.M33 * num;
			result.M34 = matrix1.M34 * num;
			result.M41 = matrix1.M41 * num;
			result.M42 = matrix1.M42 * num;
			result.M43 = matrix1.M43 * num;
			result.M44 = matrix1.M44 * num;
		}


		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Matrix2D"/> without any tolerance.
		/// </summary>
		/// <param name="other">The <see cref="Matrix2D"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals( Matrix2D other )
		{
			return ( ( ( ( ( ( this.M11 == other.M11 ) && ( this.M22 == other.M22 ) ) && ( ( this.M33 == other.M33 ) && ( this.M44 == other.M44 ) ) ) && ( ( ( this.M12 == other.M12 ) && ( this.M13 == other.M13 ) ) && ( ( this.M14 == other.M14 ) && ( this.M21 == other.M21 ) ) ) ) && ( ( ( ( this.M23 == other.M23 ) && ( this.M24 == other.M24 ) ) && ( ( this.M31 == other.M31 ) && ( this.M32 == other.M32 ) ) ) && ( ( ( this.M34 == other.M34 ) && ( this.M41 == other.M41 ) ) && ( this.M42 == other.M42 ) ) ) ) && ( this.M43 == other.M43 ) );
		}


		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/> without any tolerance.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals( object obj )
		{
			bool flag = false;
			if( obj is Matrix2D )
			{
				flag = this.Equals( (Matrix2D)obj );
			}
			return flag;
		}


		/// <summary>
		/// Gets the hash code of this <see cref="Matrix2D"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Matrix2D"/>.</returns>
		public override int GetHashCode()
		{
			return ( ( ( ( ( ( ( ( ( ( ( ( ( ( ( this.M11.GetHashCode() + this.M12.GetHashCode() ) + this.M13.GetHashCode() ) + this.M14.GetHashCode() ) + this.M21.GetHashCode() ) + this.M22.GetHashCode() ) + this.M23.GetHashCode() ) + this.M24.GetHashCode() ) + this.M31.GetHashCode() ) + this.M32.GetHashCode() ) + this.M33.GetHashCode() ) + this.M34.GetHashCode() ) + this.M41.GetHashCode() ) + this.M42.GetHashCode() ) + this.M43.GetHashCode() ) + this.M44.GetHashCode() );
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> which contains inversion of the specified matrix. 
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/>.</param>
		/// <returns>The inverted matrix.</returns>
		public static Matrix2D Invert( Matrix2D matrix )
		{
			Matrix2D result;
			Invert( ref matrix, out result );
			return result;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> which contains inversion of the specified matrix. 
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/>.</param>
		/// <param name="result">The inverted matrix as output parameter.</param>
		public static void Invert( ref Matrix2D matrix, out Matrix2D result )
		{
			float num1 = matrix.M11;
			float num2 = matrix.M12;
			float num3 = matrix.M13;
			float num4 = matrix.M14;
			float num5 = matrix.M21;
			float num6 = matrix.M22;
			float num7 = matrix.M23;
			float num8 = matrix.M24;
			float num9 = matrix.M31;
			float num10 = matrix.M32;
			float num11 = matrix.M33;
			float num12 = matrix.M34;
			float num13 = matrix.M41;
			float num14 = matrix.M42;
			float num15 = matrix.M43;
			float num16 = matrix.M44;
			float num17 = (float)( (double)num11 * (double)num16 - (double)num12 * (double)num15 );
			float num18 = (float)( (double)num10 * (double)num16 - (double)num12 * (double)num14 );
			float num19 = (float)( (double)num10 * (double)num15 - (double)num11 * (double)num14 );
			float num20 = (float)( (double)num9 * (double)num16 - (double)num12 * (double)num13 );
			float num21 = (float)( (double)num9 * (double)num15 - (double)num11 * (double)num13 );
			float num22 = (float)( (double)num9 * (double)num14 - (double)num10 * (double)num13 );
			float num23 = (float)( (double)num6 * (double)num17 - (double)num7 * (double)num18 + (double)num8 * (double)num19 );
			float num24 = (float)-( (double)num5 * (double)num17 - (double)num7 * (double)num20 + (double)num8 * (double)num21 );
			float num25 = (float)( (double)num5 * (double)num18 - (double)num6 * (double)num20 + (double)num8 * (double)num22 );
			float num26 = (float)-( (double)num5 * (double)num19 - (double)num6 * (double)num21 + (double)num7 * (double)num22 );
			float num27 = (float)( 1.0 / ( (double)num1 * (double)num23 + (double)num2 * (double)num24 + (double)num3 * (double)num25 + (double)num4 * (double)num26 ) );

			result.M11 = num23 * num27;
			result.M21 = num24 * num27;
			result.M31 = num25 * num27;
			result.M41 = num26 * num27;
			result.M12 = (float)-( (double)num2 * (double)num17 - (double)num3 * (double)num18 + (double)num4 * (double)num19 ) * num27;
			result.M22 = (float)( (double)num1 * (double)num17 - (double)num3 * (double)num20 + (double)num4 * (double)num21 ) * num27;
			result.M32 = (float)-( (double)num1 * (double)num18 - (double)num2 * (double)num20 + (double)num4 * (double)num22 ) * num27;
			result.M42 = (float)( (double)num1 * (double)num19 - (double)num2 * (double)num21 + (double)num3 * (double)num22 ) * num27;
			float num28 = (float)( (double)num7 * (double)num16 - (double)num8 * (double)num15 );
			float num29 = (float)( (double)num6 * (double)num16 - (double)num8 * (double)num14 );
			float num30 = (float)( (double)num6 * (double)num15 - (double)num7 * (double)num14 );
			float num31 = (float)( (double)num5 * (double)num16 - (double)num8 * (double)num13 );
			float num32 = (float)( (double)num5 * (double)num15 - (double)num7 * (double)num13 );
			float num33 = (float)( (double)num5 * (double)num14 - (double)num6 * (double)num13 );
			result.M13 = (float)( (double)num2 * (double)num28 - (double)num3 * (double)num29 + (double)num4 * (double)num30 ) * num27;
			result.M23 = (float)-( (double)num1 * (double)num28 - (double)num3 * (double)num31 + (double)num4 * (double)num32 ) * num27;
			result.M33 = (float)( (double)num1 * (double)num29 - (double)num2 * (double)num31 + (double)num4 * (double)num33 ) * num27;
			result.M43 = (float)-( (double)num1 * (double)num30 - (double)num2 * (double)num32 + (double)num3 * (double)num33 ) * num27;
			float num34 = (float)( (double)num7 * (double)num12 - (double)num8 * (double)num11 );
			float num35 = (float)( (double)num6 * (double)num12 - (double)num8 * (double)num10 );
			float num36 = (float)( (double)num6 * (double)num11 - (double)num7 * (double)num10 );
			float num37 = (float)( (double)num5 * (double)num12 - (double)num8 * (double)num9 );
			float num38 = (float)( (double)num5 * (double)num11 - (double)num7 * (double)num9 );
			float num39 = (float)( (double)num5 * (double)num10 - (double)num6 * (double)num9 );
			result.M14 = (float)-( (double)num2 * (double)num34 - (double)num3 * (double)num35 + (double)num4 * (double)num36 ) * num27;
			result.M24 = (float)( (double)num1 * (double)num34 - (double)num3 * (double)num37 + (double)num4 * (double)num38 ) * num27;
			result.M34 = (float)-( (double)num1 * (double)num35 - (double)num2 * (double)num37 + (double)num4 * (double)num39 ) * num27;
			result.M44 = (float)( (double)num1 * (double)num36 - (double)num2 * (double)num38 + (double)num3 * (double)num39 ) * num27;


			/*
			
			
            ///
            // Use Laplace expansion theorem to calculate the inverse of a 4x4 matrix
            // 
            // 1. Calculate the 2x2 determinants needed the 4x4 determinant based on the 2x2 determinants 
            // 3. Create the adjugate matrix, which satisfies: A * adj(A) = det(A) * I
            // 4. Divide adjugate matrix with the determinant to find the inverse
            
            float det1, det2, det3, det4, det5, det6, det7, det8, det9, det10, det11, det12;
            float detMatrix;
            FindDeterminants(ref matrix, out detMatrix, out det1, out det2, out det3, out det4, out det5, out det6, 
                             out det7, out det8, out det9, out det10, out det11, out det12);
            
            float invDetMatrix = 1f / detMatrix;
            
            Matrix ret; // Allow for matrix and result to point to the same structure
            
            ret.M11 = (matrix.M22*det12 - matrix.M23*det11 + matrix.M24*det10) * invDetMatrix;
            ret.M12 = (-matrix.M12*det12 + matrix.M13*det11 - matrix.M14*det10) * invDetMatrix;
            ret.M13 = (matrix.M42*det6 - matrix.M43*det5 + matrix.M44*det4) * invDetMatrix;
            ret.M14 = (-matrix.M32*det6 + matrix.M33*det5 - matrix.M34*det4) * invDetMatrix;
            ret.M21 = (-matrix.M21*det12 + matrix.M23*det9 - matrix.M24*det8) * invDetMatrix;
            ret.M22 = (matrix.M11*det12 - matrix.M13*det9 + matrix.M14*det8) * invDetMatrix;
            ret.M23 = (-matrix.M41*det6 + matrix.M43*det3 - matrix.M44*det2) * invDetMatrix;
            ret.M24 = (matrix.M31*det6 - matrix.M33*det3 + matrix.M34*det2) * invDetMatrix;
            ret.M31 = (matrix.M21*det11 - matrix.M22*det9 + matrix.M24*det7) * invDetMatrix;
            ret.M32 = (-matrix.M11*det11 + matrix.M12*det9 - matrix.M14*det7) * invDetMatrix;
            ret.M33 = (matrix.M41*det5 - matrix.M42*det3 + matrix.M44*det1) * invDetMatrix;
            ret.M34 = (-matrix.M31*det5 + matrix.M32*det3 - matrix.M34*det1) * invDetMatrix;
            ret.M41 = (-matrix.M21*det10 + matrix.M22*det8 - matrix.M23*det7) * invDetMatrix;
            ret.M42 = (matrix.M11*det10 - matrix.M12*det8 + matrix.M13*det7) * invDetMatrix;
            ret.M43 = (-matrix.M41*det4 + matrix.M42*det2 - matrix.M43*det1) * invDetMatrix;
            ret.M44 = (matrix.M31*det4 - matrix.M32*det2 + matrix.M33*det1) * invDetMatrix;
            
            result = ret;
            */
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains linear interpolation of the values in specified matrixes.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">The second <see cref="Vector2"/>.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <returns>>The result of linear interpolation of the specified matrixes.</returns>
		public static Matrix2D Lerp( Matrix2D matrix1, Matrix2D matrix2, float amount )
		{
			matrix1.M11 = matrix1.M11 + ( ( matrix2.M11 - matrix1.M11 ) * amount );
			matrix1.M12 = matrix1.M12 + ( ( matrix2.M12 - matrix1.M12 ) * amount );
			matrix1.M13 = matrix1.M13 + ( ( matrix2.M13 - matrix1.M13 ) * amount );
			matrix1.M14 = matrix1.M14 + ( ( matrix2.M14 - matrix1.M14 ) * amount );
			matrix1.M21 = matrix1.M21 + ( ( matrix2.M21 - matrix1.M21 ) * amount );
			matrix1.M22 = matrix1.M22 + ( ( matrix2.M22 - matrix1.M22 ) * amount );
			matrix1.M23 = matrix1.M23 + ( ( matrix2.M23 - matrix1.M23 ) * amount );
			matrix1.M24 = matrix1.M24 + ( ( matrix2.M24 - matrix1.M24 ) * amount );
			matrix1.M31 = matrix1.M31 + ( ( matrix2.M31 - matrix1.M31 ) * amount );
			matrix1.M32 = matrix1.M32 + ( ( matrix2.M32 - matrix1.M32 ) * amount );
			matrix1.M33 = matrix1.M33 + ( ( matrix2.M33 - matrix1.M33 ) * amount );
			matrix1.M34 = matrix1.M34 + ( ( matrix2.M34 - matrix1.M34 ) * amount );
			matrix1.M41 = matrix1.M41 + ( ( matrix2.M41 - matrix1.M41 ) * amount );
			matrix1.M42 = matrix1.M42 + ( ( matrix2.M42 - matrix1.M42 ) * amount );
			matrix1.M43 = matrix1.M43 + ( ( matrix2.M43 - matrix1.M43 ) * amount );
			matrix1.M44 = matrix1.M44 + ( ( matrix2.M44 - matrix1.M44 ) * amount );
			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains linear interpolation of the values in specified matrixes.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">The second <see cref="Vector2"/>.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="result">The result of linear interpolation of the specified matrixes as an output parameter.</param>
		public static void Lerp( ref Matrix2D matrix1, ref Matrix2D matrix2, float amount, out Matrix2D result )
		{
			result.M11 = matrix1.M11 + ( ( matrix2.M11 - matrix1.M11 ) * amount );
			result.M12 = matrix1.M12 + ( ( matrix2.M12 - matrix1.M12 ) * amount );
			result.M13 = matrix1.M13 + ( ( matrix2.M13 - matrix1.M13 ) * amount );
			result.M14 = matrix1.M14 + ( ( matrix2.M14 - matrix1.M14 ) * amount );
			result.M21 = matrix1.M21 + ( ( matrix2.M21 - matrix1.M21 ) * amount );
			result.M22 = matrix1.M22 + ( ( matrix2.M22 - matrix1.M22 ) * amount );
			result.M23 = matrix1.M23 + ( ( matrix2.M23 - matrix1.M23 ) * amount );
			result.M24 = matrix1.M24 + ( ( matrix2.M24 - matrix1.M24 ) * amount );
			result.M31 = matrix1.M31 + ( ( matrix2.M31 - matrix1.M31 ) * amount );
			result.M32 = matrix1.M32 + ( ( matrix2.M32 - matrix1.M32 ) * amount );
			result.M33 = matrix1.M33 + ( ( matrix2.M33 - matrix1.M33 ) * amount );
			result.M34 = matrix1.M34 + ( ( matrix2.M34 - matrix1.M34 ) * amount );
			result.M41 = matrix1.M41 + ( ( matrix2.M41 - matrix1.M41 ) * amount );
			result.M42 = matrix1.M42 + ( ( matrix2.M42 - matrix1.M42 ) * amount );
			result.M43 = matrix1.M43 + ( ( matrix2.M43 - matrix1.M43 ) * amount );
			result.M44 = matrix1.M44 + ( ( matrix2.M44 - matrix1.M44 ) * amount );
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/>.</param>
		/// <returns>Result of the matrix multiplication.</returns>
		public static Matrix2D Multiply( Matrix2D matrix1, Matrix2D matrix2 )
		{
			var m11 = ( ( ( matrix1.M11 * matrix2.M11 ) + ( matrix1.M12 * matrix2.M21 ) ) + ( matrix1.M13 * matrix2.M31 ) ) + ( matrix1.M14 * matrix2.M41 );
			var m12 = ( ( ( matrix1.M11 * matrix2.M12 ) + ( matrix1.M12 * matrix2.M22 ) ) + ( matrix1.M13 * matrix2.M32 ) ) + ( matrix1.M14 * matrix2.M42 );
			var m13 = ( ( ( matrix1.M11 * matrix2.M13 ) + ( matrix1.M12 * matrix2.M23 ) ) + ( matrix1.M13 * matrix2.M33 ) ) + ( matrix1.M14 * matrix2.M43 );
			var m14 = ( ( ( matrix1.M11 * matrix2.M14 ) + ( matrix1.M12 * matrix2.M24 ) ) + ( matrix1.M13 * matrix2.M34 ) ) + ( matrix1.M14 * matrix2.M44 );
			var m21 = ( ( ( matrix1.M21 * matrix2.M11 ) + ( matrix1.M22 * matrix2.M21 ) ) + ( matrix1.M23 * matrix2.M31 ) ) + ( matrix1.M24 * matrix2.M41 );
			var m22 = ( ( ( matrix1.M21 * matrix2.M12 ) + ( matrix1.M22 * matrix2.M22 ) ) + ( matrix1.M23 * matrix2.M32 ) ) + ( matrix1.M24 * matrix2.M42 );
			var m23 = ( ( ( matrix1.M21 * matrix2.M13 ) + ( matrix1.M22 * matrix2.M23 ) ) + ( matrix1.M23 * matrix2.M33 ) ) + ( matrix1.M24 * matrix2.M43 );
			var m24 = ( ( ( matrix1.M21 * matrix2.M14 ) + ( matrix1.M22 * matrix2.M24 ) ) + ( matrix1.M23 * matrix2.M34 ) ) + ( matrix1.M24 * matrix2.M44 );
			var m31 = ( ( ( matrix1.M31 * matrix2.M11 ) + ( matrix1.M32 * matrix2.M21 ) ) + ( matrix1.M33 * matrix2.M31 ) ) + ( matrix1.M34 * matrix2.M41 );
			var m32 = ( ( ( matrix1.M31 * matrix2.M12 ) + ( matrix1.M32 * matrix2.M22 ) ) + ( matrix1.M33 * matrix2.M32 ) ) + ( matrix1.M34 * matrix2.M42 );
			var m33 = ( ( ( matrix1.M31 * matrix2.M13 ) + ( matrix1.M32 * matrix2.M23 ) ) + ( matrix1.M33 * matrix2.M33 ) ) + ( matrix1.M34 * matrix2.M43 );
			var m34 = ( ( ( matrix1.M31 * matrix2.M14 ) + ( matrix1.M32 * matrix2.M24 ) ) + ( matrix1.M33 * matrix2.M34 ) ) + ( matrix1.M34 * matrix2.M44 );
			var m41 = ( ( ( matrix1.M41 * matrix2.M11 ) + ( matrix1.M42 * matrix2.M21 ) ) + ( matrix1.M43 * matrix2.M31 ) ) + ( matrix1.M44 * matrix2.M41 );
			var m42 = ( ( ( matrix1.M41 * matrix2.M12 ) + ( matrix1.M42 * matrix2.M22 ) ) + ( matrix1.M43 * matrix2.M32 ) ) + ( matrix1.M44 * matrix2.M42 );
			var m43 = ( ( ( matrix1.M41 * matrix2.M13 ) + ( matrix1.M42 * matrix2.M23 ) ) + ( matrix1.M43 * matrix2.M33 ) ) + ( matrix1.M44 * matrix2.M43 );
			var m44 = ( ( ( matrix1.M41 * matrix2.M14 ) + ( matrix1.M42 * matrix2.M24 ) ) + ( matrix1.M43 * matrix2.M34 ) ) + ( matrix1.M44 * matrix2.M44 );
			matrix1.M11 = m11;
			matrix1.M12 = m12;
			matrix1.M13 = m13;
			matrix1.M14 = m14;
			matrix1.M21 = m21;
			matrix1.M22 = m22;
			matrix1.M23 = m23;
			matrix1.M24 = m24;
			matrix1.M31 = m31;
			matrix1.M32 = m32;
			matrix1.M33 = m33;
			matrix1.M34 = m34;
			matrix1.M41 = m41;
			matrix1.M42 = m42;
			matrix1.M43 = m43;
			matrix1.M44 = m44;
			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/>.</param>
		/// <param name="result">Result of the matrix multiplication as an output parameter.</param>
		public static void Multiply( ref Matrix2D matrix1, ref Matrix2D matrix2, out Matrix2D result )
		{
			var m11 = ( ( ( matrix1.M11 * matrix2.M11 ) + ( matrix1.M12 * matrix2.M21 ) ) + ( matrix1.M13 * matrix2.M31 ) ) + ( matrix1.M14 * matrix2.M41 );
			var m12 = ( ( ( matrix1.M11 * matrix2.M12 ) + ( matrix1.M12 * matrix2.M22 ) ) + ( matrix1.M13 * matrix2.M32 ) ) + ( matrix1.M14 * matrix2.M42 );
			var m13 = ( ( ( matrix1.M11 * matrix2.M13 ) + ( matrix1.M12 * matrix2.M23 ) ) + ( matrix1.M13 * matrix2.M33 ) ) + ( matrix1.M14 * matrix2.M43 );
			var m14 = ( ( ( matrix1.M11 * matrix2.M14 ) + ( matrix1.M12 * matrix2.M24 ) ) + ( matrix1.M13 * matrix2.M34 ) ) + ( matrix1.M14 * matrix2.M44 );
			var m21 = ( ( ( matrix1.M21 * matrix2.M11 ) + ( matrix1.M22 * matrix2.M21 ) ) + ( matrix1.M23 * matrix2.M31 ) ) + ( matrix1.M24 * matrix2.M41 );
			var m22 = ( ( ( matrix1.M21 * matrix2.M12 ) + ( matrix1.M22 * matrix2.M22 ) ) + ( matrix1.M23 * matrix2.M32 ) ) + ( matrix1.M24 * matrix2.M42 );
			var m23 = ( ( ( matrix1.M21 * matrix2.M13 ) + ( matrix1.M22 * matrix2.M23 ) ) + ( matrix1.M23 * matrix2.M33 ) ) + ( matrix1.M24 * matrix2.M43 );
			var m24 = ( ( ( matrix1.M21 * matrix2.M14 ) + ( matrix1.M22 * matrix2.M24 ) ) + ( matrix1.M23 * matrix2.M34 ) ) + ( matrix1.M24 * matrix2.M44 );
			var m31 = ( ( ( matrix1.M31 * matrix2.M11 ) + ( matrix1.M32 * matrix2.M21 ) ) + ( matrix1.M33 * matrix2.M31 ) ) + ( matrix1.M34 * matrix2.M41 );
			var m32 = ( ( ( matrix1.M31 * matrix2.M12 ) + ( matrix1.M32 * matrix2.M22 ) ) + ( matrix1.M33 * matrix2.M32 ) ) + ( matrix1.M34 * matrix2.M42 );
			var m33 = ( ( ( matrix1.M31 * matrix2.M13 ) + ( matrix1.M32 * matrix2.M23 ) ) + ( matrix1.M33 * matrix2.M33 ) ) + ( matrix1.M34 * matrix2.M43 );
			var m34 = ( ( ( matrix1.M31 * matrix2.M14 ) + ( matrix1.M32 * matrix2.M24 ) ) + ( matrix1.M33 * matrix2.M34 ) ) + ( matrix1.M34 * matrix2.M44 );
			var m41 = ( ( ( matrix1.M41 * matrix2.M11 ) + ( matrix1.M42 * matrix2.M21 ) ) + ( matrix1.M43 * matrix2.M31 ) ) + ( matrix1.M44 * matrix2.M41 );
			var m42 = ( ( ( matrix1.M41 * matrix2.M12 ) + ( matrix1.M42 * matrix2.M22 ) ) + ( matrix1.M43 * matrix2.M32 ) ) + ( matrix1.M44 * matrix2.M42 );
			var m43 = ( ( ( matrix1.M41 * matrix2.M13 ) + ( matrix1.M42 * matrix2.M23 ) ) + ( matrix1.M43 * matrix2.M33 ) ) + ( matrix1.M44 * matrix2.M43 );
			var m44 = ( ( ( matrix1.M41 * matrix2.M14 ) + ( matrix1.M42 * matrix2.M24 ) ) + ( matrix1.M43 * matrix2.M34 ) ) + ( matrix1.M44 * matrix2.M44 );
			result.M11 = m11;
			result.M12 = m12;
			result.M13 = m13;
			result.M14 = m14;
			result.M21 = m21;
			result.M22 = m22;
			result.M23 = m23;
			result.M24 = m24;
			result.M31 = m31;
			result.M32 = m32;
			result.M33 = m33;
			result.M34 = m34;
			result.M41 = m41;
			result.M42 = m42;
			result.M43 = m43;
			result.M44 = m44;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of <see cref="Matrix2D"/> and a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <returns>Result of the matrix multiplication with a scalar.</returns>
		public static Matrix2D Multiply( Matrix2D matrix1, float scaleFactor )
		{
			matrix1.M11 *= scaleFactor;
			matrix1.M12 *= scaleFactor;
			matrix1.M13 *= scaleFactor;
			matrix1.M14 *= scaleFactor;
			matrix1.M21 *= scaleFactor;
			matrix1.M22 *= scaleFactor;
			matrix1.M23 *= scaleFactor;
			matrix1.M24 *= scaleFactor;
			matrix1.M31 *= scaleFactor;
			matrix1.M32 *= scaleFactor;
			matrix1.M33 *= scaleFactor;
			matrix1.M34 *= scaleFactor;
			matrix1.M41 *= scaleFactor;
			matrix1.M42 *= scaleFactor;
			matrix1.M43 *= scaleFactor;
			matrix1.M44 *= scaleFactor;
			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of <see cref="Matrix2D"/> and a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <param name="result">Result of the matrix multiplication with a scalar as an output parameter.</param>
		public static void Multiply( ref Matrix2D matrix1, float scaleFactor, out Matrix2D result )
		{
			result.M11 = matrix1.M11 * scaleFactor;
			result.M12 = matrix1.M12 * scaleFactor;
			result.M13 = matrix1.M13 * scaleFactor;
			result.M14 = matrix1.M14 * scaleFactor;
			result.M21 = matrix1.M21 * scaleFactor;
			result.M22 = matrix1.M22 * scaleFactor;
			result.M23 = matrix1.M23 * scaleFactor;
			result.M24 = matrix1.M24 * scaleFactor;
			result.M31 = matrix1.M31 * scaleFactor;
			result.M32 = matrix1.M32 * scaleFactor;
			result.M33 = matrix1.M33 * scaleFactor;
			result.M34 = matrix1.M34 * scaleFactor;
			result.M41 = matrix1.M41 * scaleFactor;
			result.M42 = matrix1.M42 * scaleFactor;
			result.M43 = matrix1.M43 * scaleFactor;
			result.M44 = matrix1.M44 * scaleFactor;
		}


		/// <summary>
		/// Copy the values of specified <see cref="Matrix2D"/> to the float array.
		/// </summary>
		/// <param name="matrix">The source <see cref="Matrix2D"/>.</param>
		/// <returns>The array which matrix values will be stored.</returns>
		/// <remarks>
		/// Required for OpenGL 2.0 projection matrix stuff.
		/// </remarks>
		public static float[] ToFloatArray( Matrix2D matrix )
		{
			float[] matarray = {
									matrix.M11, matrix.M12, matrix.M13, matrix.M14,
									matrix.M21, matrix.M22, matrix.M23, matrix.M24,
									matrix.M31, matrix.M32, matrix.M33, matrix.M34,
									matrix.M41, matrix.M42, matrix.M43, matrix.M44
								};
			return matarray;
		}


		/// <summary>
		/// Returns a matrix with the all values negated.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/>.</param>
		/// <returns>Result of the matrix negation.</returns>
		public static Matrix2D Negate( Matrix2D matrix )
		{
			matrix.M11 = -matrix.M11;
			matrix.M12 = -matrix.M12;
			matrix.M13 = -matrix.M13;
			matrix.M14 = -matrix.M14;
			matrix.M21 = -matrix.M21;
			matrix.M22 = -matrix.M22;
			matrix.M23 = -matrix.M23;
			matrix.M24 = -matrix.M24;
			matrix.M31 = -matrix.M31;
			matrix.M32 = -matrix.M32;
			matrix.M33 = -matrix.M33;
			matrix.M34 = -matrix.M34;
			matrix.M41 = -matrix.M41;
			matrix.M42 = -matrix.M42;
			matrix.M43 = -matrix.M43;
			matrix.M44 = -matrix.M44;
			return matrix;
		}


		/// <summary>
		/// Returns a matrix with the all values negated.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/>.</param>
		/// <param name="result">Result of the matrix negation as an output parameter.</param>
		public static void Negate( ref Matrix2D matrix, out Matrix2D result )
		{
			result.M11 = -matrix.M11;
			result.M12 = -matrix.M12;
			result.M13 = -matrix.M13;
			result.M14 = -matrix.M14;
			result.M21 = -matrix.M21;
			result.M22 = -matrix.M22;
			result.M23 = -matrix.M23;
			result.M24 = -matrix.M24;
			result.M31 = -matrix.M31;
			result.M32 = -matrix.M32;
			result.M33 = -matrix.M33;
			result.M34 = -matrix.M34;
			result.M41 = -matrix.M41;
			result.M42 = -matrix.M42;
			result.M43 = -matrix.M43;
			result.M44 = -matrix.M44;
		}


		/// <summary>
		/// Adds two matrixes.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the add sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/> on the right of the add sign.</param>
		/// <returns>Sum of the matrixes.</returns>
		public static Matrix2D operator +( Matrix2D matrix1, Matrix2D matrix2 )
		{
			matrix1.M11 = matrix1.M11 + matrix2.M11;
			matrix1.M12 = matrix1.M12 + matrix2.M12;
			matrix1.M13 = matrix1.M13 + matrix2.M13;
			matrix1.M14 = matrix1.M14 + matrix2.M14;
			matrix1.M21 = matrix1.M21 + matrix2.M21;
			matrix1.M22 = matrix1.M22 + matrix2.M22;
			matrix1.M23 = matrix1.M23 + matrix2.M23;
			matrix1.M24 = matrix1.M24 + matrix2.M24;
			matrix1.M31 = matrix1.M31 + matrix2.M31;
			matrix1.M32 = matrix1.M32 + matrix2.M32;
			matrix1.M33 = matrix1.M33 + matrix2.M33;
			matrix1.M34 = matrix1.M34 + matrix2.M34;
			matrix1.M41 = matrix1.M41 + matrix2.M41;
			matrix1.M42 = matrix1.M42 + matrix2.M42;
			matrix1.M43 = matrix1.M43 + matrix2.M43;
			matrix1.M44 = matrix1.M44 + matrix2.M44;
			return matrix1;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by the elements of another <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the div sign.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix2D"/> on the right of the div sign.</param>
		/// <returns>The result of dividing the matrixes.</returns>
		public static Matrix2D operator /( Matrix2D matrix1, Matrix2D matrix2 )
		{
			matrix1.M11 = matrix1.M11 / matrix2.M11;
			matrix1.M12 = matrix1.M12 / matrix2.M12;
			matrix1.M13 = matrix1.M13 / matrix2.M13;
			matrix1.M14 = matrix1.M14 / matrix2.M14;
			matrix1.M21 = matrix1.M21 / matrix2.M21;
			matrix1.M22 = matrix1.M22 / matrix2.M22;
			matrix1.M23 = matrix1.M23 / matrix2.M23;
			matrix1.M24 = matrix1.M24 / matrix2.M24;
			matrix1.M31 = matrix1.M31 / matrix2.M31;
			matrix1.M32 = matrix1.M32 / matrix2.M32;
			matrix1.M33 = matrix1.M33 / matrix2.M33;
			matrix1.M34 = matrix1.M34 / matrix2.M34;
			matrix1.M41 = matrix1.M41 / matrix2.M41;
			matrix1.M42 = matrix1.M42 / matrix2.M42;
			matrix1.M43 = matrix1.M43 / matrix2.M43;
			matrix1.M44 = matrix1.M44 / matrix2.M44;
			return matrix1;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by a scalar.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/> on the left of the div sign.</param>
		/// <param name="divider">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing a matrix by a scalar.</returns>
		public static Matrix2D operator /( Matrix2D matrix, float divider )
		{
			float num = 1f / divider;
			matrix.M11 = matrix.M11 * num;
			matrix.M12 = matrix.M12 * num;
			matrix.M13 = matrix.M13 * num;
			matrix.M14 = matrix.M14 * num;
			matrix.M21 = matrix.M21 * num;
			matrix.M22 = matrix.M22 * num;
			matrix.M23 = matrix.M23 * num;
			matrix.M24 = matrix.M24 * num;
			matrix.M31 = matrix.M31 * num;
			matrix.M32 = matrix.M32 * num;
			matrix.M33 = matrix.M33 * num;
			matrix.M34 = matrix.M34 * num;
			matrix.M41 = matrix.M41 * num;
			matrix.M42 = matrix.M42 * num;
			matrix.M43 = matrix.M43 * num;
			matrix.M44 = matrix.M44 * num;
			return matrix;
		}


		/// <summary>
		/// Compares whether two <see cref="Matrix2D"/> instances are equal without any tolerance.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the equal sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/> on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==( Matrix2D matrix1, Matrix2D matrix2 )
		{
			return (
				matrix1.M11 == matrix2.M11 &&
				matrix1.M12 == matrix2.M12 &&
				matrix1.M13 == matrix2.M13 &&
				matrix1.M14 == matrix2.M14 &&
				matrix1.M21 == matrix2.M21 &&
				matrix1.M22 == matrix2.M22 &&
				matrix1.M23 == matrix2.M23 &&
				matrix1.M24 == matrix2.M24 &&
				matrix1.M31 == matrix2.M31 &&
				matrix1.M32 == matrix2.M32 &&
				matrix1.M33 == matrix2.M33 &&
				matrix1.M34 == matrix2.M34 &&
				matrix1.M41 == matrix2.M41 &&
				matrix1.M42 == matrix2.M42 &&
				matrix1.M43 == matrix2.M43 &&
				matrix1.M44 == matrix2.M44
				);
		}


		/// <summary>
		/// Compares whether two <see cref="Matrix2D"/> instances are not equal without any tolerance.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the not equal sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/> on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=( Matrix2D matrix1, Matrix2D matrix2 )
		{
			return (
				matrix1.M11 != matrix2.M11 ||
				matrix1.M12 != matrix2.M12 ||
				matrix1.M13 != matrix2.M13 ||
				matrix1.M14 != matrix2.M14 ||
				matrix1.M21 != matrix2.M21 ||
				matrix1.M22 != matrix2.M22 ||
				matrix1.M23 != matrix2.M23 ||
				matrix1.M24 != matrix2.M24 ||
				matrix1.M31 != matrix2.M31 ||
				matrix1.M32 != matrix2.M32 ||
				matrix1.M33 != matrix2.M33 ||
				matrix1.M34 != matrix2.M34 ||
				matrix1.M41 != matrix2.M41 ||
				matrix1.M42 != matrix2.M42 ||
				matrix1.M43 != matrix2.M43 ||
				matrix1.M44 != matrix2.M44
				);
		}


		/// <summary>
		/// Multiplies two matrixes.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the mul sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/> on the right of the mul sign.</param>
		/// <returns>Result of the matrix multiplication.</returns>
		/// <remarks>
		/// Using matrix multiplication algorithm - see http://en.wikipedia.org/wiki/Matrix_multiplication.
		/// </remarks>
		public static Matrix2D operator *( Matrix2D matrix1, Matrix2D matrix2 )
		{
			var m11 = ( ( ( matrix1.M11 * matrix2.M11 ) + ( matrix1.M12 * matrix2.M21 ) ) + ( matrix1.M13 * matrix2.M31 ) ) + ( matrix1.M14 * matrix2.M41 );
			var m12 = ( ( ( matrix1.M11 * matrix2.M12 ) + ( matrix1.M12 * matrix2.M22 ) ) + ( matrix1.M13 * matrix2.M32 ) ) + ( matrix1.M14 * matrix2.M42 );
			var m13 = ( ( ( matrix1.M11 * matrix2.M13 ) + ( matrix1.M12 * matrix2.M23 ) ) + ( matrix1.M13 * matrix2.M33 ) ) + ( matrix1.M14 * matrix2.M43 );
			var m14 = ( ( ( matrix1.M11 * matrix2.M14 ) + ( matrix1.M12 * matrix2.M24 ) ) + ( matrix1.M13 * matrix2.M34 ) ) + ( matrix1.M14 * matrix2.M44 );
			var m21 = ( ( ( matrix1.M21 * matrix2.M11 ) + ( matrix1.M22 * matrix2.M21 ) ) + ( matrix1.M23 * matrix2.M31 ) ) + ( matrix1.M24 * matrix2.M41 );
			var m22 = ( ( ( matrix1.M21 * matrix2.M12 ) + ( matrix1.M22 * matrix2.M22 ) ) + ( matrix1.M23 * matrix2.M32 ) ) + ( matrix1.M24 * matrix2.M42 );
			var m23 = ( ( ( matrix1.M21 * matrix2.M13 ) + ( matrix1.M22 * matrix2.M23 ) ) + ( matrix1.M23 * matrix2.M33 ) ) + ( matrix1.M24 * matrix2.M43 );
			var m24 = ( ( ( matrix1.M21 * matrix2.M14 ) + ( matrix1.M22 * matrix2.M24 ) ) + ( matrix1.M23 * matrix2.M34 ) ) + ( matrix1.M24 * matrix2.M44 );
			var m31 = ( ( ( matrix1.M31 * matrix2.M11 ) + ( matrix1.M32 * matrix2.M21 ) ) + ( matrix1.M33 * matrix2.M31 ) ) + ( matrix1.M34 * matrix2.M41 );
			var m32 = ( ( ( matrix1.M31 * matrix2.M12 ) + ( matrix1.M32 * matrix2.M22 ) ) + ( matrix1.M33 * matrix2.M32 ) ) + ( matrix1.M34 * matrix2.M42 );
			var m33 = ( ( ( matrix1.M31 * matrix2.M13 ) + ( matrix1.M32 * matrix2.M23 ) ) + ( matrix1.M33 * matrix2.M33 ) ) + ( matrix1.M34 * matrix2.M43 );
			var m34 = ( ( ( matrix1.M31 * matrix2.M14 ) + ( matrix1.M32 * matrix2.M24 ) ) + ( matrix1.M33 * matrix2.M34 ) ) + ( matrix1.M34 * matrix2.M44 );
			var m41 = ( ( ( matrix1.M41 * matrix2.M11 ) + ( matrix1.M42 * matrix2.M21 ) ) + ( matrix1.M43 * matrix2.M31 ) ) + ( matrix1.M44 * matrix2.M41 );
			var m42 = ( ( ( matrix1.M41 * matrix2.M12 ) + ( matrix1.M42 * matrix2.M22 ) ) + ( matrix1.M43 * matrix2.M32 ) ) + ( matrix1.M44 * matrix2.M42 );
			var m43 = ( ( ( matrix1.M41 * matrix2.M13 ) + ( matrix1.M42 * matrix2.M23 ) ) + ( matrix1.M43 * matrix2.M33 ) ) + ( matrix1.M44 * matrix2.M43 );
			var m44 = ( ( ( matrix1.M41 * matrix2.M14 ) + ( matrix1.M42 * matrix2.M24 ) ) + ( matrix1.M43 * matrix2.M34 ) ) + ( matrix1.M44 * matrix2.M44 );
			matrix1.M11 = m11;
			matrix1.M12 = m12;
			matrix1.M13 = m13;
			matrix1.M14 = m14;
			matrix1.M21 = m21;
			matrix1.M22 = m22;
			matrix1.M23 = m23;
			matrix1.M24 = m24;
			matrix1.M31 = m31;
			matrix1.M32 = m32;
			matrix1.M33 = m33;
			matrix1.M34 = m34;
			matrix1.M41 = m41;
			matrix1.M42 = m42;
			matrix1.M43 = m43;
			matrix1.M44 = m44;
			return matrix1;
		}


		/// <summary>
		/// Multiplies the elements of matrix by a scalar.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/> on the left of the mul sign.</param>
		/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
		/// <returns>Result of the matrix multiplication with a scalar.</returns>
		public static Matrix2D operator *( Matrix2D matrix, float scaleFactor )
		{
			matrix.M11 = matrix.M11 * scaleFactor;
			matrix.M12 = matrix.M12 * scaleFactor;
			matrix.M13 = matrix.M13 * scaleFactor;
			matrix.M14 = matrix.M14 * scaleFactor;
			matrix.M21 = matrix.M21 * scaleFactor;
			matrix.M22 = matrix.M22 * scaleFactor;
			matrix.M23 = matrix.M23 * scaleFactor;
			matrix.M24 = matrix.M24 * scaleFactor;
			matrix.M31 = matrix.M31 * scaleFactor;
			matrix.M32 = matrix.M32 * scaleFactor;
			matrix.M33 = matrix.M33 * scaleFactor;
			matrix.M34 = matrix.M34 * scaleFactor;
			matrix.M41 = matrix.M41 * scaleFactor;
			matrix.M42 = matrix.M42 * scaleFactor;
			matrix.M43 = matrix.M43 * scaleFactor;
			matrix.M44 = matrix.M44 * scaleFactor;
			return matrix;
		}


		/// <summary>
		/// Subtracts the values of one <see cref="Matrix2D"/> from another <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the sub sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/> on the right of the sub sign.</param>
		/// <returns>Result of the matrix subtraction.</returns>
		public static Matrix2D operator -( Matrix2D matrix1, Matrix2D matrix2 )
		{
			matrix1.M11 = matrix1.M11 - matrix2.M11;
			matrix1.M12 = matrix1.M12 - matrix2.M12;
			matrix1.M13 = matrix1.M13 - matrix2.M13;
			matrix1.M14 = matrix1.M14 - matrix2.M14;
			matrix1.M21 = matrix1.M21 - matrix2.M21;
			matrix1.M22 = matrix1.M22 - matrix2.M22;
			matrix1.M23 = matrix1.M23 - matrix2.M23;
			matrix1.M24 = matrix1.M24 - matrix2.M24;
			matrix1.M31 = matrix1.M31 - matrix2.M31;
			matrix1.M32 = matrix1.M32 - matrix2.M32;
			matrix1.M33 = matrix1.M33 - matrix2.M33;
			matrix1.M34 = matrix1.M34 - matrix2.M34;
			matrix1.M41 = matrix1.M41 - matrix2.M41;
			matrix1.M42 = matrix1.M42 - matrix2.M42;
			matrix1.M43 = matrix1.M43 - matrix2.M43;
			matrix1.M44 = matrix1.M44 - matrix2.M44;
			return matrix1;
		}


		/// <summary>
		/// Inverts values in the specified <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		public static Matrix2D operator -( Matrix2D matrix )
		{
			matrix.M11 = -matrix.M11;
			matrix.M12 = -matrix.M12;
			matrix.M13 = -matrix.M13;
			matrix.M14 = -matrix.M14;
			matrix.M21 = -matrix.M21;
			matrix.M22 = -matrix.M22;
			matrix.M23 = -matrix.M23;
			matrix.M24 = -matrix.M24;
			matrix.M31 = -matrix.M31;
			matrix.M32 = -matrix.M32;
			matrix.M33 = -matrix.M33;
			matrix.M34 = -matrix.M34;
			matrix.M41 = -matrix.M41;
			matrix.M42 = -matrix.M42;
			matrix.M43 = -matrix.M43;
			matrix.M44 = -matrix.M44;
			return matrix;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains subtraction of one matrix from another.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">The second <see cref="Matrix2D"/>.</param>
		/// <returns>The result of the matrix subtraction.</returns>
		public static Matrix2D Subtract( Matrix2D matrix1, Matrix2D matrix2 )
		{
			matrix1.M11 = matrix1.M11 - matrix2.M11;
			matrix1.M12 = matrix1.M12 - matrix2.M12;
			matrix1.M13 = matrix1.M13 - matrix2.M13;
			matrix1.M14 = matrix1.M14 - matrix2.M14;
			matrix1.M21 = matrix1.M21 - matrix2.M21;
			matrix1.M22 = matrix1.M22 - matrix2.M22;
			matrix1.M23 = matrix1.M23 - matrix2.M23;
			matrix1.M24 = matrix1.M24 - matrix2.M24;
			matrix1.M31 = matrix1.M31 - matrix2.M31;
			matrix1.M32 = matrix1.M32 - matrix2.M32;
			matrix1.M33 = matrix1.M33 - matrix2.M33;
			matrix1.M34 = matrix1.M34 - matrix2.M34;
			matrix1.M41 = matrix1.M41 - matrix2.M41;
			matrix1.M42 = matrix1.M42 - matrix2.M42;
			matrix1.M43 = matrix1.M43 - matrix2.M43;
			matrix1.M44 = matrix1.M44 - matrix2.M44;
			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains subtraction of one matrix from another.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">The second <see cref="Matrix2D"/>.</param>
		/// <param name="result">The result of the matrix subtraction as an output parameter.</param>
		public static void Subtract( ref Matrix2D matrix1, ref Matrix2D matrix2, out Matrix2D result )
		{
			result.M11 = matrix1.M11 - matrix2.M11;
			result.M12 = matrix1.M12 - matrix2.M12;
			result.M13 = matrix1.M13 - matrix2.M13;
			result.M14 = matrix1.M14 - matrix2.M14;
			result.M21 = matrix1.M21 - matrix2.M21;
			result.M22 = matrix1.M22 - matrix2.M22;
			result.M23 = matrix1.M23 - matrix2.M23;
			result.M24 = matrix1.M24 - matrix2.M24;
			result.M31 = matrix1.M31 - matrix2.M31;
			result.M32 = matrix1.M32 - matrix2.M32;
			result.M33 = matrix1.M33 - matrix2.M33;
			result.M34 = matrix1.M34 - matrix2.M34;
			result.M41 = matrix1.M41 - matrix2.M41;
			result.M42 = matrix1.M42 - matrix2.M42;
			result.M43 = matrix1.M43 - matrix2.M43;
			result.M44 = matrix1.M44 - matrix2.M44;
		}


		public static implicit operator Matrix( Matrix2D mat )
		{
			return new Matrix
			(
				mat.M11, mat.M12, mat.M13, mat.M14,
				mat.M21, mat.M22, mat.M23, mat.M24,
				mat.M31, mat.M32, mat.M33, mat.M34,
				mat.M41, mat.M42, mat.M43, mat.M44
			);
		}


		internal string DebugDisplayString
		{
			get
			{
				if( this == Identity )
					return "Identity";

				var radians = Mathf.atan2( M21, M11 );
				var degrees = MathHelper.ToDegrees( radians );
				return string.Format( "T:({0:0.##},{1:0.##}), R:{2:0.##}, S:({3:0.##},{4:0.##})", Translation.X, Translation.Y, degrees, Scale.X, Scale.Y );
			}
		}


		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="Matrix2D"/> in the format:
		/// {M11:[<see cref="M11"/>] M12:[<see cref="M12"/>] M13:[<see cref="M13"/>] M14:[<see cref="M14"/>]}
		/// {M21:[<see cref="M21"/>] M12:[<see cref="M22"/>] M13:[<see cref="M23"/>] M14:[<see cref="M24"/>]}
		/// {M31:[<see cref="M31"/>] M32:[<see cref="M32"/>] M33:[<see cref="M33"/>] M34:[<see cref="M34"/>]}
		/// {M41:[<see cref="M41"/>] M42:[<see cref="M42"/>] M43:[<see cref="M43"/>] M44:[<see cref="M44"/>]}
		/// </summary>
		/// <returns>A <see cref="String"/> representation of this <see cref="Matrix2D"/>.</returns>
		public override string ToString()
		{
			return "{M11:" + M11 + " M12:" + M12 + " M13:" + M13 + " M14:" + M14 + "}"
				+ " {M21:" + M21 + " M22:" + M22 + " M23:" + M23 + " M24:" + M24 + "}"
				+ " {M31:" + M31 + " M32:" + M32 + " M33:" + M33 + " M34:" + M34 + "}"
				+ " {M41:" + M41 + " M42:" + M42 + " M43:" + M43 + " M44:" + M44 + "}";
		}


		/// <summary>
		/// Swap the matrix rows and columns.
		/// </summary>
		/// <param name="matrix">The matrix for transposing operation.</param>
		/// <returns>The new <see cref="Matrix2D"/> which contains the transposing result.</returns>
		public static Matrix2D Transpose( Matrix2D matrix )
		{
			Matrix2D ret;
			Transpose( ref matrix, out ret );
			return ret;
		}


		/// <summary>
		/// Swap the matrix rows and columns.
		/// </summary>
		/// <param name="matrix">The matrix for transposing operation.</param>
		/// <param name="result">The new <see cref="Matrix2D"/> which contains the transposing result as an output parameter.</param>
		public static void Transpose( ref Matrix2D matrix, out Matrix2D result )
		{
			Matrix2D ret;

			ret.M11 = matrix.M11;
			ret.M12 = matrix.M21;
			ret.M13 = matrix.M31;
			ret.M14 = matrix.M41;

			ret.M21 = matrix.M12;
			ret.M22 = matrix.M22;
			ret.M23 = matrix.M32;
			ret.M24 = matrix.M42;

			ret.M31 = matrix.M13;
			ret.M32 = matrix.M23;
			ret.M33 = matrix.M33;
			ret.M34 = matrix.M43;

			ret.M41 = matrix.M14;
			ret.M42 = matrix.M24;
			ret.M43 = matrix.M34;
			ret.M44 = matrix.M44;

			result = ret;
		}

		#endregion


		#region Private Static Methods

		/// <summary>
		/// Helper method for using the Laplace expansion theorem using two rows expansions to calculate major and 
		/// minor determinants of a 4x4 matrix. This method is used for inverting a matrix.
		/// </summary>
		private static void FindDeterminants( ref Matrix2D matrix, out float major,
											 out float minor1, out float minor2, out float minor3, out float minor4, out float minor5, out float minor6,
											 out float minor7, out float minor8, out float minor9, out float minor10, out float minor11, out float minor12 )
		{
			double det1 = (double)matrix.M11 * (double)matrix.M22 - (double)matrix.M12 * (double)matrix.M21;
			double det2 = (double)matrix.M11 * (double)matrix.M23 - (double)matrix.M13 * (double)matrix.M21;
			double det3 = (double)matrix.M11 * (double)matrix.M24 - (double)matrix.M14 * (double)matrix.M21;
			double det4 = (double)matrix.M12 * (double)matrix.M23 - (double)matrix.M13 * (double)matrix.M22;
			double det5 = (double)matrix.M12 * (double)matrix.M24 - (double)matrix.M14 * (double)matrix.M22;
			double det6 = (double)matrix.M13 * (double)matrix.M24 - (double)matrix.M14 * (double)matrix.M23;
			double det7 = (double)matrix.M31 * (double)matrix.M42 - (double)matrix.M32 * (double)matrix.M41;
			double det8 = (double)matrix.M31 * (double)matrix.M43 - (double)matrix.M33 * (double)matrix.M41;
			double det9 = (double)matrix.M31 * (double)matrix.M44 - (double)matrix.M34 * (double)matrix.M41;
			double det10 = (double)matrix.M32 * (double)matrix.M43 - (double)matrix.M33 * (double)matrix.M42;
			double det11 = (double)matrix.M32 * (double)matrix.M44 - (double)matrix.M34 * (double)matrix.M42;
			double det12 = (double)matrix.M33 * (double)matrix.M44 - (double)matrix.M34 * (double)matrix.M43;

			major = (float)( det1 * det12 - det2 * det11 + det3 * det10 + det4 * det9 - det5 * det8 + det6 * det7 );
			minor1 = (float)det1;
			minor2 = (float)det2;
			minor3 = (float)det3;
			minor4 = (float)det4;
			minor5 = (float)det5;
			minor6 = (float)det6;
			minor7 = (float)det7;
			minor8 = (float)det8;
			minor9 = (float)det9;
			minor10 = (float)det10;
			minor11 = (float)det11;
			minor12 = (float)det12;
		}

		#endregion

	}
}