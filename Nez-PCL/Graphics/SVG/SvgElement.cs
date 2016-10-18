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
		[XmlAttribute( "id" )]
		public string id;

		[XmlAttribute( "stroke" )]
		public string strokeAttribute
		{
			get { return null; }
			set
			{
				if( value.StartsWith( "#" ) )
					strokeColor = ColorExt.hexToColor( value.Substring( 1 ) );
			}
		}

		public Color strokeColor = Color.Red;

		[XmlAttribute( "fill" )]
		public string fillAttribute
		{
			get { return null; }
			set
			{
				if( value.StartsWith( "#" ) )
					fillColor = ColorExt.hexToColor( value.Substring( 1 ) );
			}
		}

		public Color fillColor;

		[XmlAttribute( "stroke-width" )]
		public string strokeWidthAttribute
		{
			get { return null; }
			set { float.TryParse( value, out strokeWidth ); }
		}

		public float strokeWidth = 1;

		[XmlAttribute( "transform" )]
		public string transformAttribute
		{
			get { return null; }
			set { _transforms = SvgTransformConverter.parseTransforms( value ); }
		}

		protected List<SvgTransform> _transforms;


		public Matrix2D getCombinedMatrix()
		{
			var m = Matrix2D.identity;
			if( _transforms != null && _transforms.Count > 0 )
			{
				foreach( var trans in _transforms )
					m = Matrix2D.multiply( m, trans.matrix );
			}

			return m;
		}

		/// <summary>
		/// helper property that just loops through all the transforms and if there is an SvgRotate transform it will return that angle
		/// </summary>
		/// <value>The rotation degrees.</value>
		public float rotationDegrees
		{
			get
			{
				if( _transforms == null )
					return 0;

				for( var i = 0; i < _transforms.Count; i++ )
				{
					if( _transforms[i] is SvgRotate )
						return ( _transforms[i] as SvgRotate ).angle;
				}

				return 0;
			}
		}

	}
}
