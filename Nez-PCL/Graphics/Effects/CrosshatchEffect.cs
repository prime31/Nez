using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class CrosshatchEffect : Effect
	{
		/// <summary>
		/// size in pixels of the crosshatch. Should be an even number because the half size is also required. Defaults to 16.
		/// </summary>
		/// <value>The size of the cross hatch.</value>
		public int crossHatchSize
		{
			get { return _crossHatchSize; }
			set
			{
				// ensure we have an even number
				if( !Mathf.isEven( value ) )
					value += 1;
				
				if( _crossHatchSize != value )
				{
					_crossHatchSize = value;
					_crosshatchSizeParam.SetValue( _crossHatchSize );
				}
			}
		}
		int _crossHatchSize = 16;
		
		EffectParameter _crosshatchSizeParam;

		
		public CrosshatchEffect() : base( Core.graphicsDevice, EffectResource.crosshatchBytes )
		{
			_crosshatchSizeParam = Parameters["crossHatchSize"];
			_crosshatchSizeParam.SetValue( _crossHatchSize );
		}
	}
}

