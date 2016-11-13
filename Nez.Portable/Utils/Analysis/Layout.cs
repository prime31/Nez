using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Analysis
{
	/// <summary>
	/// Alignment for layout.
	/// </summary>
	[Flags]
	public enum Alignment
	{
		None = 0,

		// Horizontal layouts
		Left = 1,
		Right = 2,
		HorizontalCenter = 4,

		// Vertical layouts
		Top = 8,
		Bottom = 16,
		VerticalCenter = 32,

		// Combinations
		TopLeft = Top | Left,
		TopRight = Top | Right,
		TopCenter = Top | HorizontalCenter,

		BottomLeft = Bottom | Left,
		BottomRight = Bottom | Right,
		BottomCenter = Bottom | HorizontalCenter,

		CenterLeft = VerticalCenter | Left,
		CenterRight = VerticalCenter | Right,
		Center = VerticalCenter | HorizontalCenter
	}


	/// <summary>
	/// Layout class that supports title safe area.
	/// </summary>
	/// <remarks>
	/// You have to support various resolutions when you develop multi-platform
	/// games. Also, you have to support title safe area for Xbox 360 games.
	/// 
	/// This structure places given rectangle with specified alignment and margin
	/// based on layout area (client area) with safe area.
	/// 
	/// Margin is percentage of client area size.
	/// 
	/// Example:
	/// 
	/// Place( region, 0.1f, 0.2f, Aligment.TopLeft );
	/// 
	/// Place region at 10% from left side of the client area,
	/// 20% from top of the client area.
	/// 
	/// 
	/// Place( region, 0.3f, 0.4f, Aligment.BottomRight );
	/// 
	/// Place region at 30% from right side of client,
	/// 40% from the bottom of the client area.
	/// 
	/// 
	/// You can individually specify client area and safe area.
	/// So, it is useful when you have split screen game which layout happens based
	/// on client and it takes care of the safe at same time.
	/// 
	/// </remarks>
	public struct Layout
	{
		/// <summary>
		/// Gets/Sets client area.
		/// </summary>
		public Rectangle clientArea;

		/// <summary>
		/// Gets/Sets safe area.
		/// </summary>
		public Rectangle safeArea;


		#region Initialization

		/// <summary>
		/// Construct layout object by specify both client area and safe area.
		/// </summary>
		/// <param name="client">Client area</param>
		/// <param name="safeArea">safe area</param>
		public Layout( Rectangle clientArea, Rectangle safeArea )
		{
			this.clientArea = clientArea;
			this.safeArea = safeArea;
		}

		/// <summary>
		/// Construct layout object by specify client area.
		/// Safe area becomes same size as client area.
		/// </summary>
		/// <param name="client">Client area</param>
		public Layout( Rectangle clientArea ) : this( clientArea, clientArea )
		{}

		/// <summary>
		/// Construct layout object by specify viewport.
		/// Safe area becomes same as Viewpoert.TItleSafeArea.
		/// </summary>
		public Layout( Viewport viewport )
		{
			clientArea = new Rectangle( (int)viewport.X, (int)viewport.Y, (int)viewport.Width, (int)viewport.Height );
			safeArea = viewport.TitleSafeArea;
		}

		#endregion


		/// <summary>
		/// Layouting specified region
		/// </summary>
		/// <param name="region">placing region</param>
		/// <returns>Placed position</returns>
		public Vector2 place( Vector2 size, float horizontalMargin, float verticalMargine, Alignment alignment )
		{
			var rc = new Rectangle( 0, 0, (int)size.X, (int)size.Y );
			rc = place( rc, horizontalMargin, verticalMargine, alignment );
			return new Vector2( rc.X, rc.Y );
		}


		/// <summary>
		/// Layouting specified region
		/// </summary>
		/// <param name="region">placing rectangle</param>
		/// <returns>placed rectangle</returns>
		public Rectangle place( Rectangle region, float horizontalMargin, float verticalMargine, Alignment alignment )
		{
			// Horizontal layout.
			if( ( alignment & Alignment.Left ) != 0 )
			{
				region.X = clientArea.X + (int)( clientArea.Width * horizontalMargin );
			}
			else if( ( alignment & Alignment.Right ) != 0 )
			{
				region.X = clientArea.X +
				(int)( clientArea.Width * ( 1.0f - horizontalMargin ) ) -
				region.Width;
			}
			else if( ( alignment & Alignment.HorizontalCenter ) != 0 )
			{
				region.X = clientArea.X + ( clientArea.Width - region.Width ) / 2 +
				(int)( horizontalMargin * clientArea.Width );
			}
			else
			{
				// Don't do layout.
			}

			// Vertical layout.
			if( ( alignment & Alignment.Top ) != 0 )
			{
				region.Y = clientArea.Y + (int)( clientArea.Height * verticalMargine );
			}
			else if( ( alignment & Alignment.Bottom ) != 0 )
			{
				region.Y = clientArea.Y +
				(int)( clientArea.Height * ( 1.0f - verticalMargine ) ) -
				region.Height;
			}
			else if( ( alignment & Alignment.VerticalCenter ) != 0 )
			{
				region.Y = clientArea.Y + ( clientArea.Height - region.Height ) / 2 +
				(int)( verticalMargine * clientArea.Height );
			}
			else
			{
				// Don't do layout.
			}

			// Make sure layout region is in the safe area.
			if( region.Left < safeArea.Left )
				region.X = safeArea.Left;

			if( region.Right > safeArea.Right )
				region.X = safeArea.Right - region.Width;

			if( region.Top < safeArea.Top )
				region.Y = safeArea.Top;

			if( region.Bottom > safeArea.Bottom )
				region.Y = safeArea.Bottom - region.Height;

			return region;
		}

	}
}