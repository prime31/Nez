using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.PhysicsShapes;
using Nez.Textures;

namespace Nez
{
	/// <summary>
	/// StencilLightRenderer is used for 2d lights and shadows. This works by taking each light and doing the following:
	/// - clear the stencil buffer
	/// - project from the light position to create the shadow projection of the light
	/// - render the shadow project only to the stencil buffer writing a 1 for each touched pixel
	/// - render the lights additively only where the stencil is 0 (where the shadow projection did not hit)
	/// 
	/// Setup:
	/// - add the StencilLightRenderer and have it render before other renderers
	/// - add all your StencilLights or textured lights to the StencilLightRenderer RenderLayer
	/// - you will then have access to StencilLightRenderer.RenderTexture which will contain your lightmap. You can then render it whenever you want
	///		either via a normal SpriteRenderer with multiplicative blending or via a PostProcessor
	/// </summary>
	public class StencilLightRenderer : Renderer
	{
		/// <summary>
		/// the layer we will use for our lights
		/// </summary>
		public int RenderLayer;

		/// <summary>
		/// layer mask of all the layers this light should interact with. defaults to all layers.
		/// </summary>
		public int CollidesWithLayers = Physics.AllLayers;

		const int CIRCLE_APPROXIMATION_VERTS = 12;
		Vector2[] _vertBuffer = new Vector2[4];
		PrimitiveBatch _primitiveBatch;
		DepthStencilState _depthStencilState;
		BlendState _stencilOnlyBlendState;

		public StencilLightRenderer(int renderOrder, int renderLayer, RenderTexture renderTexture) : base(renderOrder, null)
		{
			RenderLayer = renderLayer;
			RenderTexture = renderTexture;
			_primitiveBatch = new PrimitiveBatch();

			// create our material used for texture lights that only writes where the stencil is 0 and is additive
			Material = Material.StencilRead(0);
			Material.BlendState = new BlendState
			{
				ColorSourceBlend = Blend.SourceAlpha,
				ColorDestinationBlend = Blend.One,
				AlphaSourceBlend = Blend.SourceAlpha,
				AlphaDestinationBlend = Blend.One
			};

			// create our stencil for writing our occluders, only writing to the stencil buffer
			_depthStencilState = new DepthStencilState
			{
				StencilEnable = true,
				StencilFunction = CompareFunction.Always,
				StencilPass = StencilOperation.Replace,
				ReferenceStencil = 1,
				DepthBufferEnable = false,
			};
			_stencilOnlyBlendState = new BlendState
			{
				ColorWriteChannels = ColorWriteChannels.None
			};
		}

		public override void Render(Scene scene)
		{
			Insist.IsNotNull(RenderTexture, $"{nameof(StencilLightRenderer)} requires a RenderTexture!");

			var cam = Camera ?? scene.Camera;
			BeginRender(cam);

			var renderables = scene.RenderableComponents.ComponentsWithRenderLayer(RenderLayer);
			for (var i = 0; i < renderables.Length; i++)
			{
				var renderable = renderables.Buffer[i];
				if (renderable.Enabled && renderable.IsVisibleFromCamera(cam))
					RenderLight(renderable, cam);
			}

			if (ShouldDebugRender && Core.DebugRenderEnabled)
				DebugRender(scene, cam);

			EndRender();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void RenderLight(IRenderable renderable, Camera cam)
		{
			// fetch all the Colliders in range
			var colliders = Physics.BoxcastBroadphase(renderable.Bounds, CollidesWithLayers);
			var colliderCount = IEnumerableExtensions.IEnumerableExt.Count(colliders);

			if (colliderCount > 0)
			{
				Core.GraphicsDevice.DepthStencilState = _depthStencilState;
				Core.GraphicsDevice.BlendState = _stencilOnlyBlendState;
				Core.GraphicsDevice.Clear(ClearOptions.Stencil, new Color(0, 0, 0, 0), 0, 0);

				_primitiveBatch.Begin(cam.ProjectionMatrix, cam.TransformMatrix);
				foreach (var collider in colliders)
				{
					if (collider.Shape is Polygon shape)
						RenderPolygon(shape, renderable.Bounds.Center);
					else if (collider.Shape is Circle circle)
						RenderCircle(circle, renderable.Bounds.Center);
					else
						throw new System.NotImplementedException();
				}
				_primitiveBatch.End();
			}

			RenderAfterStateCheck(renderable, cam);
			Graphics.Instance.Batcher.FlushBatch();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void RenderPolygon(Polygon polygon, Vector2 lightPos) => RenderVerts(polygon.Position, lightPos, polygon.Points);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void RenderCircle(Circle circle, Vector2 lightPos)
		{
			var points = Polygon.BuildSymmetricalPolygon(CIRCLE_APPROXIMATION_VERTS, circle.Radius);
			RenderVerts(circle.Position, lightPos, points);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void RenderVerts(Vector2 position, Vector2 lightPos, Vector2[] verts)
		{
			// TODO: should we check to see if the light is inside the verts and early out?
			for (var i = 0; i < verts.Length; i++)
			{
				var vertex = verts[i] + position;
				var nextVertex = verts[(i + 1) % verts.Length] + position;
				var startToEnd = nextVertex - vertex;
				var normal = new Vector2(startToEnd.Y, -startToEnd.X);
				normal.Normalize();
				var lightToStart = lightPos - vertex;

				Vector2.Dot(ref normal, ref lightToStart, out var nDotL);
				if (nDotL > 0)
				{
					var midpoint = (nextVertex + vertex) * 0.5f;
					Debug.DrawLine(vertex, nextVertex, Color.Green);
					Debug.DrawLine(midpoint, midpoint + normal * 20, Color.Green);

					var point1 = nextVertex + (Vector2.Normalize(nextVertex - lightPos) * Screen.Width);
					var point2 = vertex + (Vector2.Normalize(vertex - lightPos) * Screen.Width);

					var poly = new Vector2[] { nextVertex, point1, point2, vertex };
					_vertBuffer[0] = nextVertex;
					_vertBuffer[1] = point1;
					_vertBuffer[2] = point2;
					_vertBuffer[3] = vertex;
					_primitiveBatch.DrawPolygon(poly, 4, Color.Black);
				}
			}
		}
	}
}
