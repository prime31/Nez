using System;
using System.Collections.Generic;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// helper for building a BehaviorTree using a fluent API. Leaf nodes need to first have a parent added. Parents can be Composites or
	/// Decorators. Decorators are automatically closed when a leaf node is added. Composites must have endComposite called to close them.
	/// </summary>
	public class BehaviorTreeBuilder<T>
	{
		T _context;

		/// <summary>
		/// Last node created.
		/// </summary>
		Behavior<T> _currentNode;

		/// <summary>
		/// Stack nodes that we are build via the fluent API.
		/// </summary>
		Stack<Behavior<T>> _parentNodeStack = new Stack<Behavior<T>>();


		public BehaviorTreeBuilder( T context )
		{
			_context = context;
		}


		public static BehaviorTreeBuilder<T> begin( T context )
		{
			return new BehaviorTreeBuilder<T>( context );
		}


		BehaviorTreeBuilder<T> setChildOnParent( Behavior<T> child )
		{
			var parent = _parentNodeStack.Peek();
			if( parent is Composite<T> )
			{
				( parent as Composite<T> ).addChild( child );
			}
			else if( parent is Decorator<T> )
			{
				// Decorators have just one child so end it automatically
				( parent as Decorator<T> ).child = child;
				endDecorator();
			}

			return this;
		}


		/// <summary>
		/// pushes a Composite or Decorator on the stack
		/// </summary>
		/// <returns>The parent node.</returns>
		/// <param name="composite">Composite.</param>
		BehaviorTreeBuilder<T> pushParentNode( Behavior<T> composite )
		{
			if( _parentNodeStack.Count > 0 )
				setChildOnParent( composite );

			_parentNodeStack.Push( composite );
			return this;
		}


		BehaviorTreeBuilder<T> endDecorator()
		{
			_currentNode = _parentNodeStack.Pop();
			return this;
		}


		#region Leaf Nodes (actions and sub trees)

		public BehaviorTreeBuilder<T> action( Func<T,TaskStatus> func )
		{
			Assert.isFalse( _parentNodeStack.Count == 0, "Can't create an unnested Action node. It must be a leaf node." );
			return setChildOnParent( new ExecuteAction<T>( func ) );
		}


		/// <summary>
		/// Like an action node but the function can return true/false and is mapped to success/failure.
		/// </summary>
		public BehaviorTreeBuilder<T> action( Func<T,bool> func )
		{
			return action( t => func( t ) ? TaskStatus.Success : TaskStatus.Failure );
		}


		public BehaviorTreeBuilder<T> conditional( Func<T,TaskStatus> func )
		{
			Assert.isFalse( _parentNodeStack.Count == 0, "Can't create an unnested Conditional node. It must be a leaf node." );
			return setChildOnParent( new ExecuteActionConditional<T>( func ) );
		}


		/// <summary>
		/// Like a conditional node but the function can return true/false and is mapped to success/failure.
		/// </summary>
		public BehaviorTreeBuilder<T> conditional( Func<T,bool> func )
		{
			return conditional( t => func( t ) ? TaskStatus.Success : TaskStatus.Failure );
		}


		public BehaviorTreeBuilder<T> logAction( string text )
		{
			Assert.isFalse( _parentNodeStack.Count == 0, "Can't create an unnested Action node. It must be a leaf node." );
			return setChildOnParent( new LogAction<T>( text ) );
		}


		public BehaviorTreeBuilder<T> waitAction( float waitTime )
		{
			Assert.isFalse( _parentNodeStack.Count == 0, "Can't create an unnested Action node. It must be a leaf node." );
			return setChildOnParent( new WaitAction<T>( waitTime ) );
		}


		/// <summary>
		/// Splice a sub tree into the parent tree.
		/// </summary>
		public BehaviorTreeBuilder<T> subTree( BehaviorTree<T> subTree )
		{
			Assert.isFalse( _parentNodeStack.Count == 0, "Can't splice an unnested sub tree, there must be a parent tree." );
			return setChildOnParent( new BehaviorTreeReference<T>( subTree ) );
		}

		#endregion


		#region Decorators

		public BehaviorTreeBuilder<T> conditionalDecorator( Func<T,TaskStatus> func, bool shouldReevaluate = true )
		{
			var conditional = new ExecuteActionConditional<T>( func );
			return pushParentNode( new ConditionalDecorator<T>( conditional, shouldReevaluate ) );
		}


		/// <summary>
		/// Like a conditional decorator node but the function can return true/false and is mapped to success/failure.
		/// </summary>
		public BehaviorTreeBuilder<T> conditionalDecorator( Func<T,bool> func, bool shouldReevaluate = true )
		{
			return conditionalDecorator( t => func( t ) ? TaskStatus.Success : TaskStatus.Failure, shouldReevaluate );
		}


		public BehaviorTreeBuilder<T> alwaysFail()
		{
			return pushParentNode( new AlwaysFail<T>() );
		}


		public BehaviorTreeBuilder<T> alwaysSucceed()
		{
			return pushParentNode( new AlwaysSucceed<T>() );
		}


		public BehaviorTreeBuilder<T> inverter()
		{
			return pushParentNode( new Inverter<T>() );
		}


		public BehaviorTreeBuilder<T> repeater( int count )
		{
			return pushParentNode( new Repeater<T>( count ) );
		}


		public BehaviorTreeBuilder<T> untilFail()
		{
			return pushParentNode( new UntilFail<T>() );
		}


		public BehaviorTreeBuilder<T> untilSuccess()
		{
			return pushParentNode( new UntilSuccess<T>() );
		}

		#endregion


		#region Composites

		public BehaviorTreeBuilder<T> parallel()
		{
			return pushParentNode( new Parallel<T>() );
		}


		public BehaviorTreeBuilder<T> parallelSelector()
		{
			return pushParentNode( new ParallelSelector<T>() );
		}


		public BehaviorTreeBuilder<T> selector( AbortTypes abortType = AbortTypes.None )
		{
			return pushParentNode( new Selector<T>( abortType ) );
		}


		public BehaviorTreeBuilder<T> randomSelector()
		{
			return pushParentNode( new RandomSelector<T>() );
		}


		public BehaviorTreeBuilder<T> sequence( AbortTypes abortType = AbortTypes.None )
		{
			return pushParentNode( new Sequence<T>( abortType ) );
		}


		public BehaviorTreeBuilder<T> randomSequence()
		{
			return pushParentNode( new RandomSequence<T>() );
		}


		public BehaviorTreeBuilder<T> endComposite()
		{
			Assert.isTrue( _parentNodeStack.Peek() is Composite<T>, "attempting to end a composite but the top node is a decorator" );
			_currentNode = _parentNodeStack.Pop();
			return this;
		}

		#endregion


		public BehaviorTree<T> build()
		{
			Assert.isNotNull( _currentNode, "Can't create a behaviour tree with zero nodes" );
			return new BehaviorTree<T>( _context, _currentNode );
		}

	}
}

