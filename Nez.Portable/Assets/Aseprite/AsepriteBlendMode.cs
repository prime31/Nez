namespace Nez.Aseprite
{
	/// <summary>
	/// Defines the blend mode used by Aseprite layers when blending cels.
	/// </summary>
	public enum AsepriteBlendMode
	{
		/// <summary>
		///     Normal blend mode is the standard blend mode that takes the top layer alone without mixing any color from
		///     the layer beneath it.
		///     <code>f(a,b) = b</code>
		/// </summary>
		Normal = 0,

		/// <summary>
		///     Multiply blend mode that takes the RGB component values of each pixel from the top layer and multiplies them
		///     with the RGB component values of the corresponding pixel from the bottom layer.
		///     <code>f(a,b) = ab</code>
		/// </summary>
		Multiply = 1,

		/// <summary>
		///     Screen blend mode takes the RGB component values of each pixel from the top and bottom layer and inverts 
		///     them, then multiples the RGB component values of each pixel from the top layer with the RGB component values
		///     of the corresponding pixel from the bottom layer, then the RGB component value of each resulting pixel is 
		///     inverted again.
		///     <code>f(a,b) = 1-(1-a)(1-b)</code>
		/// </summary>
		Screen = 2,

		/// <summary>
		///     Overlay blend combines the Multiply and Screen blend modes based on the tonal value of the bottom layer. If 
		///     the bottom layer is darker than 50% gray, then the tonal values are multiplied; otherwise, they get 
		///     screened.  In both cases the resulting value is doubled after.
		///     <code>f(a,b) = 2ab when a less than 0.5</code>
		///     <code>f(a,b) = 1-2(1-a)(1-b) when a equal to or greater than 0.5</code>
		/// </summary>
		Overlay = 3,

		/// <summary>
		///     Darken blend retains the smallest of each RGB component for each corresponding pixel from the top and bottom
		///     layer.
		///     <code>f((r1,g1,b1), (r2,b2,g2)) = [min(r1,r2), min(g1,g2), min(b1,b2)]</code>
		/// </summary>
		Darken = 4,

		/// <summary>
		///     Lighten blend retains the largest of each RGB component for each corresponding pixel from the top and bottom
		///     layer.
		/// <code>f((r1,g1,b1), (r2,b2,g2)) = [max(r1,r2), max(g1,g2), max(b1,b2)]</code>
		/// </summary>
		Lighten = 5,

		/// <summary>
		///     Color Dodge blend divides each pixel from the bottom layer with the corresponding inverted pixel from the
		///     top layer.
		///     <code>f(a,b) = a/(1-b)</code>
		/// </summary>
		ColorDodge = 6,

		/// <summary>
		///     Color Burn blend divides each inverted pixel from the bottom layer with the corresponding pixel from the top
		///     layer, then inverts the resulting value.
		/// <code>f(a,b) = 1-(1-a)/b</code>
		/// </summary>
		ColorBurn = 7,

		/// <summary>
		///     Hard Light blend combines the Multiply and Screen blend modes based on the tonal value of the top layer. 
		///     If the top layer is darker than 50% gray, then the tonal values are multiplied; otherwise, they get 
		///     screened.  In both cases the resulting value is doubled after.
		///     <code>f(a,b) = 2ab when b less than 0.5</code>
		///     <code>f(a,b) = 1-2(1-a)(1-b) when b equal to or greater than 0.5</code>
		/// </summary>
		HardLight = 8,

		/// <summary>
		///     Soft Light blend modulates the tonal values of the bottom layer by the tonal values of the top layer.
		///     <code>f(a,b) = (2b-1)(a-a^2)+a when b is less than 0.5</code>
		///     <code>f(a,b) = (2b-1)(sqrt(a)-a)+a when b is equal to or greater than 0.5</code>
		/// </summary>
		SoftLight = 9,

		/// <summary>
		///     Difference blend returns the absolute value in the difference between RGB component value of each pixel in 
		///     the top layer from the RGB component value in the corresponding pixel in the bottom layer.
		///     <code>f(a,b) = |a-b|</code>
		/// </summary>
		Difference = 10,

		/// <summary>
		///     Exclusion blend mode takes the sum of the RGB component values of each pixel in the top layer with the RGB
		///     component value of each corresponding pixel in the bottom layer, then subtracts the doubled product of top 
		///     and bottom layer.
		/// <code>f(a,b) = a+b-2ab</code>
		/// </summary>
		Exclusion = 11,

		/// <summary>
		///     Hue blend mode preserves the luma and chroma of each pixel in the bottom layer and adopts the hue of the
		///     corresponding pixel in the top layer.
		///     <code>f((Ha,Sa,La),(Hb,Sb,Lb)) = (Hb, Sa, La)</code>
		/// </summary>
		Hue = 12,

		/// <summary>
		///     Saturation blend mode preserves the luma and hue of each pixel in the bottom layer and adopts the chroma of
		///     the corresponding pixel in the top layer.
		///     <code>f((Ha,Sa,La),(Hb,Sb,Lb)) = (Ha, Sb, La)</code>
		/// </summary>
		Saturation = 13,

		/// <summary>
		///     The color blend mode preserves the luma of each pixel in the bottom layer and adopts the hue and chorma of
		///     corresponding pixel in the top layer.
		///     <code>f((Ha,Sa,La),(Hb,Sb,Lb)) = (Hb, Sb, La)</code>
		/// </summary>
		Color = 14,

		/// <summary>
		///     Luminosity blend mode preserves the hue and chroma of each pixel in the bottom layer and adopts the luma of
		///     the corresponding pixel in then top layer.
		///     <code>f((Ha,Sa,La),(Hb,Sb,Lb)) = (Ha, Sa, Lb)</code>
		/// </summary>
		Luminosity = 15,

		/// <summary>
		///     Addition blend mode adds the RGB component values of each pixel from the top layer with the RGB component 
		///     values of each corresponding pixel in the bottom layer.
		///     <code>f(a,b) = a + b</code>
		/// </summary>
		Addition = 16,

		/// <summary>
		///     Subtract blend mode subtracts the RGB component values of each pixel from the top layer from the RGB 
		///     component values of each corresponding pixel in the bottom layer.
		///     <code>f(a,b) = a - b</code>
		/// </summary>
		Subtract = 17,

		/// <summary>
		///     Divide blend mode divides the RGB component values of each pixel from the bottom layer by the RGB component
		///     values of each corresponding pixel from the top layer.
		///     <code>f(a,b) = a/b</code>
		/// </summary>
		Divide = 18
	}
}