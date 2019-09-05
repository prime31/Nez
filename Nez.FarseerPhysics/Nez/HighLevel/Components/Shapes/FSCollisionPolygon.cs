using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSCollisionPolygon : FSCollisionShape
	{
		/// <summary>
		/// verts are stored in sim units
		/// </summary>
		protected Vertices _verts;

		Vector2 _center;
		protected bool _areVertsDirty = true;


		public FSCollisionPolygon()
		{
			_fixtureDef.Shape = new PolygonShape();
		}


		public FSCollisionPolygon(List<Vector2> vertices) : this()
		{
			_verts = new Vertices(vertices);
			_verts.Scale(new Vector2(FSConvert.DisplayToSim));
		}


		public FSCollisionPolygon(Vector2[] vertices) : this()
		{
			_verts = new Vertices(vertices);
			_verts.Scale(new Vector2(FSConvert.DisplayToSim));
		}


		#region Configuration

		public FSCollisionPolygon SetVertices(Vertices vertices)
		{
			_verts = new Vertices(vertices);
			_areVertsDirty = true;
			RecreateFixture();
			return this;
		}


		public FSCollisionPolygon SetVertices(List<Vector2> vertices)
		{
			_verts = new Vertices(vertices);
			_areVertsDirty = true;
			RecreateFixture();
			return this;
		}


		public FSCollisionPolygon SetCenter(Vector2 center)
		{
			_center = center;
			_areVertsDirty = true;
			RecreateFixture();
			return this;
		}

		#endregion


		public override void OnAddedToEntity()
		{
			UpdateVerts();
			CreateFixture();
		}


		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			if (comp == Transform.Component.Scale)
				RecreateFixture();
		}


		internal override void CreateFixture()
		{
			UpdateVerts();
			base.CreateFixture();
		}


		protected void RecreateFixture()
		{
			DestroyFixture();
			UpdateVerts();
			CreateFixture();
		}


		protected void UpdateVerts()
		{
			Insist.IsNotNull(_verts, "verts cannot be null!");

			if (!_areVertsDirty)
				return;

			_areVertsDirty = false;

			var shapeVerts = (_fixtureDef.Shape as PolygonShape).Vertices;
			shapeVerts.attachedToBody = false;

			shapeVerts.Clear();
			shapeVerts.AddRange(_verts);
			shapeVerts.Scale(Transform.Scale);
			shapeVerts.Translate(ref _center);

			(_fixtureDef.Shape as PolygonShape).SetVerticesNoCopy(shapeVerts);
		}
	}
}