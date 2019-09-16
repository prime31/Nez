using Microsoft.Xna.Framework.Graphics;
using Nez;


namespace Nez3D
{
	/// <summary>
	/// Model3D represents a normal 3D Model but it is rendered by a standard Nez Camera on the same plane as all the 2D sprites. For this
	/// reason, the Model is inflated by a scale of 80 by default. This keeps the Model at approximately the same scale as 2D sprites. You
	/// can adjust this via the Vector3s present in this class (which replace the 2D Transform) and the 3D Camera fields (which are all
	/// suffixed with "3D").
	/// </summary>
	public class Model3D : Renderable3D
	{
		public override RectangleF Bounds
		{
			get
			{
				var sphere = _model.Meshes[0].BoundingSphere;
				var sizeX = sphere.Radius * 2 * Scale.X;
				var sizeY = sphere.Radius * 2 * Scale.Y;
				var x = (Position.X + sphere.Center.X - sizeX / 2);
				var y = (Position.Y + sphere.Center.Y - sizeY / 2);

				return new RectangleF(x, y, sizeX, sizeY);
			}
		}

		Model _model;


		public Model3D(Model model, Texture2D texture = null)
		{
			_model = model;

			if (texture != null)
			{
				foreach (var mesh in model.Meshes)
				{
					foreach (BasicEffect effect in mesh.Effects)
					{
						effect.TextureEnabled = true;
						effect.Texture = texture;
					}
				}
			}
		}

		public Model3D EnableDefaultLighting()
		{
			foreach (var mesh in _model.Meshes)
			foreach (BasicEffect effect in mesh.Effects)
				effect.EnableDefaultLighting();
			return this;
		}

		public override void Render(Batcher batcher, Camera camera)
		{
			// flush the 2D batch so we render appropriately depth-wise
			batcher.FlushBatch();

			Core.GraphicsDevice.BlendState = BlendState.Opaque;
			Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			for (var i = 0; i < _model.Meshes.Count; i++)
			{
				var mesh = _model.Meshes[i];
				for (var j = 0; j < mesh.Effects.Count; j++)
				{
					var effect = mesh.Effects[j] as BasicEffect;
					effect.World = WorldMatrix;
					effect.View = camera.ViewMatrix3D;
					effect.Projection = camera.ProjectionMatrix3D;
				}

				mesh.Draw();
			}
		}
	}
}