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
		public bool AllowsMaterialRemoval = true;
		List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();

		public override void Initialize()
		{
			base.Initialize();

			_wantsIndentWhenDrawn = true;

			var material = GetValue<Material>();
			if (material == null)
				return;

			// fetch our inspectors and let them know who their parent is
			_inspectors = TypeInspectorUtils.GetInspectableProperties(material);

			// if we are a Material<T>, we need to fix the duplicate Effect due to the "new T effect"
			if (ReflectionUtils.IsGenericTypeOrSubclassOfGenericType(material.GetType()))
			{
				var didFindEffectInspector = false;
				for (var i = 0; i < _inspectors.Count; i++)
				{
					var isEffectInspector = _inspectors[i] is EffectInspector;
					if (isEffectInspector)
					{
						if (didFindEffectInspector)
						{
							_inspectors.RemoveAt(i);
							break;
						}

						didFindEffectInspector = true;
					}
				}
			}
		}

		public override void DrawMutable()
		{
			var isOpen = ImGui.CollapsingHeader($"{_name}", ImGuiTreeNodeFlags.FramePadding);

			if (GetValue() == null)
			{
				if (isOpen)
					DrawNullMaterial();
				return;
			}

			NezImGui.ShowContextMenuTooltip();

			if (ImGui.BeginPopupContextItem())
			{
				if (AllowsMaterialRemoval && ImGui.Selectable("Remove Material"))
				{
					SetValue(null);
					_inspectors.Clear();
					ImGui.CloseCurrentPopup();
				}

				if (ImGui.Selectable("Set Effect", false, ImGuiSelectableFlags.DontClosePopups))
					ImGui.OpenPopup("effect-chooser");

				ImGui.EndPopup();
			}

			if (isOpen)
			{
				ImGui.Indent();

				if (_inspectors.Count == 0)
				{
					if (NezImGui.CenteredButton("Set Effect", 0.6f))
						ImGui.OpenPopup("effect-chooser");
				}

				for (var i = _inspectors.Count - 1; i >= 0; i--)
				{
					if (_inspectors[i].IsTargetDestroyed)
					{
						_inspectors.RemoveAt(i);
						continue;
					}

					_inspectors[i].Draw();
				}

				ImGui.Unindent();
			}

			if (DrawEffectChooserPopup())
				ImGui.CloseCurrentPopup();
		}

		void DrawNullMaterial()
		{
			if (NezImGui.CenteredButton("Create Material", 0.5f, ImGui.GetStyle().IndentSpacing * 0.5f))
			{
				var material = new Material();
				SetValue(material);
				_inspectors = TypeInspectorUtils.GetInspectableProperties(material);
			}
		}

		bool DrawEffectChooserPopup()
		{
			var createdEffect = false;
			if (ImGui.BeginPopup("effect-chooser"))
			{
				foreach (var subclassType in InspectorCache.GetAllEffectSubclassTypes())
				{
					if (ImGui.Selectable(subclassType.Name))
					{
						// create the Effect, remove the existing EffectInspector and create a new one
						var effect = Activator.CreateInstance(subclassType) as Effect;
						var material = GetValue<Material>();
						material.Effect = effect;

						for (var i = _inspectors.Count - 1; i >= 0; i--)
						{
							if (_inspectors[i].GetType() == typeof(EffectInspector))
								_inspectors.RemoveAt(i);
						}

						var inspector = new EffectInspector();
						inspector.SetTarget(material, ReflectionUtils.GetFieldInfo(material, "Effect"));
						inspector.Initialize();
						_inspectors.Add(inspector);

						createdEffect = true;
					}
				}

				ImGui.EndPopup();
			}

			return createdEffect;
		}
	}
}