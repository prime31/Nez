using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class Mesh : RenderableComponent
	{
		public override float width
		{
			get { return 50; }
		}

		public override float height
		{
			get { return 50; }
		}

		BasicEffect _basicEffect;
		VertexPositionColorTexture[] _verts;
		int _primitiveCount;


		public Mesh()
		{
			var totalVerts = 3;
			_verts = new VertexPositionColorTexture[totalVerts];
			for( var i = 0; i < totalVerts; i++ )
			{
				_verts[i] = new VertexPositionColorTexture();
				_verts[i].Color = Color.White;
			}
				
			_verts[0].Position = new Vector3( -25, -25, 0 );
			_verts[0].TextureCoordinate = new Vector2( 0, 1 );
			_verts[1].Position = new Vector3( 25, -25, 0 );
			_verts[1].TextureCoordinate = new Vector2( 1, 1 );
			_verts[2].Position = new Vector3( 25, 25, 0 );
			_verts[2].TextureCoordinate = new Vector2( 1, 0 );

			_primitiveCount = 1;
		}


		public override void onAddedToEntity()
		{
			_basicEffect = entity.scene.contentManager.loadMonoGameEffect<BasicEffect>();
			_basicEffect.World = Matrix.Identity;
			_basicEffect.VertexColorEnabled = true;
			_basicEffect.TextureEnabled = true;
			_basicEffect.Texture = entity.scene.contentManager.Load<Texture2D>( "Images/dots-512" );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( isVisibleFromCamera( camera ) )
			{
				_basicEffect.Projection = camera.getProjectionMatrix();
				_basicEffect.View = camera.transformMatrix;
				_basicEffect.World = entity.transform.localToWorldTransform;
				_basicEffect.CurrentTechnique.Passes[0].Apply();

				Core.graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
				Core.graphicsDevice.DrawUserPrimitives( PrimitiveType.TriangleList, _verts, 0, _primitiveCount, VertexPositionColorTexture.VertexDeclaration );

				// primitive count helper
				//case PrimitiveType.TriangleStrip: { return verticesOrIndices - 2; }
				//sccase PrimitiveType.TriangleList: { return verticesOrIndices / 3; }
			}
		}
	}
}

