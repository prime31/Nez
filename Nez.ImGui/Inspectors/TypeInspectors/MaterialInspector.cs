using System;
using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.IEnumerableExtensions;


namespace Nez.ImGuiTools.TypeInspectors
{
	public class MaterialInspector : AbstractTypeInspector
	{
		public bool allowsMaterialRemoval = true;
		List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

		public override void initialize()
		{
			base.initialize();

			_wantsIndentWhenDrawn = true;

			var material = getValue<Material>();
			if( material == null )
				return;

			// fetch our inspectors and let them know who their parent is
			_inspectors = TypeInspectorUtils.getInspectableProperties( material );

			// if we are a Material<T>, we need to fix the duplicate Effect due to the "new T effect"
			if( ReflectionUtils.isGenericTypeOrSubclassOfGenericType( material.GetType() ) )
			{
				var didFindEffectInspector = false;
				for( var i = 0; i < _inspectors.Count; i++ )
				{
					var isEffectInspector = _inspectors[i] is EffectInspector;
					if( isEffectInspector )
					{
						if( didFindEffectInspector )
						{
							_inspectors.RemoveAt( i );
							break;
						}
						didFindEffectInspector = true;
					}
				}
			}
		}

		public override void drawMutable()
		{
			var isOpen = ImGui.CollapsingHeader( $"{_name}", ImGuiTreeNodeFlags.FramePadding );

			if( getValue() == null )
			{
				if( isOpen )
					drawNullMaterial();
				return;
			}

			NezImGui.ShowContextMenuTooltip();

			if( ImGui.BeginPopupContextItem() )
			{
				if( allowsMaterialRemoval && ImGui.Selectable( "Remove Material" ) )
				{
					setValue( null );
					_inspectors.Clear();
					ImGui.CloseCurrentPopup();
				}

				if( ImGui.Selectable( "Set Effect", false, ImGuiSelectableFlags.DontClosePopups ) )
					ImGui.OpenPopup( "effect-chooser" );

				ImGui.EndPopup();
			}

			if( isOpen )
			{
				ImGui.Indent();

				if( _inspectors.Count == 0 )
				{
					if( NezImGui.CenteredButton( "Set Effect", 0.6f ) )
						ImGui.OpenPopup( "effect-chooser" );
				}

				for( var i = _inspectors.Count - 1; i >= 0; i-- )
				{
					if( _inspectors[i].isTargetDestroyed )
					{
						_inspectors.RemoveAt( i );
						continue;
					}
					_inspectors[i].draw();
				}
				ImGui.Unindent();
			}

			if( drawEffectChooserPopup() )
				ImGui.CloseCurrentPopup();
		}

		void drawNullMaterial()
		{
			if( NezImGui.CenteredButton( "Create Material", 0.5f, ImGui.GetStyle().IndentSpacing * 0.5f ) )
			{
				var material = new Material();
				setValue( material );
				_inspectors = TypeInspectorUtils.getInspectableProperties( material );
			}
		}

		bool drawEffectChooserPopup()
		{
			var createdEffect = false;
			if( ImGui.BeginPopup( "effect-chooser" ) )
			{
				foreach( var subclassType in InspectorCache.getAllEffectSubclassTypes() )
				{
					if( ImGui.Selectable( subclassType.Name ) )
					{
						// create the Effect, remove the existing EffectInspector and create a new one
						var effect = Activator.CreateInstance( subclassType ) as Effect;
						var material = getValue<Material>();
						material.effect = effect;

						for( var i = _inspectors.Count - 1; i >= 0; i-- )
						{
							if( _inspectors[i].GetType() == typeof( EffectInspector ) )
								_inspectors.RemoveAt( i );
						}

						var inspector = new EffectInspector();
						inspector.setTarget( material, ReflectionUtils.getFieldInfo( material, "effect" ) );
						inspector.initialize();
						_inspectors.Add( inspector );

						createdEffect = true;
					}
				}
				ImGui.EndPopup();
			}
			return createdEffect;
		}

	}
}
