using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// basic class that can be used to create simple meshes. For more advanced usage subclass and override what is needed.
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
					_bounds.calculateBounds( entity.transform.position + _minVertPosition, _localOffset, _origin, entity.transform.scale, entity.transform.rotation, _width, _height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		public VertexPositionColorTexture[] verts;
		public PrimitiveType primitiveType = PrimitiveType.TriangleList;
		public Texture2D texture
		{
			set
			{
				// if we have our BasicEffect loaded set the texture directly else store it off for when the BasicEffect is created
				if( _basicEffect != null )
					_basicEffect.Texture = value;
				else
					_texture = value;
			}
		}
		Texture2D _texture;

		BasicEffect _basicEffect;
		int _primitiveCount;
		Vector2 _minVertPosition;
		float _width;
		float _height;


		public Mesh()
		{}


		public Mesh( Texture2D texture )
		{
			_texture = texture;
		}


		public override void onAddedToEntity()
		{
			_basicEffect = entity.scene.contentManager.loadMonoGameEffect<BasicEffect>();
			_basicEffect.VertexColorEnabled = true;
			_basicEffect.TextureEnabled = true;
			_basicEffect.Texture = _texture;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			_basicEffect.Projection = camera.projectionMatrix;
			_basicEffect.View = camera.transformMatrix;
			_basicEffect.World = entity.transform.localToWorldTransform;
			_basicEffect.CurrentTechnique.Passes[0].Apply();

			Core.graphicsDevice.SamplerStates[0] = Core.defaultSamplerState;
			Core.graphicsDevice.DrawUserPrimitives( primitiveType, verts, 0, _primitiveCount, VertexPositionColorTexture.VertexDeclaration );
		}
	
	
		/// <summary>
		/// recalculates the bounds and optionally sets the UVs. The UVs are setup to map the texture in a best fit fashion.
		/// </summary>
		/// <param name="recalculateUVs">If set to <c>true</c> recalculate U vs.</param>
		public void recalculateBounds( bool recalculateUVs )
		{
			// TODO: use Rectanglef.rectEncompassingPoints
			_minVertPosition = new Vector2( float.MaxValue, float.MaxValue );
			var max = new Vector2( float.MinValue, float.MinValue );

			for( var i = 0; i < verts.Length; i++ )
			{
				_minVertPosition.X = MathHelper.Min( _minVertPosition.X, verts[i].Position.X );
				_minVertPosition.Y = MathHelper.Min( _minVertPosition.Y, verts[i].Position.Y );
				max.X = MathHelper.Max( max.X, verts[i].Position.X );
				max.Y = MathHelper.Max( max.Y, verts[i].Position.Y );
			}

			_width = max.X - _minVertPosition.X;
			_height = max.Y - _minVertPosition.Y;


			// handle UVs if need be
			if( recalculateUVs )
			{
				for( var i = 0; i < verts.Length; i++ )
				{
					verts[i].TextureCoordinate.X = ( verts[i].Position.X - _minVertPosition.X ) / _width;
					verts[i].TextureCoordinate.Y = ( verts[i].Position.Y - _minVertPosition.Y ) / _height;
				}
			}


			// calculate primitive count as well
			switch( primitiveType )
			{
				case PrimitiveType.TriangleList:
					_primitiveCount = verts.Length / 3;
					break;
				case PrimitiveType.TriangleStrip:
					_primitiveCount = verts.Length - 2;
					break;
			}
		}


		/// <summary>
		/// helper that sets the color for all verts
		/// </summary>
		/// <param name="color">Color.</param>
		public void setColorForAllVerts( Color color )
		{
			for( var i = 0; i < verts.Length; i++ )
				verts[i].Color = color;
		}


		/// <summary>
		/// sets the vert positions. If the positions array does not match the verts array size the verts array will be recreated.
		/// </summary>
		/// <param name="positions">Positions.</param>
		public void setVertPositions( Vector3[] positions )
		{
			if( verts == null || verts.Length != positions.Length )
				verts = new VertexPositionColorTexture[positions.Length];

			for( var i = 0; i < verts.Length; i++ )
				verts[i].Position = positions[i];
		}

	}
}

