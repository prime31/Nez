using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Nez
{
	public class Transform
	{
		[Flags]
		enum DirtyType
		{
			Clean,
			PositionDirty,
			ScaleDirty,
			RotationDirty
		}

		public enum Component
		{
			Position,
			Scale,
			Rotation
		}


		#region properties and fields

		/// <summary>
		/// the Entity associated with this transform
		/// </summary>
		public readonly Entity entity;

		/// <summary>
		/// the parent Transform of this Transform
		/// </summary>
		/// <value>The parent.</value>
		public Transform parent
		{
			get { return _parent; }
			set { setParent( value ); }
		}


		/// <summary>
		/// total children of this Transform
		/// </summary>
		/// <value>The child count.</value>
		public int childCount { get { return _children.Count; } }


		/// <summary>
		/// position of the transform in world space
		/// </summary>
		/// <value>The position.</value>
		public Vector2 position
		{
			get
			{
				updateTransform();
				if( _positionDirty )
				{
					if( parent == null )
					{
						_position = _localPosition;
					}
					else
					{
						parent.updateTransform();
						Vector2Ext.transform( ref _localPosition, ref parent._worldTransform, out _position );
					}

					_positionDirty = false;
				}
				return _position;
			}
			set { setPosition( value ); }
		}


		/// <summary>
		/// position of the transform relative to the parent transform. If the transform has no parent, it is the same as Transform.position
		/// </summary>
		/// <value>The local position.</value>
		public Vector2 localPosition
		{
			get
			{
				updateTransform();
				return _localPosition;
			}
			set { setLocalPosition( value ); }
		}


		/// <summary>
		/// rotation of the transform in world space in radians
		/// </summary>
		/// <value>The rotation.</value>
		public float rotation
		{
			get
			{
				updateTransform();
				return _rotation;
			}
			set { setRotation( value ); }
		}


		/// <summary>
		/// rotation of the transform in world space in degrees
		/// </summary>
		/// <value>The rotation degrees.</value>
		public float rotationDegrees
		{
			get { return MathHelper.ToDegrees( _rotation ); }
			set { setRotation( MathHelper.ToRadians( value ) ); }
		}


		/// <summary>
		/// the rotation of the transform relative to the parent transform's rotation. If the transform has no parent, it is the same as Transform.rotation
		/// </summary>
		/// <value>The local rotation.</value>
		public float localRotation
		{
			get
			{
				updateTransform();
				return _localRotation;
			}
			set { setLocalRotation( value ); }
		}


		/// <summary>
		/// rotation of the transform relative to the parent transform's rotation in degrees
		/// </summary>
		/// <value>The rotation degrees.</value>
		public float localRotationDegrees
		{
			get { return MathHelper.ToDegrees( _localRotation ); }
			set { localRotation = MathHelper.ToRadians( value ); }
		}


		/// <summary>
		/// global scale of the transform
		/// </summary>
		/// <value>The scale.</value>
		public Vector2 scale
		{
			get
			{
				updateTransform();
				return _scale;
			}
			set { setScale( value ); }
		}


		/// <summary>
		/// the scale of the transform relative to the parent. If the transform has no parent, it is the same as Transform.scale
		/// </summary>
		/// <value>The local scale.</value>
		public Vector2 localScale
		{
			get
			{
				updateTransform();
				return _localScale;
			}
			set { setLocalScale( value ); }
		}


		public Matrix2D worldInverseTransform
		{
			get
			{
				updateTransform();
				if( _worldInverseDirty )
				{
					Matrix2D.invert( ref _worldTransform, out _worldInverseTransform );
					_worldInverseDirty = false;
				}
				return _worldInverseTransform;
			}
		}


		public Matrix2D localToWorldTransform
		{
			get
			{
				updateTransform();
				return _worldTransform;
			}
		}


		public Matrix2D worldToLocalTransform
		{
			get
			{
				if( _worldToLocalDirty )
				{
					if( parent == null )
					{
						_worldToLocalTransform = Matrix2D.identity;
					}
					else
					{
						parent.updateTransform();
						Matrix2D.invert( ref parent._worldTransform, out _worldToLocalTransform );
					}

					_worldToLocalDirty = false;
				}
				return _worldToLocalTransform;
			}
		}


		Transform _parent;
		DirtyType hierarchyDirty;

		bool _localDirty;
		bool _localPositionDirty;
		bool _localScaleDirty;
		bool _localRotationDirty;
		bool _positionDirty;
		bool _worldToLocalDirty;
		bool _worldInverseDirty;

		// value is automatically recomputed from the position, rotation and scale
		Matrix2D _localTransform;

		// value is automatically recomputed from the local and the parent matrices.
		Matrix2D _worldTransform = Matrix2D.identity;
		Matrix2D _worldToLocalTransform = Matrix2D.identity;
		Matrix2D _worldInverseTransform = Matrix2D.identity;

		Matrix2D _rotationMatrix;
		Matrix2D _translationMatrix;
		Matrix2D _scaleMatrix;

		Vector2 _position;
		Vector2 _scale;
		float _rotation;

		Vector2 _localPosition;
		Vector2 _localScale;
		float _localRotation;

		List<Transform> _children = new List<Transform>();

		#endregion


		public Transform( Entity entity )
		{
			this.entity = entity;
			_scale = _localScale = Vector2.One;
		}


		/// <summary>
		/// returns the Transform child at index
		/// </summary>
		/// <returns>The child.</returns>
		/// <param name="index">Index.</param>
		public Transform getChild( int index )
		{
			return _children[index];
		}
	

		#region Fluent setters

		/// <summary>
		/// sets the parent Transform of this Transform
		/// </summary>
		/// <returns>The parent.</returns>
		/// <param name="parent">Parent.</param>
		public Transform setParent( Transform parent )
		{
			if( _parent == parent )
				return this;

			if( _parent != null )
				_parent._children.Remove( this );

			if( parent != null )
				parent._children.Add( this );

			_parent = parent;
			setDirty( DirtyType.PositionDirty );

			return this;
		}


		/// <summary>
		/// sets the position of the transform in world space
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="position">Position.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setPosition( Vector2 position )
		{
			if( position == _position )
				return this;
			
			_position = position;
			if( parent != null )
				localPosition = Vector2.Transform( _position, worldToLocalTransform );
			else
				localPosition = position;

			_positionDirty = false;

			return this;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setPosition( float x, float y )
		{
			return setPosition( new Vector2( x, y ) );
		}


		/// <summary>
		/// sets the position of the transform relative to the parent transform. If the transform has no parent, it is the same
		/// as Transform.position
		/// </summary>
		/// <returns>The local position.</returns>
		/// <param name="localPosition">Local position.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setLocalPosition( Vector2 localPosition )
		{
			if( localPosition == _localPosition )
				return this;

			_localPosition = localPosition;
			_localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
			setDirty( DirtyType.PositionDirty );

			return this;
		}


		/// <summary>
		/// sets the rotation of the transform in world space in radians
		/// </summary>
		/// <returns>The rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setRotation( float radians )
		{
			_rotation = radians;
			if( parent != null )
				localRotation = parent.rotation + radians;
			else
				localRotation = radians;
			
			return this;
		}


		/// <summary>
		/// sets the rotation of the transform in world space in degrees
		/// </summary>
		/// <returns>The rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setRotationDegrees( float degrees )
		{
			return setRotation( MathHelper.ToRadians( degrees ) );
		}


		/// <summary>
		/// sets the the rotation of the transform relative to the parent transform's rotation. If the transform has no parent, it is the
		/// same as Transform.rotation
		/// </summary>
		/// <returns>The local rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setLocalRotation( float radians )
		{
			_localRotation = radians;
			_localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
			setDirty( DirtyType.RotationDirty );

			return this;
		}


		/// <summary>
		/// sets the the rotation of the transform relative to the parent transform's rotation. If the transform has no parent, it is the
		/// same as Transform.rotation
		/// </summary>
		/// <returns>The local rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setLocalRotationDegrees( float degrees )
		{
			return setLocalRotation( MathHelper.ToRadians( degrees ) );
		}


		/// <summary>
		/// sets the global scale of the transform
		/// </summary>
		/// <returns>The scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setScale( Vector2 scale )
		{
			_scale = scale;
			if( parent != null )
				localScale = scale / parent._scale;
			else
				localScale = scale;
			
			return this;
		}


		/// <summary>
		/// sets the global scale of the transform
		/// </summary>
		/// <returns>The scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setScale( float scale )
		{
			return setScale( new Vector2( scale ) );
		}


		/// <summary>
		/// sets the scale of the transform relative to the parent. If the transform has no parent, it is the same as Transform.scale
		/// </summary>
		/// <returns>The local scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setLocalScale( Vector2 scale )
		{
			_localScale = scale;
			_localDirty = _positionDirty = _localScaleDirty = true;
			setDirty( DirtyType.ScaleDirty );

			return this;
		}


		/// <summary>
		/// sets the scale of the transform relative to the parent. If the transform has no parent, it is the same as Transform.scale
		/// </summary>
		/// <returns>The local scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Transform setLocalScale( float scale )
		{
			return setLocalScale( new Vector2( scale ) );
		}

		#endregion


		/// <summary>
		/// rounds the position of the Transform
		/// </summary>
		public void roundPosition()
		{
			position = _position.round();
		}
	

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void updateTransform()
		{
			if( hierarchyDirty != DirtyType.Clean )
			{
				if( parent != null )
					parent.updateTransform();

				if( _localDirty )
				{
					if( _localPositionDirty )
					{
						Matrix2D.createTranslation( _localPosition.X, _localPosition.Y, out _translationMatrix );
						_localPositionDirty = false;
					}

					if( _localRotationDirty )
					{
						Matrix2D.createRotation( _localRotation, out _rotationMatrix );
						_localRotationDirty = false;
					}

					if( _localScaleDirty )
					{
						Matrix2D.createScale( _localScale.X, _localScale.Y, out _scaleMatrix );
						_localScaleDirty = false;
					}

					Matrix2D.multiply( ref _scaleMatrix, ref _rotationMatrix, out _localTransform );
					Matrix2D.multiply( ref _localTransform, ref _translationMatrix, out _localTransform );

					if( parent == null )
					{
						_worldTransform = _localTransform;
						_rotation = _localRotation;
						_scale = _localScale;
						_worldInverseDirty = true;
					}
					_localDirty = false;
				}

				if( parent != null )
				{
					Matrix2D.multiply( ref _localTransform, ref parent._worldTransform, out _worldTransform );

					_rotation = _localRotation + parent._rotation;
					_scale = parent._scale * _localScale;
					_worldInverseDirty = true;
				}

				_worldToLocalDirty = true;
				_positionDirty = true;
				hierarchyDirty = DirtyType.Clean;
			}
		}


		/// <summary>
		/// sets the dirty flag on the enum and passes it down to our children
		/// </summary>
		/// <param name="dirtyFlagType">Dirty flag type.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void setDirty( DirtyType dirtyFlagType )
		{
			if( ( hierarchyDirty & dirtyFlagType ) == 0 )
			{
				hierarchyDirty |= dirtyFlagType;

				switch( dirtyFlagType )
				{
					case DirtyType.PositionDirty:
						entity.onTransformChanged( Component.Position );
						break;
					case DirtyType.RotationDirty:
						entity.onTransformChanged( Component.Rotation );
						break;
					case DirtyType.ScaleDirty:
						entity.onTransformChanged( Component.Scale );
						break;
				}

				// dirty our children as well so they know of the changes
				for( var i = 0; i < _children.Count; i++ )
					_children[i].setDirty( dirtyFlagType );
			}
		}


		public void copyFrom( Transform transform )
		{
			_position = transform.position;
			_localPosition = transform._localPosition;
			_rotation = transform._rotation;
			_localRotation = transform._localRotation;
			_scale = transform._scale;
			_localScale = transform._localScale;

			setDirty( DirtyType.PositionDirty );
			setDirty( DirtyType.RotationDirty );
			setDirty( DirtyType.ScaleDirty );
		}
	

		public override string ToString()
		{
			return string.Format( "[Transform: parent: {0}, position: {1}, rotation: {2}, scale: {3}, localPosition: {4}, localRotation: {5}, localScale: {6}]", parent != null, position, rotation, scale, localPosition, localRotation, localScale );
		}

	}
}

