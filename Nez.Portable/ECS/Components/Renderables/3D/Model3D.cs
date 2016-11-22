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
		public override RectangleF bounds
		{
			get
			{
				var sphere = _model.Meshes[0].BoundingSphere;
				var sizeX = sphere.Radius * 2 * scale.X;
				var sizeY = sphere.Radius * 2 * scale.Y;
				var x = ( position.X + sphere.Center.X - sizeX / 2 );
				var y = ( position.Y + sphere.Center.Y - sizeY / 2 );

				return new RectangleF( x, y, sizeX, sizeY );
			}
		}
		
		Model _model;


		public Model3D( Model model, Texture2D texture = null )
		{
			_model = model;

			if( texture != null )
			{
				foreach( var mesh in model.Meshes )
				{
					foreach( BasicEffect effect in mesh.Effects )
					{
						effect.TextureEnabled = true;
						effect.Texture = texture;
					}
				}
			}
		}


		public Model3D enableDefaultLighting()
		{
			foreach( var mesh in _model.Meshes )
				foreach( BasicEffect effect in mesh.Effects )
					effect.EnableDefaultLighting();
			return this;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			// flush the 2D batch so we render appropriately depth-wise
			graphics.batcher.flushBatch();

			Core.graphicsDevice.BlendState = BlendState.Opaque;
			Core.graphicsDevice.DepthStencilState = DepthStencilState.Default;

			for( var i = 0; i < _model.Meshes.Count; i++ )
			{
				var mesh = _model.Meshes[i];
				for( var j = 0; j < mesh.Effects.Count; j++ )
				{
					var effect = mesh.Effects[j] as BasicEffect;
					effect.World = worldMatrix;
					effect.View = camera.viewMatrix3D;
					effect.Projection = camera.projectionMatrix3D;
				}
				mesh.Draw();
			}
		}

	}
}
