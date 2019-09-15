using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;


namespace Nez.Svg
{
	/// <summary>
	/// base class for all SVG elements. Has some helpers for parsing colors and dealing with transforms.
	/// </summary>
	public abstract class SvgElement
	{
		[XmlAttribute("id")] public string Id;

		[XmlAttribute("stroke")]
		public string StrokeAttribute
		{
			get => null;
			set
			{
				if (value.StartsWith("#"))
					StrokeColor = ColorExt.HexToColor(value.Substring(1));
			}
		}

		public Color StrokeColor = Color.Red;

		[XmlAttribute("fill")]
		public string FillAttribute
		{
			get => null;
			set
			{
				if (value.StartsWith("#"))
					FillColor = ColorExt.HexToColor(value.Substring(1));
			}
		}

		public Color FillColor;

		[XmlAttribute("stroke-width")]
		public string StrokeWidthAttribute
		{
			get => null;
			set => float.TryParse(value, out StrokeWidth);
		}

		public float StrokeWidth = 1;

		[XmlAttribute("transform")]
		public string TransformAttribute
		{
			get => null;
			set => _transforms = SvgTransformConverter.ParseTransforms(value);
		}

		protected List<SvgTransform> _transforms;


		public Matrix2D GetCombinedMatrix()
		{
			var m = Matrix2D.Identity;
			if (_transforms != null && _transforms.Count > 0)
			{
				foreach (var trans in _transforms)
					m = Matrix2D.Multiply(m, trans.Matrix);
			}

			return m;
		}

		/// <summary>
		/// helper property that just loops through all the transforms and if there is an SvgRotate transform it will return that angle
		/// </summary>
		/// <value>The rotation degrees.</value>
		public float RotationDegrees
		{
			get
			{
				if (_transforms == null)
					return 0;

				for (var i = 0; i < _transforms.Count; i++)
				{
					if (_transforms[i] is SvgRotate)
						return (_transforms[i] as SvgRotate).Angle;
				}

				return 0;
			}
		}
	}
}