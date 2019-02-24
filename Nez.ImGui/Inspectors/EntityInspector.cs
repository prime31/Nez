using System.Collections.Generic;
using ImGuiNET;
using Nez.ImGuiTools.ComponentInspectors;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
	public class EntityInspector
	{
		public Entity entity => _entity;

		Entity _entity;
		bool _shouldFocusWindow;
		List<IComponentInspector> _componentInspectors = new List<IComponentInspector>();

        public EntityInspector( Entity entity )
		{
			_entity = entity;

			for( var i = 0; i < entity.components.count; i++ )
				_componentInspectors.Add( new ComponentInspector( entity.components[i] ) );
			
			_componentInspectors.Add( new TransformInspector( entity.transform ) );
		}

		public void draw()
		{
			// check to see if we are still alive
			if( entity.isDestroyed )
			{
				Core.getGlobalManager<ImGuiManager>().stopInspectingEntity( this );
				return;
			}

			if( _shouldFocusWindow )
			{
				_shouldFocusWindow = false;
				ImGui.SetNextWindowFocus();
				ImGui.SetNextWindowCollapsed( false );
			}

			ImGui.SetNextWindowSize( new Num.Vector2( 335, 400 ), ImGuiCond.FirstUseEver );
			ImGui.SetNextWindowSizeConstraints( new Num.Vector2( 335, 200 ), new Num.Vector2( Screen.width, Screen.height ) );

			var open = true;
			if( ImGui.Begin( $"Entity Inspector: {entity.name}", ref open ) )
			{
                var enabled = entity.enabled;
                if( ImGui.Checkbox( "Enabled", ref enabled ) )
                    entity.enabled = enabled;

                var updateInterval = (int)entity.updateInterval;
                if( ImGui.SliderInt( "Update Interval", ref updateInterval, 1, 100 ) )
                    entity.updateInterval = (uint)updateInterval;

                var tag = entity.tag;
				if( ImGui.InputInt( "Tag", ref tag ) )
                    entity.tag = tag;

				NezImGui.MediumVerticalSpace();

				for( var i = _componentInspectors.Count - 1; i >= 0; i-- )
				{
					_componentInspectors[i].draw();
					NezImGui.MediumVerticalSpace();
				}
				ImGui.End();
			}

			if( !open )
				Core.getGlobalManager<ImGuiManager>().stopInspectingEntity( this );
		}

		/// <summary>
		/// sets this EntityInspector to be focused the next time it is drawn
		/// </summary>
		public void setWindowFocus()
		{
			_shouldFocusWindow = true;
		}
	}
}