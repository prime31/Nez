﻿using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class ScanlinesEffect : Effect
	{
		[Range( 0.001f, 1f, 0.001f )]
		public float attenuation
		{
			get { return _attenuation; }
			set
			{
				if( _attenuation != value )
				{
					_attenuation = value;
					_attenuationParam.SetValue( _attenuation );
				}
			}
		}

		[Range( 10, 1000, 1 )]
		public float linesFactor
		{
			get { return _linesFactor; }
			set
			{
				if( _linesFactor != value )
				{
					_linesFactor = value;
					_linesFactorParam.SetValue( _linesFactor );
				}
			}
		}


		float _attenuation = 0.04f;
		float _linesFactor = 800f;

		EffectParameter _attenuationParam;
		EffectParameter _linesFactorParam;

		
		public ScanlinesEffect() : base( Core.graphicsDevice, EffectResource.scanlinesBytes )
		{
			_attenuationParam = Parameters["_attenuation"];
			_linesFactorParam = Parameters["_linesFactor"];

			_attenuationParam.SetValue( _attenuation );
			_linesFactorParam.SetValue( _linesFactor );
		}
	}
}

