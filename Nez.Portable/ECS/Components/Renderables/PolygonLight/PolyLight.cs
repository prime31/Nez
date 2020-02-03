using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Nez.Shadows
{
	/// <summary>
	/// Point light that also casts shadows
	/// </summary>
	public class PolyLight : RenderableComponent
	{
		/// <summary>
		/// layer mask of all the layers this light should interact with. defaults to all layers.
		/// </summary>
		public int CollidesWithLayers = Physics.AllLayers;

		public override RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, new Vector2(_radius, _radius),
						Vector2.One, 0, _radius * 2f, _radius * 2f);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// Radius of influence of the light
		/// </summary>
		[Range(0, 2000)]
		public float Radius
		{
			get => _radius;
			set => SetRadius(value);
		}

		/// <summary>
		/// Power of the light, from 0 (turned off) to 1 for maximum brightness
		/// </summary>
		[Range(0, 50)] public float Power;

		protected float _radius;
		protected VisibilityComputer _visibility;

		PolygonLightEffect _lightEffect;
		FastList<short> _indices = new FastList<short>(50);
		FastList<VertexPositionTexture> _vertices = new FastList<VertexPositionTexture>(20);

		// shared Collider cache used for querying for nearby geometry. Maxes out at 10 Colliders.
		static protected Collider[] _colliderCache = new Collider[10];


		public PolyLight() : this(400)
		{
		}

		public PolyLight(float radius) : this(radius, Color.White)
		{
		}

		public PolyLight(float radius, Color color) : this(radius, color, 1.0f)
		{
		}

		public PolyLight(float radius, Color color, float power)
		{
			Radius = radius;
			Power = power;
			Color = color;
			ComputeTriangleIndices();
		}

		#region Fluent setters

		public virtual PolyLight SetRadius(float radius)
		{
			if (radius != _radius)
			{
				_radius = radius;
				_areBoundsDirty = true;

				if (_lightEffect != null)
					_lightEffect.LightRadius = radius;
			}

			return this;
		}

		public PolyLight SetPower(float power)
		{
			Power = power;
			return this;
		}

		#endregion


		/// <summary>
		/// fetches any Colliders that should be considered for occlusion. Subclasses with a shape other than a circle can override this.
		/// </summary>
		/// <returns>The overlapped components.</returns>
		protected virtual int GetOverlappedColliders()
		{
			return Physics.OverlapCircleAll(Entity.Position + _localOffset, _radius, _colliderCache, CollidesWithLayers);
		}

		/// <summary>
		/// override point for calling through to VisibilityComputer that allows subclasses to setup their visibility boundaries for
		/// different shaped lights.
		/// </summary>
		protected virtual void LoadVisibilityBoundaries() => _visibility.LoadRectangleBoundaries();

		#region Component and RenderableComponent

		public override void OnAddedToEntity()
		{
			_lightEffect = Entity.Scene.Content.LoadNezEffect<PolygonLightEffect>();
			_lightEffect.LightRadius = Radius;
			_visibility = new VisibilityComputer();
		}

		public override void Render(Batcher batcher, Camera camera) => RenderImpl(batcher, camera, false);

		public override void DebugRender(Batcher batcher)
		{
			// here, we just assume the Camera being used by the Renderer is the standard Scene Camera
			RenderImpl(batcher, Entity.Scene.Camera, true);

			// draw a square for our pivot/origin and draw our bounds
			batcher.DrawPixel(Entity.Transform.Position + _localOffset, Debug.Colors.RenderableCenter, 4);
			batcher.DrawHollowRect(Bounds, Debug.Colors.RenderableBounds);
		}

		void RenderImpl(Batcher batcher, Camera camera, bool debugDraw)
		{
			if (Power > 0 && IsVisibleFromCamera(camera))
			{
				var totalOverlaps = GetOverlappedColliders();

				// compute the visibility mesh
				_visibility.Begin(Entity.Transform.Position + _localOffset, _radius);
				LoadVisibilityBoundaries();
				for (var i = 0; i < totalOverlaps; i++)
				{
					if (!_colliderCache[i].IsTrigger)
						_visibility.AddColliderOccluder(_colliderCache[i]);
				}

				System.Array.Clear(_colliderCache, 0, totalOverlaps);

				// generate a triangle list from the encounter points
				var encounters = _visibility.End();
				GenerateVertsFromEncounters(encounters);
				ListPool<Vector2>.Free(encounters);

				var primitiveCount = _vertices.Length / 2;
				if (primitiveCount == 0)
					return;

				Core.GraphicsDevice.BlendState = BlendState.Additive;
				Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

				if (debugDraw)
				{
					var rasterizerState = new RasterizerState();
					rasterizerState.FillMode = FillMode.WireFrame;
					rasterizerState.CullMode = CullMode.None;
					Core.GraphicsDevice.RasterizerState = rasterizerState;
				}

				// Apply the effect
				_lightEffect.ViewProjectionMatrix = camera.ViewProjectionMatrix;
				_lightEffect.LightSource = Entity.Transform.Position;
				_lightEffect.LightColor = Color.ToVector3() * Power;
				_lightEffect.Techniques[0].Passes[0].Apply();

				Core.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertices.Buffer, 0,
					_vertices.Length, _indices.Buffer, 0, primitiveCount);
			}
		}

		#endregion


		/// <summary>
		/// adds a vert to the list
		/// </summary>
		/// <param name="position">Position.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AddVert(Vector2 position)
		{
			var index = _vertices.Length;
			_vertices.EnsureCapacity();
			_vertices.Buffer[index].Position = position.ToVector3();
			_vertices.Buffer[index].TextureCoordinate = position;
			_vertices.Length++;
		}

		void ComputeTriangleIndices(int totalTris = 20)
		{
			_indices.Reset();

			// compute the indices to form triangles
			for (var i = 0; i < totalTris; i += 2)
			{
				_indices.Add(0);
				_indices.Add((short) (i + 2));
				_indices.Add((short) (i + 1));
			}
		}

		void GenerateVertsFromEncounters(List<Vector2> encounters)
		{
			_vertices.Reset();

			// add a vertex for the center of the mesh
			AddVert(Entity.Transform.Position);

			// add all the other encounter points as vertices storing their world position as UV coordinates
			for (var i = 0; i < encounters.Count; i++)
				AddVert(encounters[i]);

			// if we dont have enough tri indices add enough for our encounter list
			var triIndices = _indices.Length / 3;
			if (encounters.Count > triIndices)
				ComputeTriangleIndices(encounters.Count);
		}
	}
}