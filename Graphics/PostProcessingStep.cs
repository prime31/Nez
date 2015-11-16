using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// Post Processing step for rendering actions after everthing done.
	/// </summary>
	public interface PostProcessingStep
	{
		/// <summary>
		/// Step is Enabled or not.
		/// </summary>
		bool enabled { get; set; }

		/// <summary>
		/// Render PostProcess to target texture.
		/// </summary>
		/// <param name="gameContext">Game Context</param>
		/// <param name="source">Source Input</param>
		/// <param name="target">Rendered Output.</param>
		void process( Texture2D source, RenderTarget2D target );

	}
}

