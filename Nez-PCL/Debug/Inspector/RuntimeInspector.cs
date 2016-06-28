using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.UI;


#if DEBUG
namespace Nez
{
	public class RuntimeInspector : IDisposable
	{
		UICanvas ui;
		ScreenSpaceCamera _camera;
		List<ComponentInspector> _components = new List<ComponentInspector>();

		// ui fields
		Skin _skin;
		ScrollPane _scrollPane;
		Table _table;
		Entity entity;


		public RuntimeInspector( Entity entity )
		{
			this.entity = entity;
			prepCanvas();
			cacheTransformInspector();
			_camera = new ScreenSpaceCamera();
			Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
		}


		void onGraphicsDeviceReset()
		{
			_scrollPane.setHeight( Screen.height );
		}


		public void render()
		{
			// update transform
			getOrCreateComponentInspector( null ).update();

			foreach( var comp in entity.components )
			{
				var inspector = getOrCreateComponentInspector( comp );
				inspector.update();
			}

			Graphics.instance.batcher.begin();
			( ui as IUpdatable ).update();
			ui.render( Graphics.instance, _camera );
			Graphics.instance.batcher.end();
		}


		ComponentInspector getOrCreateComponentInspector( Component comp )
		{
			var inspector = _components.Where( i => i.component == comp ).FirstOrDefault();
			if( inspector == null )
			{
				inspector = new ComponentInspector( comp );
				inspector.initialize( _table, _skin );
				_components.Add( inspector );
			}

			return inspector;
		}


		void cacheTransformInspector()
		{
			// add Transform separately
			var transformInspector = new ComponentInspector( entity.transform );
			transformInspector.initialize( _table, _skin );
			_components.Add( transformInspector );
		}


		void prepCanvas()
		{
			_skin = Skin.createDefaultSkin();

			// modify some of the default styles to better suit our needs
			var tfs = _skin.get<TextFieldStyle>();
			tfs.background.leftWidth = tfs.background.rightWidth = 4;
			tfs.background.bottomHeight = 0;
			tfs.background.topHeight = 3;

			var checkbox = _skin.get<CheckBoxStyle>();
			checkbox.checkboxOn.minWidth = checkbox.checkboxOn.minHeight = 15;
			checkbox.checkboxOff.minWidth = checkbox.checkboxOff.minHeight = 15;
			checkbox.checkboxOver.minWidth = checkbox.checkboxOver.minHeight = 15;

			// since we arent using this as a Component on an Entity we'll fake it here
			ui = new UICanvas();
			ui.entity = entity;
			ui.onAddedToEntity();
			ui.stage.isFullScreen = true;

			_table = new Table();
			_table.top().left();
			_table.defaults().setPadTop( 5 ).setPadLeft( 5 ).setPadRight( 5 ).setAlign( Align.left );
			_table.setBackground( new PrimitiveDrawable( new Color( 40, 40, 40 ) ) );

			// wrap up the table in a ScrollPane
			_scrollPane = ui.stage.addElement( new ScrollPane( _table, _skin ) );
			// force a validate which will layout the ScrollPane and populate the proper scrollBarWidth
			_scrollPane.validate();
			_scrollPane.setSize( 295 + _scrollPane.getScrollBarWidth(), Screen.height );
		}


		#region IDisposable Support

		bool _disposedValue = false;

		void Dispose( bool disposing )
		{
			if( !_disposedValue )
			{
				Core.emitter.removeObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
				_disposedValue = true;
			}
		}


		public void Dispose()
		{
			Dispose( true );
		}

		#endregion

	}
}
#endif