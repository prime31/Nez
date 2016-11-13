using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// basic class that can be used to create simple meshes. For more advanced usage subclass and override what is needed. The general gist
	/// of usage is the following:
	/// - call setVertPositions
	/// - call setTriangles to set the triangle indices
	/// - call recalculateBounds to prepare the Mesh for rendering and culling
	/// </summary>
	public class Mesh : RenderableComponent
	{
		/// <summary>
		/// the AABB that wraps this object
		/// </summary>
		/// <value>The bounds.</value>
		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds.calculateBounds( entity.transform.position + _topLeftVertPosition, Vector2.Zero, Vector2.Zero, entity.transform.scale, entity.transform.rotation, _width, _height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		bool _vertexColorEnabled = true;
		Texture2D _texture;
		BasicEffect _basicEffect;

		int _primitiveCount;
		Vector2 _topLeftVertPosition;
		float _width;
		float _height;
		int[] _triangles;
		VertexPositionColorTexture[] _verts;


		#region configuration

		/// <summary>
		/// recalculates the bounds and optionally sets the UVs. The UVs are setup to map the texture in a best fit fashion.
		/// </summary>
		/// <param name="recalculateUVs">If set to <c>true</c> recalculate U vs.</param>
		public Mesh recalculateBounds( bool recalculateUVs )
		{
			_topLeftVertPosition = new Vector2( float.MaxValue, float.MaxValue );
			var max = new Vector2( float.MinValue, float.MinValue );

			for( var i = 0; i < _verts.Length; i++ )
			{
				_topLeftVertPosition.X = MathHelper.Min( _topLeftVertPosition.X, _verts[i].Position.X );
				_topLeftVertPosition.Y = MathHelper.Min( _topLeftVertPosition.Y, _verts[i].Position.Y );
				max.X = MathHelper.Max( max.X, _verts[i].Position.X );
				max.Y = MathHelper.Max( max.Y, _verts[i].Position.Y );
			}

			_width = max.X - _topLeftVertPosition.X;
			_height = max.Y - _topLeftVertPosition.Y;

			// handle UVs if need be
			if( recalculateUVs )
			{
				for( var i = 0; i < _verts.Length; i++ )
				{
					_verts[i].TextureCoordinate.X = ( _verts[i].Position.X - _topLeftVertPosition.X ) / _width;
					_verts[i].TextureCoordinate.Y = ( _verts[i].Position.Y - _topLeftVertPosition.Y ) / _height;
				}
			}

			return this;
		}


		/// <summary>
		/// sets whether vertex colors will be used by the shader
		/// </summary>
		/// <returns>The enable vertex colors.</returns>
		/// <param name="shouldEnableVertexColors">If set to <c>true</c> should enable vertex colors.</param>
		public Mesh setVertexColorEnabled( bool shouldEnableVertexColors )
		{
			if( _basicEffect != null )
				_basicEffect.VertexColorEnabled = shouldEnableVertexColors;
			else
				_vertexColorEnabled = shouldEnableVertexColors;

			return this;
		}


		/// <summary>
		/// sets the texture. Pass in null to unset the texture.
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="texture">Texture.</param>
		public Mesh setTexture( Texture2D texture )
		{
			if( _basicEffect != null )
			{
				_basicEffect.Texture = texture;
				_basicEffect.TextureEnabled = texture != null;
			}
			else
			{
				// store this away until the BasicEffect is created
				_texture = texture;
			}

			return this;
		}


		/// <summary>
		/// helper that sets the color for all verts
		/// </summary>
		/// <param name="color">Color.</param>
		public Mesh setColorForAllVerts( Color color )
		{
			for( var i = 0; i < _verts.Length; i++ )
				_verts[i].Color = color;
			return this;
		}


		/// <summary>
		/// sets the color for all of the verts
		/// </summary>
		/// <returns>The color.</returns>
		/// <param name="color">Color.</param>
		public new Mesh setColor( Color color )
		{
			setColorForAllVerts( color );
			return this;
		}


		/// <summary>
		/// sets the vertex color for a single vert
		/// </summary>
		/// <returns>The color for vert.</returns>
		/// <param name="vertIndex">Vert index.</param>
		/// <param name="color">Color.</param>
		public Mesh setColorForVert( int vertIndex, Color color )
		{
			_verts[vertIndex].Color = color;
			return this;
		}


		/// <summary>
		/// sets the vert positions. If the positions array does not match the verts array size the verts array will be recreated.
		/// </summary>
		/// <param name="positions">Positions.</param>
		public Mesh setVertPositions( Vector2[] positions )
		{
			if( _verts == null || _verts.Length != positions.Length )
				_verts = new VertexPositionColorTexture[positions.Length];

			for( var i = 0; i < _verts.Length; i++ )
				_verts[i].Position = positions[i].toVector3();
			return this;
		}


		/// <summary>
		/// sets the vert positions. If the positions array does not match the verts array size the verts array will be recreated.
		/// </summary>
		/// <param name="positions">Positions.</param>
		public Mesh setVertPositions( Vector3[] positions )
		{
			if( _verts == null || _verts.Length != positions.Length )
				_verts = new VertexPositionColorTexture[positions.Length];

			for( var i = 0; i < _verts.Length; i++ )
				_verts[i].Position = positions[i];
			return this;
		}


		/// <summary>
		/// sets the triangle indices for rendering
		/// </summary>
		/// <returns>The triangles.</returns>
		/// <param name="triangles">Triangles.</param>
		public Mesh setTriangles( int[] triangles )
		{
			Assert.isTrue( triangles.Length % 3 == 0, "triangles must be a multiple of 3" );
			_primitiveCount = triangles.Length / 3;
			_triangles = triangles;
			return this;
		}

		#endregion


		#region RenderableComponent

		public override void onAddedToEntity()
		{
			_basicEffect = entity.scene.content.loadMonoGameEffect<BasicEffect>();
			_basicEffect.VertexColorEnabled = _vertexColorEnabled;

			if( _texture != null )
			{
				_basicEffect.Texture = _texture;
				_basicEffect.TextureEnabled = true;
				_texture = null;
			}
		}


		public override void render( Graphics graphics, Camera camera )
		{
			_basicEffect.Projection = camera.projectionMatrix;
			_basicEffect.View = camera.transformMatrix;
			_basicEffect.World = entity.transform.localToWorldTransform;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			Core.graphicsDevice.DrawUserIndexedPrimitives( PrimitiveType.TriangleList, _verts, 0, _verts.Length, _triangles, 0, _primitiveCount );
		}

		#endregion

	}
}

