using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.UI;


#if DEBUG
namespace Nez
{
	/// <summary>
	/// container for a Component/PostProcessor/Transform and all of its inspectable properties
	/// </summary>
	public class InspectorList
	{
		public object target;
		public string name;

		List<Inspector> _inspectors;
		CheckBox _enabledCheckbox;


		public InspectorList( object target )
		{
			this.target = target;
			name = target.GetType().Name;
			_inspectors = Inspector.getInspectableProperties( target );
		}


		public InspectorList( Transform transform )
		{
			name = "Transform";
			_inspectors = Inspector.getTransformProperties( transform );
		}


		public void initialize( Table table, Skin skin, float leftCellWidth )
		{
			table.getRowDefaults().setPadTop( 10 );
			table.add( name.Replace( "PostProcessor", string.Empty ) ).getElement<Label>().setFontScale( 1f ).setFontColor( new Color( 241, 156, 0 ) );

			// if we have a component, stick a bool for enabled here
			if( target != null )
			{
				_enabledCheckbox = new CheckBox( string.Empty, skin );
				_enabledCheckbox.programmaticChangeEvents = false;

				if( target is Component )
					_enabledCheckbox.isChecked = ( (Component)target ).enabled;
				else if( target is PostProcessor )
					_enabledCheckbox.isChecked = ((PostProcessor)target ).enabled;
				
				_enabledCheckbox.onChanged += newValue =>
				{
					if( target is Component )
						((Component)target).enabled = newValue;
					else if( target is PostProcessor )
						( (PostProcessor)target ).enabled = newValue;
				};

				table.add( _enabledCheckbox ).right();
			}
			table.row();

			foreach( var i in _inspectors )
			{
				i.initialize( table, skin, leftCellWidth );
				table.row();
			}
		}


		public void update()
		{
			foreach( var i in _inspectors )
				i.update();
		}

	}
}
#endif