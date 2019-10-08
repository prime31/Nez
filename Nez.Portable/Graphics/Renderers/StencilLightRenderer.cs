using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.PhysicsShapes;

namespace Nez
{
	/// <summary>
	/// wip: not ready for use!
	/// TODO: circle could use optimzation
	/// TODO: combine circle and polygon code
	/// TODO: expose blend states
	/// </summary>
	public class StencilLightRenderer : Renderer
	{
		public int RenderLayer;

		const int CIRCLE_APPROXIMATION_VERTS = 12;
		PrimitiveBatch _primitiveBatch;
		DepthStencilState _depthStencilState;
		BlendState _stencilOnlyBlendState;

		public StencilLightRenderer(int renderOrder, int renderLayer) : base(renderOrder, null)
		{
			RenderLayer = renderLayer;
			_primitiveBatch = new PrimitiveBatch();

			Material = Material.StencilRead(0);
			Material.BlendState = new BlendState
			{
				ColorSourceBlend = Blend.SourceAlpha,
				ColorDestinationBlend = Blend.One,
				AlphaSourceBlend = Blend.SourceAlpha,
				AlphaDestinationBlend = Blend.One
			};

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
			var colliders = Physics.BoxcastBroadphase(renderable.Bounds);
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
				}
				_primitiveBatch.End();
			}

			RenderAfterStateCheck(renderable, cam);
			Graphics.Instance.Batcher.FlushBatch();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void RenderPolygon(Polygon polygon, Vector2 lightPos)
		{
			var totalVerts = polygon.Points.Length;
			for (var i = 0; i < totalVerts; i++)
			{
				var vertex = polygon.Points[i] + polygon.position;
				var nextVertex = polygon.Points[(i + 1) % totalVerts] + polygon.position;
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

					var point1 = vertex + (Vector2.Normalize(vertex - lightPos) * Screen.Width);
					var point2 = nextVertex + (Vector2.Normalize(nextVertex - lightPos) * Screen.Width);

					var poly = new Vector2[] { nextVertex, point2, point1, vertex };
					_primitiveBatch.DrawPolygon(poly, 4, Color.Black);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void RenderCircle(Circle circle, Vector2 lightPos)
		{
			var points = Polygon.BuildSymmetricalPolygon(CIRCLE_APPROXIMATION_VERTS, circle.Radius);
			var totalVerts = points.Length;
			for (var i = 0; i < totalVerts; i++)
			{
				var vertex = points[i] + circle.position;
				var nextVertex = points[(i + 1) % totalVerts] + circle.position;
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

					var point1 = vertex + (Vector2.Normalize(vertex - lightPos) * Screen.Width);
					var point2 = nextVertex + (Vector2.Normalize(nextVertex - lightPos) * Screen.Width);

					var poly = new Vector2[] { nextVertex, point2, point1, vertex };
					_primitiveBatch.DrawPolygon(poly, 4, Color.Black);
				}
			}
		}
	}
}
