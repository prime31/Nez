using System.Globalization;


namespace Nez.Svg
{
	public class SvgRotate : SvgTransform
	{
		public float angle;
		public float centerX;
		public float centerY;


		public SvgRotate( float angle )
		{
			this.angle = angle;

			calculateMatrix();
		}


		public SvgRotate( float angle, float centerX, float centerY )
		{
			this.angle = angle;
			this.centerX = centerX;
			this.centerY = centerY;

			calculateMatrix();
		}


		void calculateMatrix()
		{
			var mat = Matrix2D.createTranslation( -centerX, -centerY );
			mat.multiplyRotation( angle * Mathf.deg2Rad );
			mat.multiplyTranslation( centerX, centerY );

			matrix = mat;
		}


		public override string ToString()
		{
			return string.Format( CultureInfo.InvariantCulture, "rotate({0}, {1}, {2})", angle, centerX, centerY );
		}
	}
}
