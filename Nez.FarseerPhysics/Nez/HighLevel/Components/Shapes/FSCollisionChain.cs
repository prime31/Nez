using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSCollisionChain : FSCollisionShape
	{
		List<Vector2> _verts;
		bool _loop;


		public FSCollisionChain()
		{
			_fixtureDef.Shape = new ChainShape();
		}


		public FSCollisionChain(List<Vector2> verts) : this()
		{
			_verts = verts;
		}


		public FSCollisionChain(Vector2[] verts) : this()
		{
			_verts = new List<Vector2>(verts);
		}


		#region Configuration

		public FSCollisionChain SetLoop(bool loop)
		{
			_loop = loop;
			RecreateFixture();
			return this;
		}


		public FSCollisionChain SetVertices(Vertices verts)
		{
			_verts = verts;
			RecreateFixture();
			return this;
		}


		public FSCollisionChain SetVertices(List<Vector2> verts)
		{
			_verts = verts;
			RecreateFixture();
			return this;
		}


		public FSCollisionChain SetVertices(Vector2[] verts)
		{
			if (_verts == null)
				_verts = new List<Vector2>();

			_verts.Clear();
			_verts.AddRange(verts);
			RecreateFixture();
			return this;
		}

		#endregion


		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			if (comp == Transform.Component.Scale)
				RecreateFixture();
		}


		void RecreateFixture()
		{
			Insist.IsNotNull(_verts, "verts cannot be null!");

			DestroyFixture();

			// scale our verts and convert them to sim units
			var verts = new Vertices(_verts);
			verts.Scale(Transform.Scale * FSConvert.DisplayToSim);

			var chainShape = _fixtureDef.Shape as ChainShape;
			chainShape.SetVertices(verts, _loop);

			CreateFixture();
		}
	}
}