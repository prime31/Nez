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