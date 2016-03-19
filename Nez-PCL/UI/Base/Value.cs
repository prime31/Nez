using System;


namespace Nez.UI
{
	/// <summary>
	/// Value placeholder, allowing the value to be computed on request. Values are provided an element for context which reduces the
	/// number of value instances that need to be created and reduces verbosity in code that specifies values
	/// </summary>
	public abstract class Value
	{
		/// <summary>
		/// context May be null
		/// </summary>
		/// <param name="context">Context.</param>
		abstract public float get( Element context );

		/// <summary>
		/// A value that is always zero.
		/// </summary>
		static public Fixed zero = new Fixed( 0 );


		/// <summary>
		/// A fixed value that is not computed each time it is used.
		/// </summary>
		public class Fixed : Value
		{
			private float value;

			public Fixed( float value )
			{
				this.value = value;
			}

			public override float get( Element context )
			{
				return value;
			}
		}


		static public Value minWidth = new MinWidthValue();

		/// <summary>
		/// Value that is the minWidth of the element in the cell.
		/// </summary>
		public class MinWidthValue : Value
		{
			public override float get( Element context )
			{
				if( context is ILayout )
					return ( (ILayout)context ).minWidth;
				return context == null ? 0 : context.width;
			}
		}


		static public Value minHeight = new MinHeightValue();

		/// <summary>
		/// Value that is the minHeight of the element in the cell.
		/// </summary>
		public class MinHeightValue : Value
		{
			public override float get( Element context )
			{
				if( context is ILayout )
					return ( (ILayout)context ).minHeight;
				return context == null ? 0 : context.height;
			}
		}


		static public Value prefWidth = new PrefWidthValue();

		/// <summary>
		/// Value that is the prefWidth of the element in the cell.
		/// </summary>
		public class PrefWidthValue : Value
		{
			public override float get( Element context )
			{
				if( context is ILayout )
					return ( (ILayout)context ).preferredWidth;
				return context == null ? 0 : context.width;

			}
		}


		static public Value prefHeight = new PrefHeightValue();

		/// <summary>
		/// Value that is the prefHeight of the element in the cell.
		/// </summary>
		public class PrefHeightValue : Value
		{
			public override float get( Element context )
			{
				if( context is ILayout )
					return ( (ILayout)context ).preferredHeight;
				return context == null ? 0 : context.height;
			}
		}


		static public Value maxWidth = new MaxWidthValue();

		/// <summary>
		/// Value that is the maxWidth of the element in the cell.
		/// </summary>
		public class MaxWidthValue : Value
		{
			public override float get( Element context )
			{
				if( context is ILayout )
					return ( (ILayout)context ).maxWidth;
				return context == null ? 0 : context.width;
			}
		}


		static public Value maxHeight = new MaxHeightValue();

		/// <summary>
		/// Value that is the maxHeight of the element in the cell.
		/// </summary>
		public class MaxHeightValue : Value
		{
			public override float get( Element context )
			{
				if( context is ILayout )
					return ( (ILayout)context ).maxHeight;
				return context == null ? 0 : context.height;
			}
		}


		/// <summary>
		/// Value that is the maxHeight of the element in the cell.
		/// </summary>
		static public Value percentWidth( float percent )
		{
			return new PercentWidthValue() {
				percent = percent
			};
		}

		/// <summary>
		/// Returns a value that is a percentage of the element's width.
		/// </summary>
		public class PercentWidthValue : Value
		{
			public float percent;

			public override float get( Element element )
			{
				return element.width * percent;
			}
		}


		/// <summary>
		/// Returns a value that is a percentage of the specified elements's width. The context element is ignored.
		/// </summary>
		static public Value percentWidth( float percent, Element delegateElement )
		{
			return new PercentWidthDelegateValue() {
				delegateElement = delegateElement,
				percent = percent
			};
		}

		/// <summary>
		/// Returns a value that is a percentage of the specified elements's width. The context element is ignored.
		/// </summary>
		public class PercentWidthDelegateValue : Value
		{
			public Element delegateElement;
			public float percent;

			public override float get( Element element )
			{
				return delegateElement.width * percent;
			}
		}


		/// <summary>
		/// Returns a value that is a percentage of the element's height.
		/// </summary>
		static public Value percentHeight( float percent )
		{
			return new PercentageHeightValue() {
				percent = percent
			};
		}

		/// <summary>
		/// Returns a value that is a percentage of the element's height.
		/// </summary>
		public class PercentageHeightValue : Value
		{
			public float percent;

			public override float get( Element element )
			{
				return element.height * percent;
			}
		}


		/// <summary>
		/// Returns a value that is a percentage of the specified elements's height. The context element is ignored.
		/// </summary>
		static public Value percentHeight( float percent, Element delegateElement )
		{
			return new PercentHeightDelegateValue() {
				delegateElement = delegateElement,
				percent = percent
			};
		}

		/// <summary>
		/// Returns a value that is a percentage of the specified elements's height. The context element is ignored.
		/// </summary>
		public class PercentHeightDelegateValue : Value
		{
			public Element delegateElement;
			public float percent;

			public override float get( Element element )
			{
				return delegateElement.height * percent;
			}
		}

	}
}

