using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.UI;


#if DEBUG
namespace Nez
{
	/// <summary>
	/// container for a Component and all of its inspectable properties. Does double duty by housing Transform.
	/// </summary>
	public class ComponentInspector
	{
		public Component component;
		public string name;

		List<Inspector> _inspectors;
		CheckBox _enabledCheckbox;


		public ComponentInspector( Component component )
		{
			this.component = component;
			name = component.GetType().Name;
			_inspectors = Inspector.getInspectableProperties( component );
		}


		public ComponentInspector( Transform transform )
		{
			name = "Transform";
			_inspectors = Inspector.getTransformProperties( transform );
		}


		public void update()
		{
			foreach( var i in _inspectors )
				i.update();
		}


		public void initialize( Table table, Skin skin )
		{
			table.add( name ).getElement<Label>().setFontScale( 1.4f ).setFontColor( new Color( 241, 156, 0 ) );

			// if we have a component, stick a bool for enabled here
			if( component != null )
			{
				var enabledLabel = new Label( "Enabled", skin );

				_enabledCheckbox = new CheckBox( string.Empty, skin );
				_enabledCheckbox.programmaticChangeEvents = false;
				_enabledCheckbox.isChecked = component.enabled;
				_enabledCheckbox.onChanged += newValue =>
				{
					component.enabled = newValue;
				};

				var hBox = new HorizontalGroup( 5 );
				hBox.addElement( enabledLabel );
				hBox.addElement( _enabledCheckbox );
				table.add( hBox ).right();
			}

			table.row();

			foreach( var i in _inspectors )
			{
				i.initialize( table, skin );
				table.row();
			}
		}

	}
}
#endif