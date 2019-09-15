using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSPolygonBody : FSRenderableBody
	{
		/// <summary>
		/// verts are stored in display units. We convert to sim units if the Transform.scale changes.
		/// </summary>
		protected Vertices _verts;


		public FSPolygonBody(Sprite sprite, List<Vector2> verts) : base(sprite)
		{
			_verts = new Vertices(verts);
		}

		public override void Initialize()
		{
			base.Initialize();
			Body.AttachPolygon(_verts, 1);
		}

		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			base.OnEntityTransformChanged(comp);
			if (_ignoreTransformChanges)
				return;

			// we only care about scale. base handles pos/rot
			if (comp == Transform.Component.Scale)
			{
				// fetch the Vertices, clear them, add our originals and scale them
				var poly = Body.FixtureList[0].Shape as PolygonShape;
				var verts = poly.Vertices;
				verts.Clear();
				verts.AddRange(_verts);
				verts.Scale(Transform.Scale);
				poly.SetVerticesNoCopy(verts);

				// wake the body if it is asleep to update collisions
				if (!Body.IsAwake)
					Body.IsAwake = true;
			}
		}
	}
}