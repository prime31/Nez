using Microsoft.Xna.Framework;


namespace Nez.Tweens
{
	public class RenderableColorTween : ColorTween, ITweenTarget<Color>
	{
		RenderableComponent _renderable;


		public void setTweenedValue( Color value )
		{
			_renderable.color = value;
		}


		public Color getTweenedValue()
		{
			return _renderable.color;
		}


		public new object getTargetObject()
		{
			return _renderable;
		}


		protected override void updateValue()
		{
			setTweenedValue( Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
		}


		public void setTarget( RenderableComponent renderable )
		{
			_renderable = renderable;
		}

	}
}
