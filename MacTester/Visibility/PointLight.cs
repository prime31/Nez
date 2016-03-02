using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nez;


namespace Nez.Shadows
{
	/// <summary>
	/// Point light that also casts shadows
	/// </summary>
	public class PointLight : RenderableComponent
	{
		/// <summary>
		/// layer mask of all the layers this light should interact with. defaults to all layers.
		/// </summary>
		public int collidesWithLayers = Physics.allLayers;

		public override float width { get { return radius * 2f; } }
		public override float height { get { return radius * 2f; } }

		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds.calculateBounds( entity.transform.position, _localPosition, new Vector2( radius, radius ), entity.transform.scale, entity.transform.rotation, width, height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// Radius of influence of the light
		/// </summary>
		public float radius;

		/// <summary>
		/// Power of the light, from 0 (turned off) to 
		/// 1 for maximum brightness        
		/// </summary>
		public float power;

		Effect _lightEffect;


		public PointLight( float radius ) : this( radius, Color.White )
		{}


		public PointLight( float radius, Color color ) : this( radius, color, 1.0f )
		{}


		public PointLight( float radius, Color color, float power )
		{
			this.radius = radius;
			this.power = power;
			this.color = color;
		}


		public override void onAddedToEntity()
		{
			_lightEffect = entity.scene.contentManager.loadEffect( "Content/effects/Light.mgfxo" );
		}


		public override void render( Graphics graphics, Camera camera )
		{
			if( power > 0 && isVisibleFromCamera( camera ) )
			{
				var size = radius * 2f;
				var obstacles = Physics.boxcastBroadphase( new RectangleF( entity.transform.position.X - radius, entity.transform.position.Y - radius, size, size ), collidesWithLayers );

				// Compute the visibility mesh
				var visibility = new VisibilityComputer( entity.transform.position, radius );
				foreach( var v in obstacles )
					visibility.addSquareOccluder( v.bounds );

				// Generate a triangle list from the encounter points
				VertexPositionTexture[] vertices;
				short[] indices;

				var encounters = visibility.computeVisibilityPolygon();
				triangleListFromEncounters( encounters, out vertices, out indices );

				Core.graphicsDevice.BlendState = BlendState.Additive;
				Core.graphicsDevice.RasterizerState = RasterizerState.CullNone;

				// Apply the effect
				_lightEffect.Parameters["viewProjectionMatrix"].SetValue( entity.scene.camera.getViewProjectionMatrix() );
				_lightEffect.Parameters["lightSource"].SetValue( entity.transform.position );
				_lightEffect.Parameters["lightColor"].SetValue( color.ToVector3() * power );
				_lightEffect.Parameters["lightRadius"].SetValue( radius );
				_lightEffect.Techniques[0].Passes[0].Apply();

				// Draw the light on screen, using the triangle fan from the computed
				// visibility mesh so that the light only influences the area that can be 
				// "seen" from the light's position.
				Core.graphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>
                (
					PrimitiveType.TriangleList,
					vertices,
					0,
					vertices.Length,
					indices,
					0,
					indices.Length / 3
				);
			}
		}


		void triangleListFromEncounters( List<Vector2> encounters, out VertexPositionTexture[] vertexArray, out short[] indexArray )
		{
			var vertices = new List<VertexPositionTexture>();

			// Add a vertex for the center of the mesh
			vertices.Add( new VertexPositionTexture( new Vector3( entity.transform.position.X, entity.transform.position.Y, 0 ), entity.transform.position ) );

			// Add all the other encounter points as vertices storing their world position as UV coordinates
			foreach( var vertex in encounters )
				vertices.Add( new VertexPositionTexture( new Vector3( vertex.X, vertex.Y, 0 ), vertex ) );

			// Compute the indices to form triangles
			var indices = new List<short>();
			for( int i = 0; i < encounters.Count; i += 2 )
			{
				indices.Add( 0 );
				indices.Add( (short)( i + 2 ) );
				indices.Add( (short)( i + 1 ) );                
			}

			vertexArray = vertices.ToArray<VertexPositionTexture>();
			indexArray = indices.ToArray<short>();
		}

	}
}
