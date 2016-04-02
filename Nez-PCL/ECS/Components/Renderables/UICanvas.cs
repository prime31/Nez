using System;
using Nez.UI;
using Microsoft.Xna.Framework;


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


		/// <summary>
		/// displays a simple dialog with a button to close it
		/// </summary>
		/// <returns>The dialog.</returns>
		/// <param name="title">Title.</param>
		/// <param name="messageText">Message text.</param>
		/// <param name="closeButtonText">Close button text.</param>
		public Dialog showDialog( string title, string messageText, string closeButtonText )
		{
			var skin = Skin.createDefaultSkin();

			var style = new WindowStyle {
				background = new PrimitiveDrawable( new Color( 50, 50, 50 ) ),
				stageBackground = new PrimitiveDrawable( new Color( 0, 0, 0, 150 ) )
			};

			var dialog = new Dialog( title, style );
			dialog.getTitleLabel().getStyle().background = new PrimitiveDrawable( new Color( 55, 100, 100 ) );
			dialog.pad( 20, 5, 5, 5 );
			dialog.addText( messageText );
			dialog.addButton( new TextButton( closeButtonText, skin ) ).onClicked += butt => dialog.hide();
			dialog.show( stage );

			return dialog;
		}
	}
}

