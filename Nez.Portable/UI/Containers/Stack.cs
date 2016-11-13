using System;


namespace Nez.UI
{
	/// <summary>
	/// A stack is a container that sizes its children to its size and positions them at 0,0 on top of each other.
	/// 
	/// The preferred and min size of the stack is the largest preferred and min size of any children. The max size of the stack is the
	/// smallest max size of any children.
	/// </summary>
	public class Stack : Group
	{
		#region ILayout

		public override float minWidth
		{
			get
			{
				if( sizeInvalid )
					computeSize();
				return _minWidth;
			}
		}

		public override float minHeight
		{
			get
			{
				if( sizeInvalid )
					computeSize();
				return _minHeight;
			}
		}

		public override float preferredWidth
		{
			get
			{
				if( sizeInvalid )
					computeSize();
				return _prefWidth;
			}
		}

		public override float preferredHeight
		{
			get
			{
				if( sizeInvalid )
					computeSize();
				return _prefHeight;
			}
		}

		public override float maxWidth
		{
			get
			{
				if( sizeInvalid )
					computeSize();
				return _maxWidth;
			}
		}

		public override float maxHeight
		{
			get
			{
				if( sizeInvalid )
					computeSize();
				return _maxHeight;
			}
		}

		float _prefWidth, _prefHeight, _minWidth, _minHeight, _maxWidth, _maxHeight;

		#endregion

		bool sizeInvalid = true;


		public Stack()
		{
			transform = false;
			setSize( 150, 150 );
			touchable = Touchable.ChildrenOnly;
		}


		public override void invalidate()
		{
			base.invalidate();
			sizeInvalid = true;
		}


		private void computeSize()
		{
			sizeInvalid = false;
			_prefWidth = 0;
			_prefHeight = 0;
			_minWidth = 0;
			_minHeight = 0;
			_maxWidth = 0;
			_maxHeight = 0;

			for( int i = 0, n = children.Count; i < n; i++ )
			{
				var child = children[i];
				float childMaxWidth, childMaxHeight;
				if( child is ILayout )
				{
					var layout = (ILayout)child;
					_prefWidth = Math.Max( _prefWidth, layout.preferredWidth );
					_prefHeight = Math.Max( _prefHeight, layout.preferredHeight );
					_minWidth = Math.Max( _minWidth, layout.minWidth );
					_minHeight = Math.Max( _minHeight, layout.minHeight );
					childMaxWidth = layout.maxWidth;
					childMaxHeight = layout.maxHeight;
				}
				else
				{
					_prefWidth = Math.Max( _prefWidth, child.width );
					_prefHeight = Math.Max( _prefHeight, child.height );
					_minWidth = Math.Max( _minWidth, child.width );
					_minHeight = Math.Max( _minHeight, child.height );
					childMaxWidth = 0;
					childMaxHeight = 0;
				}

				if( childMaxWidth > 0 )
					_maxWidth = _maxWidth == 0 ? childMaxWidth : Math.Min( _maxWidth, childMaxWidth );
				if( childMaxHeight > 0 )
					_maxHeight = _maxHeight == 0 ? childMaxHeight : Math.Min( _maxHeight, childMaxHeight );
			}
		}


		public T add<T>( T element ) where T : Element
		{
			return addElement( element );
		}


		public override void layout()
		{
			if( sizeInvalid )
				computeSize();
			
			for( int i = 0, n = children.Count; i < n; i++ )
			{
				Element child = children[i];
				child.setBounds( 0, 0, width, height );
				if( child is ILayout )
					( (ILayout)child ).validate();
			}
		}

	}
}

