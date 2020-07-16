using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.ImGuiTools.TypeInspectors
{
	/// <summary>
	/// this is a bit of a hack. The class implements properties that just forward the calls to the BlendState
	/// that we are inspecting.
	/// </summary>
	public class BlendStateInspector : AbstractTypeInspector
	{
		List<AbstractTypeInspector> _inspectors = new List<AbstractTypeInspector>();
		BlendState _blendState;

		BlendFunction AlphaBlendFunction
		{
			get => _blendState.AlphaBlendFunction;
			set => _blendState.AlphaBlendFunction = value;
		}

		Blend AlphaDestinationBlend
		{
			get => _blendState.AlphaDestinationBlend;
			set => _blendState.AlphaDestinationBlend = value;
		}

		Blend AlphaSourceBlend
		{
			get => _blendState.AlphaSourceBlend;
			set => _blendState.AlphaSourceBlend = value;
		}

		BlendFunction ColorBlendFunction
		{
			get => _blendState.ColorBlendFunction;
			set => _blendState.ColorBlendFunction = value;
		}

		Blend ColorDestinationBlend
		{
			get => _blendState.ColorDestinationBlend;
			set => _blendState.ColorDestinationBlend = value;
		}

		Blend ColorSourceBlend
		{
			get => _blendState.ColorSourceBlend;
			set => _blendState.ColorSourceBlend = value;
		}

		Color BlendFactor
		{
			get => _blendState.BlendFactor;
			set => _blendState.BlendFactor = value;
		}

		public override void Initialize()
		{
			// we have to clone the BlendState since it is often set from one of the static BlendState ivars
			var tmpBlendState = GetValue<BlendState>();
			_blendState = new BlendState
			{
				AlphaBlendFunction = tmpBlendState.AlphaBlendFunction,
				AlphaDestinationBlend = tmpBlendState.AlphaDestinationBlend,
				AlphaSourceBlend = tmpBlendState.AlphaSourceBlend,
				ColorBlendFunction = tmpBlendState.ColorBlendFunction,
				ColorDestinationBlend = tmpBlendState.ColorDestinationBlend,
				ColorSourceBlend = tmpBlendState.ColorSourceBlend,
				ColorWriteChannels = tmpBlendState.ColorWriteChannels,
				ColorWriteChannels1 = tmpBlendState.ColorWriteChannels1,
				ColorWriteChannels2 = tmpBlendState.ColorWriteChannels2,
				ColorWriteChannels3 = tmpBlendState.ColorWriteChannels3,
				BlendFactor = tmpBlendState.BlendFactor,
				MultiSampleMask = tmpBlendState.MultiSampleMask
			};
			SetValue(_blendState);

			var props = GetType().GetRuntimeProperties();

			AbstractTypeInspector inspector = new EnumInspector();
			inspector.SetTarget(this, props.Where(p => p.Name == "AlphaBlendFunction").First());
			inspector.Initialize();
			_inspectors.Add(inspector);

			inspector = new EnumInspector();
			inspector.SetTarget(this, props.Where(p => p.Name == "AlphaDestinationBlend").First());
			inspector.Initialize();
			_inspectors.Add(inspector);

			inspector = new EnumInspector();
			inspector.SetTarget(this, props.Where(p => p.Name == "AlphaSourceBlend").First());
			inspector.Initialize();
			_inspectors.Add(inspector);

			inspector = new EnumInspector();
			inspector.SetTarget(this, props.Where(p => p.Name == "ColorBlendFunction").First());
			inspector.Initialize();
			_inspectors.Add(inspector);

			inspector = new EnumInspector();
			inspector.SetTarget(this, props.Where(p => p.Name == "ColorDestinationBlend").First());
			inspector.Initialize();
			_inspectors.Add(inspector);

			inspector = new EnumInspector();
			inspector.SetTarget(this, props.Where(p => p.Name == "ColorSourceBlend").First());
			inspector.Initialize();
			_inspectors.Add(inspector);

			inspector = new SimpleTypeInspector();
			inspector.SetTarget(this, props.Where(p => p.Name == "BlendFactor").First());
			inspector.Initialize();
			_inspectors.Add(inspector);
		}

		public override void DrawMutable()
		{
			if (ImGui.CollapsingHeader(_name))
			{
				// this is the amount of space the labels on the right require. The rest goes to the widgets
				ImGui.PushItemWidth(-125);
				foreach (var i in _inspectors)
					i.Draw();
				ImGui.PopItemWidth();
			}
		}
	}
}