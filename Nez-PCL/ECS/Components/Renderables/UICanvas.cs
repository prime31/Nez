using System;
using Nez.UI;


namespace Nez
{
	public class UICanvas : RenderableComponent, IUpdatable
	{
		public override float width { get { return stage.getWidth(); } }

		public override float height { get { return stage.getHeight(); } }

		public Stage stage;


		public UICanvas()
		{
			stage = new Stage();
		}


		public override void onAddedToEntity()
		{
			stage.entity = entity;
		}


		public override void onRemovedFromEntity()
		{
			stage.entity = null;
			stage.dispose();
		}


		void IUpdatable.update()
		{
			stage.update();
		}


		public override void render( Graphics graphics, Camera camera )
		{
			stage.render( graphics, camera );
		}


		public override void debugRender( Graphics graphics )
		{
			stage.getRoot().debugRender( graphics );
		}
	}
}

