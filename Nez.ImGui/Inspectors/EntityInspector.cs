using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Nez.ImGuiTools.ObjectInspectors;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
	public class EntityInspector
	{
		public Entity entity => _entity;

		Entity _entity;
		string _entityWindowId = "entity-" + NezImGui.GetScopeId().ToString();
		bool _shouldFocusWindow;
		string _componentNameFilter;
		TransformInspector _transformInspector;
		List<IComponentInspector> _componentInspectors = new List<IComponentInspector>();

        public EntityInspector( Entity entity )
		{
			_entity = entity;
			_transformInspector = new TransformInspector( _entity.transform );

			for( var i = 0; i < entity.components.count; i++ )
				_componentInspectors.Add( new ComponentInspector( entity.components[i] ) );
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

			// every 60 frames we check for newly added Components and add them
			if( Time.frameCount % 60 == 0 )
			{
				for( var i = 0; i < _entity.components.count; i++ )
				{
					var component = _entity.components[i];
					if( _componentInspectors.Where( inspector => inspector.component != null && inspector.component == component ).Count() == 0 )
						_componentInspectors.Insert( 0, new ComponentInspector( component ) );
				}
			}

			ImGui.SetNextWindowSize( new Num.Vector2( 335, 400 ), ImGuiCond.FirstUseEver );
			ImGui.SetNextWindowSizeConstraints( new Num.Vector2( 335, 200 ), new Num.Vector2( Screen.width, Screen.height ) );

			var open = true;
			if( ImGui.Begin( $"Entity Inspector: {entity.name}###" + _entityWindowId, ref open ) )
			{
                var enabled = entity.enabled;
                if( ImGui.Checkbox( "Enabled", ref enabled ) )
                    entity.enabled = enabled;
				
				ImGui.InputText( "Name", ref _entity.name, 25 );

                var updateInterval = (int)entity.updateInterval;
                if( ImGui.SliderInt( "Update Interval", ref updateInterval, 1, 100 ) )
                    entity.updateInterval = (uint)updateInterval;

                var tag = entity.tag;
				if( ImGui.InputInt( "Tag", ref tag ) )
                    entity.tag = tag;

				NezImGui.MediumVerticalSpace();
				_transformInspector.draw();
				NezImGui.MediumVerticalSpace();

				// watch out for removed Components
				for( var i = _componentInspectors.Count - 1; i >= 0; i-- )
				{
					if( _componentInspectors[i].entity == null )
					{
						_componentInspectors.RemoveAt( i );
						continue;
					}
					_componentInspectors[i].draw();
					NezImGui.MediumVerticalSpace();
				}

				if( NezImGui.CenteredButton( "Add Component", 0.6f ) )
				{
					_componentNameFilter = "";
					ImGui.OpenPopup( "component-selector" );
				}

				drawComponentSelectorPopup();

				ImGui.End();
			}

			if( !open )
				Core.getGlobalManager<ImGuiManager>().stopInspectingEntity( this );
		}

		private void drawComponentSelectorPopup()
		{
			if( ImGui.BeginPopup( "component-selector" ) )
			{
				ImGui.InputText( "###ComponentFilter", ref _componentNameFilter, 25 );
				ImGui.Separator();

				var isNezType = false;
				var isColliderType = false;
				foreach( var subclassType in InspectorCache.getAllComponentSubclassTypes() )
				{
					if( string.IsNullOrEmpty( _componentNameFilter ) || subclassType.Name.ToLower().Contains( _componentNameFilter.ToLower() ) )
					{
						// stick a seperator in after custom Components and before Colliders
						if( !isNezType && subclassType.Namespace.StartsWith( "Nez" ) )
						{
							isNezType = true;
							ImGui.Separator();
						}

						if( !isColliderType && typeof( Collider ).IsAssignableFrom( subclassType ) )
						{
							isColliderType = true;
							ImGui.Separator();
						}

						if( ImGui.Selectable( subclassType.Name ) )
						{
							entity.addComponent( Activator.CreateInstance( subclassType ) as Component );
							ImGui.CloseCurrentPopup();
						}
					}
				}
				ImGui.EndPopup();
			}
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