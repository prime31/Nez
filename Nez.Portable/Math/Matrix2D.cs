using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Represents the right-handed 3x3 floating point matrix, which can store translation, scale and rotation information.
	/// </summary>
	[DebuggerDisplay("{debugDisplayString,nq}")]
	public struct Matrix2D : IEquatable<Matrix2D>
	{
		#region Public Fields

		public float M11; // x scale
		public float M12;

		public float M21;
		public float M22; // y scale

		public float M31; // x translation
		public float M32; // y translation

		#endregion


		#region Public Properties

		/// <summary>
		/// Returns the identity matrix.
		/// </summary>
		public static Matrix2D Identity => _identity;

		/// <summary>
		/// Position stored in this matrix.
		/// </summary>
		public Vector2 Translation
		{
			get => new Vector2(M31, M32);
			set
			{
				M31 = value.X;
				M32 = value.Y;
			}
		}

		/// <summary>
		/// rotation in radians stored in this matrix
		/// </summary>
		/// <value>The rotation.</value>
		public float Rotation
		{
			get => Mathf.Atan2(M21, M11);
			set
			{
				var val1 = Mathf.Cos(value);
				var val2 = Mathf.Sin(value);

				M11 = val1;
				M12 = val2;
				M21 = -val2;
				M22 = val1;
			}
		}

		/// <summary>
		/// rotation in degrees stored in this matrix
		/// </summary>
		/// <value>The rotation degrees.</value>
		public float RotationDegrees
		{
			get => MathHelper.ToDegrees(Rotation);
			set => Rotation = MathHelper.ToRadians(value);
		}

		/// <summary>
		/// Scale stored in this matrix.
		/// </summary>
		public Vector2 Scale
		{
			get => new Vector2(M11, M22);
			set
			{
				M11 = value.X;
				M22 = value.Y;
			}
		}

		#endregion


		static Matrix2D _identity = new Matrix2D(1f, 0f, 0f, 1f, 0f, 0f);


		/// <summary>
		/// Constructs a matrix.
		/// </summary>
		public Matrix2D(float m11, float m12, float m21, float m22, float m31, float m32)
		{
			M11 = m11;
			M12 = m12;

			M21 = m21;
			M22 = m22;

			M31 = m31;
			M32 = m32;
		}


		#region Public static methods

		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> which contains sum of two matrixes.
		/// </summary>
		/// <param name="matrix1">The first matrix to add.</param>
		/// <param name="matrix2">The second matrix to add.</param>
		/// <returns>The result of the matrix addition.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D Add(Matrix2D matrix1, Matrix2D matrix2)
		{
			matrix1.M11 += matrix2.M11;
			matrix1.M12 += matrix2.M12;

			matrix1.M21 += matrix2.M21;
			matrix1.M22 += matrix2.M22;

			matrix1.M31 += matrix2.M31;
			matrix1.M32 += matrix2.M32;

			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> which contains sum of two matrixes.
		/// </summary>
		/// <param name="matrix1">The first matrix to add.</param>
		/// <param name="matrix2">The second matrix to add.</param>
		/// <param name="result">The result of the matrix addition as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Add(ref Matrix2D matrix1, ref Matrix2D matrix2, out Matrix2D result)
		{
			result.M11 = matrix1.M11 + matrix2.M11;
			result.M12 = matrix1.M12 + matrix2.M12;

			result.M21 = matrix1.M21 + matrix2.M21;
			result.M22 = matrix1.M22 + matrix2.M22;

			result.M31 = matrix1.M31 + matrix2.M31;
			result.M32 = matrix1.M32 + matrix2.M32;
		}


		/// <summary>
		/// Creates a new rotation <see cref="Matrix2D"/> around Z axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>The rotation <see cref="Matrix2D"/> around Z axis.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D CreateRotation(float radians)
		{
			Matrix2D result;
			CreateRotation(radians, out result);
			return result;
		}


		/// <summary>
		/// Creates a new rotation <see cref="Matrix2D"/> around Z axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix2D"/> around Z axis as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateRotation(float radians, out Matrix2D result)
		{
			result = Identity;

			var val1 = (float) Math.Cos(radians);
			var val2 = (float) Math.Sin(radians);

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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D CreateScale(float scale)
		{
			Matrix2D result;
			CreateScale(scale, scale, out result);
			return result;
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="scale">Scale value for all three axises.</param>
		/// <param name="result">The scaling <see cref="Matrix2D"/> as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateScale(float scale, out Matrix2D result)
		{
			CreateScale(scale, scale, out result);
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xScale">Scale value for X axis.</param>
		/// <param name="yScale">Scale value for Y axis.</param>
		/// <returns>The scaling <see cref="Matrix2D"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D CreateScale(float xScale, float yScale)
		{
			Matrix2D result;
			CreateScale(xScale, yScale, out result);
			return result;
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xScale">Scale value for X axis.</param>
		/// <param name="yScale">Scale value for Y axis.</param>
		/// <param name="result">The scaling <see cref="Matrix2D"/> as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateScale(float xScale, float yScale, out Matrix2D result)
		{
			result.M11 = xScale;
			result.M12 = 0;

			result.M21 = 0;
			result.M22 = yScale;

			result.M31 = 0;
			result.M32 = 0;
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="scale"><see cref="Vector2"/> representing x and y scale values.</param>
		/// <returns>The scaling <see cref="Matrix2D"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D CreateScale(Vector2 scale)
		{
			Matrix2D result;
			CreateScale(ref scale, out result);
			return result;
		}


		/// <summary>
		/// Creates a new scaling <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="scale"><see cref="Vector3"/> representing x,y and z scale values.</param>
		/// <param name="result">The scaling <see cref="Matrix2D"/> as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateScale(ref Vector2 scale, out Matrix2D result)
		{
			result.M11 = scale.X;
			result.M12 = 0;

			result.M21 = 0;
			result.M22 = scale.Y;

			result.M31 = 0;
			result.M32 = 0;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <returns>The translation <see cref="Matrix2D"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D CreateTranslation(float xPosition, float yPosition)
		{
			Matrix2D result;
			CreateTranslation(xPosition, yPosition, out result);
			return result;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <param name="result">The translation <see cref="Matrix2D"/> as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateTranslation(ref Vector2 position, out Matrix2D result)
		{
			result.M11 = 1;
			result.M12 = 0;

			result.M21 = 0;
			result.M22 = 1;

			result.M31 = position.X;
			result.M32 = position.Y;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <returns>The translation <see cref="Matrix2D"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D CreateTranslation(Vector2 position)
		{
			Matrix2D result;
			CreateTranslation(ref position, out result);
			return result;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <param name="result">The translation <see cref="Matrix2D"/> as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateTranslation(float xPosition, float yPosition, out Matrix2D result)
		{
			result.M11 = 1;
			result.M12 = 0;

			result.M21 = 0;
			result.M22 = 1;

			result.M31 = xPosition;
			result.M32 = yPosition;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Determinant()
		{
			return M11 * M22 - M12 * M21;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Invert(ref Matrix2D matrix, out Matrix2D result)
		{
			var det = 1 / matrix.Determinant();

			result.M11 = matrix.M22 * det;
			result.M12 = -matrix.M12 * det;

			result.M21 = -matrix.M21 * det;
			result.M22 = matrix.M11 * det;

			result.M31 = (matrix.M32 * matrix.M21 - matrix.M31 * matrix.M22) * det;
			result.M32 = -(matrix.M32 * matrix.M11 - matrix.M31 * matrix.M12) * det;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by the elements of another matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix2D"/>.</param>
		/// <returns>The result of dividing the matrix.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D Divide(Matrix2D matrix1, Matrix2D matrix2)
		{
			matrix1.M11 = matrix1.M11 / matrix2.M11;
			matrix1.M12 = matrix1.M12 / matrix2.M12;

			matrix1.M21 = matrix1.M21 / matrix2.M21;
			matrix1.M22 = matrix1.M22 / matrix2.M22;

			matrix1.M31 = matrix1.M31 / matrix2.M31;
			matrix1.M32 = matrix1.M32 / matrix2.M32;
			return matrix1;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by the elements of another matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix2D"/>.</param>
		/// <param name="result">The result of dividing the matrix as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Divide(ref Matrix2D matrix1, ref Matrix2D matrix2, out Matrix2D result)
		{
			result.M11 = matrix1.M11 / matrix2.M11;
			result.M12 = matrix1.M12 / matrix2.M12;

			result.M21 = matrix1.M21 / matrix2.M21;
			result.M22 = matrix1.M22 / matrix2.M22;

			result.M31 = matrix1.M31 / matrix2.M31;
			result.M32 = matrix1.M32 / matrix2.M32;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <returns>The result of dividing a matrix by a scalar.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D Divide(Matrix2D matrix1, float divider)
		{
			float num = 1f / divider;
			matrix1.M11 = matrix1.M11 * num;
			matrix1.M12 = matrix1.M12 * num;

			matrix1.M21 = matrix1.M21 * num;
			matrix1.M22 = matrix1.M22 * num;

			matrix1.M31 = matrix1.M31 * num;
			matrix1.M32 = matrix1.M32 * num;

			return matrix1;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <param name="result">The result of dividing a matrix by a scalar as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Divide(ref Matrix2D matrix1, float divider, out Matrix2D result)
		{
			float num = 1f / divider;
			result.M11 = matrix1.M11 * num;
			result.M12 = matrix1.M12 * num;

			result.M21 = matrix1.M21 * num;
			result.M22 = matrix1.M22 * num;

			result.M31 = matrix1.M31 * num;
			result.M32 = matrix1.M32 * num;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains linear interpolation of the values in specified matrixes.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">The second <see cref="Vector2"/>.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <returns>>The result of linear interpolation of the specified matrixes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D Lerp(Matrix2D matrix1, Matrix2D matrix2, float amount)
		{
			matrix1.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
			matrix1.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);

			matrix1.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
			matrix1.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);

			matrix1.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
			matrix1.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains linear interpolation of the values in specified matrixes.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">The second <see cref="Vector2"/>.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="result">The result of linear interpolation of the specified matrixes as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Lerp(ref Matrix2D matrix1, ref Matrix2D matrix2, float amount, out Matrix2D result)
		{
			result.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
			result.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);

			result.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
			result.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);

			result.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
			result.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/>.</param>
		/// <returns>Result of the matrix multiplication.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D Multiply(Matrix2D matrix1, Matrix2D matrix2)
		{
			var m11 = (matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21);
			var m12 = (matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22);

			var m21 = (matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21);
			var m22 = (matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22);

			var m31 = (matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21) + matrix2.M31;
			var m32 = (matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22) + matrix2.M32;

			matrix1.M11 = m11;
			matrix1.M12 = m12;

			matrix1.M21 = m21;
			matrix1.M22 = m22;

			matrix1.M31 = m31;
			matrix1.M32 = m32;
			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/>.</param>
		/// <param name="result">Result of the matrix multiplication as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Multiply(ref Matrix2D matrix1, ref Matrix2D matrix2, out Matrix2D result)
		{
			var m11 = (matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21);
			var m12 = (matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22);

			var m21 = (matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21);
			var m22 = (matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22);

			var m31 = (matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21) + matrix2.M31;
			var m32 = (matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22) + matrix2.M32;

			result.M11 = m11;
			result.M12 = m12;

			result.M21 = m21;
			result.M22 = m22;

			result.M31 = m31;
			result.M32 = m32;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of <see cref="Matrix2D"/> and a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <returns>Result of the matrix multiplication with a scalar.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D Multiply(Matrix2D matrix1, float scaleFactor)
		{
			matrix1.M11 *= scaleFactor;
			matrix1.M12 *= scaleFactor;

			matrix1.M21 *= scaleFactor;
			matrix1.M22 *= scaleFactor;

			matrix1.M31 *= scaleFactor;
			matrix1.M32 *= scaleFactor;
			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of <see cref="Matrix2D"/> and a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <param name="result">Result of the matrix multiplication with a scalar as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Multiply(ref Matrix2D matrix1, float scaleFactor, out Matrix2D result)
		{
			result.M11 = matrix1.M11 * scaleFactor;
			result.M12 = matrix1.M12 * scaleFactor;

			result.M21 = matrix1.M21 * scaleFactor;
			result.M22 = matrix1.M22 * scaleFactor;

			result.M31 = matrix1.M31 * scaleFactor;
			result.M32 = matrix1.M32 * scaleFactor;
		}


		/// <summary>
		/// Adds two matrixes.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the add sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/> on the right of the add sign.</param>
		/// <returns>Sum of the matrixes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D operator +(Matrix2D matrix1, Matrix2D matrix2)
		{
			matrix1.M11 = matrix1.M11 + matrix2.M11;
			matrix1.M12 = matrix1.M12 + matrix2.M12;

			matrix1.M21 = matrix1.M21 + matrix2.M21;
			matrix1.M22 = matrix1.M22 + matrix2.M22;

			matrix1.M31 = matrix1.M31 + matrix2.M31;
			matrix1.M32 = matrix1.M32 + matrix2.M32;
			return matrix1;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by the elements of another <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the div sign.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix2D"/> on the right of the div sign.</param>
		/// <returns>The result of dividing the matrixes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D operator /(Matrix2D matrix1, Matrix2D matrix2)
		{
			matrix1.M11 = matrix1.M11 / matrix2.M11;
			matrix1.M12 = matrix1.M12 / matrix2.M12;

			matrix1.M21 = matrix1.M21 / matrix2.M21;
			matrix1.M22 = matrix1.M22 / matrix2.M22;

			matrix1.M31 = matrix1.M31 / matrix2.M31;
			matrix1.M32 = matrix1.M32 / matrix2.M32;
			return matrix1;
		}


		/// <summary>
		/// Divides the elements of a <see cref="Matrix2D"/> by a scalar.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/> on the left of the div sign.</param>
		/// <param name="divider">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing a matrix by a scalar.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D operator /(Matrix2D matrix, float divider)
		{
			float num = 1f / divider;
			matrix.M11 = matrix.M11 * num;
			matrix.M12 = matrix.M12 * num;

			matrix.M21 = matrix.M21 * num;
			matrix.M22 = matrix.M22 * num;

			matrix.M31 = matrix.M31 * num;
			matrix.M32 = matrix.M32 * num;
			return matrix;
		}


		/// <summary>
		/// Compares whether two <see cref="Matrix2D"/> instances are equal without any tolerance.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the equal sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/> on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Matrix2D matrix1, Matrix2D matrix2)
		{
			return (
				matrix1.M11 == matrix2.M11 &&
				matrix1.M12 == matrix2.M12 &&
				matrix1.M21 == matrix2.M21 &&
				matrix1.M22 == matrix2.M22 &&
				matrix1.M31 == matrix2.M31 &&
				matrix1.M32 == matrix2.M32
			);
		}


		/// <summary>
		/// Compares whether two <see cref="Matrix2D"/> instances are not equal without any tolerance.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the not equal sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/> on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Matrix2D matrix1, Matrix2D matrix2)
		{
			return (
				matrix1.M11 != matrix2.M11 ||
				matrix1.M12 != matrix2.M12 ||
				matrix1.M21 != matrix2.M21 ||
				matrix1.M22 != matrix2.M22 ||
				matrix1.M31 != matrix2.M31 ||
				matrix1.M32 != matrix2.M32
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D operator *(Matrix2D matrix1, Matrix2D matrix2)
		{
			var m11 = (matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21);
			var m12 = (matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22);

			var m21 = (matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21);
			var m22 = (matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22);

			var m31 = (matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21) + matrix2.M31;
			var m32 = (matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22) + matrix2.M32;

			matrix1.M11 = m11;
			matrix1.M12 = m12;

			matrix1.M21 = m21;
			matrix1.M22 = m22;

			matrix1.M31 = m31;
			matrix1.M32 = m32;

			return matrix1;
		}


		/// <summary>
		/// Multiplies the elements of matrix by a scalar.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/> on the left of the mul sign.</param>
		/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
		/// <returns>Result of the matrix multiplication with a scalar.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D operator *(Matrix2D matrix, float scaleFactor)
		{
			matrix.M11 = matrix.M11 * scaleFactor;
			matrix.M12 = matrix.M12 * scaleFactor;

			matrix.M21 = matrix.M21 * scaleFactor;
			matrix.M22 = matrix.M22 * scaleFactor;

			matrix.M31 = matrix.M31 * scaleFactor;
			matrix.M32 = matrix.M32 * scaleFactor;
			return matrix;
		}


		/// <summary>
		/// Subtracts the values of one <see cref="Matrix2D"/> from another <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/> on the left of the sub sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/> on the right of the sub sign.</param>
		/// <returns>Result of the matrix subtraction.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D operator -(Matrix2D matrix1, Matrix2D matrix2)
		{
			matrix1.M11 = matrix1.M11 - matrix2.M11;
			matrix1.M12 = matrix1.M12 - matrix2.M12;

			matrix1.M21 = matrix1.M21 - matrix2.M21;
			matrix1.M22 = matrix1.M22 - matrix2.M22;

			matrix1.M31 = matrix1.M31 - matrix2.M31;
			matrix1.M32 = matrix1.M32 - matrix2.M32;
			return matrix1;
		}


		/// <summary>
		/// Inverts values in the specified <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix2D"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D operator -(Matrix2D matrix)
		{
			matrix.M11 = -matrix.M11;
			matrix.M12 = -matrix.M12;

			matrix.M21 = -matrix.M21;
			matrix.M22 = -matrix.M22;

			matrix.M31 = -matrix.M31;
			matrix.M32 = -matrix.M32;
			return matrix;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains subtraction of one matrix from another.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">The second <see cref="Matrix2D"/>.</param>
		/// <returns>The result of the matrix subtraction.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D Subtract(Matrix2D matrix1, Matrix2D matrix2)
		{
			matrix1.M11 = matrix1.M11 - matrix2.M11;
			matrix1.M12 = matrix1.M12 - matrix2.M12;

			matrix1.M21 = matrix1.M21 - matrix2.M21;
			matrix1.M22 = matrix1.M22 - matrix2.M22;

			matrix1.M31 = matrix1.M31 - matrix2.M31;
			matrix1.M32 = matrix1.M32 - matrix2.M32;
			return matrix1;
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains subtraction of one matrix from another.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">The second <see cref="Matrix2D"/>.</param>
		/// <param name="result">The result of the matrix subtraction as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Subtract(ref Matrix2D matrix1, ref Matrix2D matrix2, out Matrix2D result)
		{
			result.M11 = matrix1.M11 - matrix2.M11;
			result.M12 = matrix1.M12 - matrix2.M12;

			result.M21 = matrix1.M21 - matrix2.M21;
			result.M22 = matrix1.M22 - matrix2.M22;

			result.M31 = matrix1.M31 - matrix2.M31;
			result.M32 = matrix1.M32 - matrix2.M32;
		}


		/// <summary>
		/// Swap the matrix rows and columns.
		/// </summary>
		/// <param name="matrix">The matrix for transposing operation.</param>
		/// <returns>The new <see cref="Matrix2D"/> which contains the transposing result.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D Transpose(Matrix2D matrix)
		{
			Matrix2D ret;
			Transpose(ref matrix, out ret);
			return ret;
		}


		/// <summary>
		/// Swap the matrix rows and columns.
		/// </summary>
		/// <param name="matrix">The matrix for transposing operation.</param>
		/// <param name="result">The new <see cref="Matrix2D"/> which contains the transposing result as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Transpose(ref Matrix2D matrix, out Matrix2D result)
		{
			Matrix2D ret;
			ret.M11 = matrix.M11;
			ret.M12 = matrix.M21;

			ret.M21 = matrix.M12;
			ret.M22 = matrix.M22;

			ret.M31 = 0;
			ret.M32 = 0;
			result = ret;
		}

		#endregion


		#region public methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MultiplyTranslation(float x, float y)
		{
			var trans = CreateTranslation(x, y);
			Multiply(ref this, ref trans, out this);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MultiplyRotation(float radians)
		{
			var rot = CreateRotation(radians);
			Multiply(ref this, ref rot, out this);
		}

		#endregion


		#region Object

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Matrix2D"/> without any tolerance.
		/// </summary>
		/// <param name="other">The <see cref="Matrix2D"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(Matrix2D other)
		{
			return this == other;
		}


		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/> without any tolerance.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Matrix2D)
				return Equals((Matrix2D) obj);

			return false;
		}


		/// <summary>
		/// Gets the hash code of this <see cref="Matrix2D"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Matrix2D"/>.</returns>
		public override int GetHashCode()
		{
			return M11.GetHashCode() + M12.GetHashCode() + M21.GetHashCode() + M22.GetHashCode() + M31.GetHashCode() +
			       M32.GetHashCode();
		}


		public static implicit operator Matrix(Matrix2D mat)
		{
			return new Matrix
			(
				mat.M11, mat.M12, 0, 0,
				mat.M21, mat.M22, 0, 0,
				0, 0, 1, 0,
				mat.M31, mat.M32, 0, 1
			);
		}


		internal string DebugDisplayString
		{
			get
			{
				if (this == Identity)
					return "Identity";

				return string.Format("T:({0:0.##},{1:0.##}), R:{2:0.##}, S:({3:0.##},{4:0.##})", Translation.X,
					Translation.Y, RotationDegrees, Scale.X, Scale.Y);
			}
		}


		public override string ToString()
		{
			return "{M11:" + M11 + " M12:" + M12 + "}"
			       + " {M21:" + M21 + " M22:" + M22 + "}"
			       + " {M31:" + M31 + " M32:" + M32 + "}";
		}

		#endregion
	}
}