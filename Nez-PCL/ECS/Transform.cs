using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez
{
	public class Transform
	{
		[Flags]
		enum DirtyFlagType
		{
			Clean,
			PositionDirty,
			ScaleDirty,
			RotationDirty = 4,
			All = 7
		}


		#region properties and fields

		internal Entity entity;

		/// <summary>
		/// the parent Transform of this Transform
		/// </summary>
		/// <value>The parent.</value>
		public Transform parent
		{
			get { return _parent; }
			set
			{
				if( _parent == value )
					return;

				if( _parent != null )
					_parent._children.Remove( this );

				if( value != null )
					value._children.Add( this );

				_parent = value;
			}
		}
		Transform _parent;


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
						Vector2.Transform( ref _localPosition, ref parent._worldTransform, out _position );
					}
					_positionDirty = false;
				}
				return _position;
			}
			set
			{
				_position = value;
				if( parent != null )
				{
					localPosition = Vector2.Transform( _position, worldToLocalTransform );
				}
				else
				{
					localPosition = value;
				}
				_positionDirty = false;
			}
		}


		/// <summary>
		/// rotation of the transform in world space
		/// </summary>
		/// <value>The rotation.</value>
		public float rotation
		{
			get
			{
				updateTransform();
				return _rotation;
			}
			set
			{
				_rotation = value;
				if( parent != null )
				{
					localRotation = parent.rotation + value;
				}
				else
				{
					localRotation = value;
				}
			}
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
			set
			{
				_scale = value;
				if( parent != null )
				{
					localScale = value / parent._scale;
				}
				else
				{
					localScale = value;
				}
			}
		}


		/// <summary>
		/// position of the transform relative to the parent transform
		/// </summary>
		/// <value>The local position.</value>
		public Vector2 localPosition
		{
			get
			{
				updateTransform();
				return _localPosition;
			}
			set
			{
				_localPosition = value;
				_localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
				setDirty( DirtyFlagType.PositionDirty );
			}
		}


		/// <summary>
		/// the rotation of the transform relative to the parent transform's rotation
		/// </summary>
		/// <value>The local rotation.</value>
		public float localRotation
		{
			get
			{
				updateTransform();
				return _localRotation;
			}
			set
			{
				_localRotation = value;
				_localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
				setDirty( DirtyFlagType.RotationDirty );
			}
		}


		/// <summary>
		/// the scale of the transform relative to the parent
		/// </summary>
		/// <value>The local scale.</value>
		public Vector2 localScale
		{
			get
			{
				updateTransform();
				return _localScale;
			}
			set
			{
				_localScale = value;
				_localDirty = _positionDirty = _localScaleDirty = true;
				setDirty( DirtyFlagType.ScaleDirty );
			}
		}


		public Matrix worldInverseTransform
		{
			get
			{
				updateTransform();
				if( _worldInverseDirty )
				{
					Matrix.Invert( ref _worldTransform, out _worldInverseTransform );
					_worldInverseDirty = false;
				}
				return _worldInverseTransform;
			}
		}


		public Matrix localToWorldTransform
		{
			get
			{
				updateTransform();
				return _worldTransform;
			}
		}


		public Matrix worldToLocalTransform
		{
			get
			{
				if( _worldToLocalDirty )
				{
					if( parent == null )
					{
						_worldToLocalTransform = Matrix.Identity;
					}
					else
					{
						Matrix.Invert( ref parent._worldTransform, out _worldToLocalTransform );
					}
					_worldToLocalDirty = false;
				}
				return _worldToLocalTransform;
			}
		}


		DirtyFlagType hierarchyDirty;

		bool _localDirty;
		bool _localPositionDirty;
		bool _localScaleDirty;
		bool _localRotationDirty;
		bool _positionDirty;
		bool _worldToLocalDirty;
		bool _worldInverseDirty;

		// value is automatically recomputed from the position, rotation and scale
		Matrix _localTransform;

		// value is automatically recomputed from the local and the parent matrices.
		Matrix _worldTransform;
		Matrix _worldToLocalTransform;
		Matrix _worldInverseTransform;

		Matrix _rotationMatrix;
		Matrix _translationMatrix;
		Matrix _scaleMatrix;

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


		void updateTransform()
		{
			if( hierarchyDirty != DirtyFlagType.Clean )
			{
				if( parent != null && hierarchyDirty != DirtyFlagType.Clean )
				{
					parent.updateTransform();
				}

				if( _localDirty )
				{
					if( _localPositionDirty )
					{
						//Matrix.CreateTranslation( ref localPosition, out TranslationMatrix );
						Matrix.CreateTranslation( _localPosition.X, _localPosition.Y, 0, out _translationMatrix );
						_localPositionDirty = false;
					}

					if( _localRotationDirty )
					{
						//Matrix.CreateFromQuaternion( ref localOrientation, out QuaternionMatrix );
						Matrix.CreateRotationZ( _localRotation, out _rotationMatrix );
						_localRotationDirty = false;
					}

					if( _localScaleDirty )
					{
						//Matrix.CreateScale( ref localScale, out ScaleMatrix );
						Matrix.CreateScale( _localScale.X, _localScale.Y, 1f, out _scaleMatrix );
						_localScaleDirty = false;
					}

					Matrix.Multiply( ref _scaleMatrix, ref _rotationMatrix, out _localTransform );
					Matrix.Multiply( ref _localTransform, ref _translationMatrix, out _localTransform );

					if( parent == null )
					{
						_worldTransform = _localTransform;
						_rotation = _localRotation;
						_scale = _localScale;
						_worldInverseDirty = true;
					}
					_localDirty = true;
				}

				if( parent != null )
				{
					Matrix.Multiply( ref _localTransform, ref parent._worldTransform, out _worldTransform );
					//Quaternion.Concatenate( ref localOrientation, ref ParentTransform.orientation, out orientation );
					_rotation = _localRotation + parent._rotation;
					_scale = parent._scale * _localScale;
					_worldInverseDirty = true;
				}

				_worldToLocalDirty = true;
				_positionDirty = true;
				hierarchyDirty = DirtyFlagType.Clean;
			}
		}


		/// <summary>
		/// sets the dirty flag on the enum and passes it down to our children
		/// </summary>
		/// <param name="dirtyFlagType">Dirty flag type.</param>
		void setDirty( DirtyFlagType dirtyFlagType )
		{
			if( ( hierarchyDirty & dirtyFlagType ) == 0 )
			{
				hierarchyDirty |= dirtyFlagType;

				entity.onTransformChanged();

				// dirty our children as well so they know of the changes
				for( int i = 0; i < _children.Count; i++ )
					_children[i].setDirty( dirtyFlagType );
			}
		}


		public override string ToString()
		{
			return string.Format( "[Transform: parent: {0}, position: {1}, rotation: {2}, scale: {3}, localPosition: {4}, localRotation: {5}, localScale: {6}]", parent != null, position, rotation, scale, localPosition, localRotation, localScale );
		}

	}
}

