using System;


namespace Nez
{
	/// <summary>
	/// Range attribute.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class RangeAttribute : InspectableAttribute
	{
		public float minValue, maxValue, stepSize;


		public RangeAttribute( float minValue, float maxValue, float stepSize )
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.stepSize = stepSize;
		}


		public RangeAttribute( float minValue, float maxValue ) : this( minValue, maxValue, 0.1f )
		{ }

	}
}

