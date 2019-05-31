using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Svg
{
	/// <summary>
	/// assists in debugging the data from an SVG file. All the supported shapes will be displayed.
	/// </summary>
	public class SvgDebugComponent : RenderableComponent
	{
		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds.location = entity.transform.position;
					_areBoundsDirty = false;
				}
				return _bounds;
			}
		}

		public SvgDocument svgDoc;

		ISvgPathBuilder _pathBuilder;


		/// <summary>
		/// beware! If pathBuilder is null the SvgReflectionPathBuilder will be used and it is slow as dirt.
		/// </summary>
		/// <param name="pathToSvgFile">Path to svg file relative to the Content folder</param>
		/// <param name="pathBuilder">Path builder.</param>
		public SvgDebugComponent( string pathToSvgFile, ISvgPathBuilder pathBuilder = null )
		{
			svgDoc = SvgDocument.open( TitleContainer.OpenStream( "Content/" + pathToSvgFile ) );
			_pathBuilder = pathBuilder;
			_bounds = new RectangleF( 0, 0, svgDoc.width, svgDoc.height );

			if( _pathBuilder == null )
				_pathBuilder = new SvgReflectionPathBuilder();
		}

		public override void render( Graphics graphics, Camera camera )
		{
			// in some rare cases, there are SVG elements outside of a group so we'll render those cases first
			renderRects( graphics.batcher, svgDoc.rectangles );
			renderPaths( graphics.batcher, svgDoc.paths );
			renderLines( graphics.batcher, svgDoc.lines );
			renderCircles( graphics.batcher, svgDoc.circles );
			renderPolygons( graphics.batcher, svgDoc.polygons );
			renderPolylines( graphics.batcher, svgDoc.polylines );
			renderImages( graphics.batcher, svgDoc.images );

			if( svgDoc.groups != null )
			{
				foreach( var g in svgDoc.groups )
				{
					renderRects( graphics.batcher, g.rectangles );
					renderPaths( graphics.batcher, g.paths );
					renderLines( graphics.batcher, g.lines );
					renderCircles( graphics.batcher, g.circles );
					renderPolygons( graphics.batcher, g.polygons );
					renderPolylines( graphics.batcher, g.polylines );
					renderImages( graphics.batcher, g.images );
				}
			}
		}


		#region SVG part renderers

		void renderRects( Batcher batcher, SvgRectangle[] rects )
		{
			if( rects == null )
				return;

			foreach( var rect in rects )
			{
				var fixedPts = rect.getTransformedPoints();
				for( var i = 0; i < fixedPts.Length - 1; i++ )
					batcher.drawLine( fixedPts[i], fixedPts[i + 1], rect.strokeColor, rect.strokeWidth );
				batcher.drawLine( fixedPts[3], fixedPts[0], rect.strokeColor, rect.strokeWidth );
			}
		}

		void renderPaths( Batcher batcher, SvgPath[] paths )
		{
			if( paths == null && _pathBuilder != null )
				return;

			foreach( var path in paths )
			{
				var points = path.getTransformedDrawingPoints( _pathBuilder, 3 );
				for( var i = 0; i < points.Length - 1; i++ )
				{
					batcher.drawLine( points[i], points[i + 1], path.strokeColor, path.strokeWidth );
					batcher.drawPixel( points[i], Color.Yellow, 3 );
				}
			}
		}

		void renderLines( Batcher batcher, SvgLine[] lines )
		{
			if( lines == null )
				return;

			foreach( var line in lines )
			{
				var fixedPts = line.getTransformedPoints();
				batcher.drawLine( fixedPts[0], fixedPts[1], line.strokeColor, line.strokeWidth );
			}
		}

		void renderCircles( Batcher batcher, SvgCircle[] circles )
		{
			if( circles == null )
				return;

			foreach( var circ in circles )
				batcher.drawCircle( circ.centerX, circ.centerY, circ.radius, circ.strokeColor, (int)circ.strokeWidth );
		}

		void renderPolygons( Batcher batcher, SvgPolygon[] polygons )
		{
			if( polygons == null )
				return;

			foreach( var poly in polygons )
			{
				var fixedPts = poly.getTransformedPoints();
				for( var i = 0; i < fixedPts.Length - 1; i++ )
					batcher.drawLine( fixedPts[i], fixedPts[i + 1], poly.strokeColor, poly.strokeWidth );
			}
		}

		void renderPolylines( Batcher batcher, SvgPolyline[] polylines )
		{
			if( polylines == null )
				return;

			foreach( var poly in polylines )
			{
				var fixedPts = poly.getTransformedPoints();
				for( var i = 0; i < fixedPts.Length - 1; i++ )
					batcher.drawLine( fixedPts[i], fixedPts[i + 1], poly.strokeColor, poly.strokeWidth );
			}
		}

		/// <summary>
		/// attempts to load and draw the SvgImage. If it cannot load a Texture it will just draw a rect.
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		/// <param name="images">Images.</param>
		void renderImages( Batcher batcher, SvgImage[] images )
		{
			if( images == null )
				return;

			foreach( var image in images )
			{
				var tex = image.getTexture( entity.scene.content );
				if( tex != null )
				{
					var rotation = image.rotationDegrees * Mathf.deg2Rad;
					if( rotation == 0 )
					{
						batcher.draw( tex, image.rect );
					}
					else
					{
						var rect = image.rect;
						var origin = new Vector2( 0.5f * rect.width, 0.5f * rect.height );
						rect.location += origin;

						batcher.draw( tex, rect, null, Color.White, rotation, origin, SpriteEffects.None, layerDepth );
					}
				}
				else
				{
					batcher.drawRect( image.x, image.y, image.width, image.height, Color.DarkRed );
				}
			}
		}

		#endregion

	}
}
