using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.ImGuiTools.TypeInspectors
{
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
			set => _blendState.AlphaSourceBlend = value;
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

		public override void initialize()
		{
			_blendState = getValue<BlendState>();
			var props = GetType().GetRuntimeProperties();

			AbstractTypeInspector inspector = new EnumInspector();
			inspector.setTarget( this, props.Where( p => p.Name == "AlphaBlendFunction" ).First() );
			inspector.initialize();
			_inspectors.Add( inspector );

			inspector = new EnumInspector();
			inspector.setTarget( this, props.Where( p => p.Name == "AlphaDestinationBlend" ).First() );
			inspector.initialize();
			_inspectors.Add( inspector );

			inspector = new EnumInspector();
			inspector.setTarget( this, props.Where( p => p.Name == "AlphaSourceBlend" ).First() );
			inspector.initialize();
			_inspectors.Add( inspector );

			inspector = new EnumInspector();
			inspector.setTarget( this, props.Where( p => p.Name == "ColorBlendFunction" ).First() );
			inspector.initialize();
			_inspectors.Add( inspector );

			inspector = new EnumInspector();
			inspector.setTarget( this, props.Where( p => p.Name == "ColorDestinationBlend" ).First() );
			inspector.initialize();
			_inspectors.Add( inspector );

			inspector = new EnumInspector();
			inspector.setTarget( this, props.Where( p => p.Name == "ColorSourceBlend" ).First() );
			inspector.initialize();
			_inspectors.Add( inspector );

			inspector = new ColorInspector();
			inspector.setTarget( this, props.Where( p => p.Name == "BlendFactor" ).First() );
			inspector.initialize();
			_inspectors.Add( inspector );
		}

		public override void draw()
		{
			if( ImGui.CollapsingHeader( _name ) )
			{
				ImGui.Indent();
				//NezImGui.beginBorderedGroup();
				ImGui.Text( _name );
				foreach( var i in _inspectors )
					i.draw();
				//NezImGui.endBorderedGroup();
				ImGui.Unindent();
			}
		}
	}
}
