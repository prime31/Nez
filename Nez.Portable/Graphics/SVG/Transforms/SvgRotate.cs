using System.Globalization;


namespace Nez.Svg
{
	public class SvgRotate : SvgTransform
	{
		public float Angle;
		public float CenterX;
		public float CenterY;


		public SvgRotate(float angle)
		{
			Angle = angle;

			CalculateMatrix();
		}


		public SvgRotate(float angle, float centerX, float centerY)
		{
			Angle = angle;
			CenterX = centerX;
			CenterY = centerY;

			CalculateMatrix();
		}


		void CalculateMatrix()
		{
			var mat = Matrix2D.CreateTranslation(-CenterX, -CenterY);
			mat.MultiplyRotation(Angle * Mathf.Deg2Rad);
			mat.MultiplyTranslation(CenterX, CenterY);

			Matrix = mat;
		}


		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "rotate({0}, {1}, {2})", Angle, CenterX, CenterY);
		}
	}
}