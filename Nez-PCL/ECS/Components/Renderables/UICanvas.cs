using System;
using Nez.UI;


namespace Nez
{
	/// <summary>
	/// simple component that houses a Stage and delegates update/render/debugRender calls
	/// </summary>
	public class UICanvas : RenderableComponent, IUpdatable
	{
		public override float width { get { return stage.getWidth(); } }

		public override float height { get { return stage.getHeight(); } }

		public Stage stage;

		/// <summary>
		/// if true, the rawMousePosition will be used else the scaledMousePosition will be used. If your UI is in screen space (using a 
		/// ScreenSpaceRenderer for example) then set this to true so input is not scaled.
		/// </summary>
		public bool isFullScreen
		{
			get { return stage.isFullScreen; }
			set { stage.isFullScreen = value; }
		}


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

