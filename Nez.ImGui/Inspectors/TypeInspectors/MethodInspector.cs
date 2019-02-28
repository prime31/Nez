using System;
using System.Reflection;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Num = System.Numerics;

namespace Nez.ImGuiTools.TypeInspectors
{
	public class MethodInspector : AbstractTypeInspector
	{
		static Type[] _allowedTypes = { typeof( int ), typeof( float ), typeof( string ), typeof( bool ), typeof( Vector2 ), typeof( Vector3 ) };
		Type _parameterType;
		string _parameterName;
		
		int _intParam;
		float _floatParam;
		string _stringParam = string.Empty;
		bool _boolParam;
		Num.Vector2 _vec2Param;
		Num.Vector3 _vec3Param;


		public static bool areParametersValid( ParameterInfo[] parameters )
		{
			if( parameters.Length == 0 )
				return true;

			if( parameters.Length > 1 )
			{
				Debug.warn( $"method {parameters[0].Member.Name} has InspectorCallableAttribute but it has more than 1 parameter" );
				return false;
			}

			var paramType = parameters[0].ParameterType;
			if( _allowedTypes.contains( paramType ) )
				return true;

			Debug.warn( $"method {parameters[0].Member.Name} has InspectorCallableAttribute but it has an invalid paraemter type {paramType}" );

			return false;
		}

		public override void initialize()
		{
			base.initialize();

			_scopeId = NezImGui.GetScopeId();

			// we could have zero or 1 param
			var parameters = ( _memberInfo as MethodInfo ).GetParameters();
			if( parameters.Length == 0 )
				return;

			var parameter = parameters[0];
			_parameterType = parameter.ParameterType;
			_parameterName = parameter.Name;
		}

		public override void drawMutable()
		{
			if( ImGui.Button( _name ) )
				onButtonClicked();
			
			if( _parameterType != null )
				ImGui.SameLine();
			
			ImGui.PushItemWidth( -ImGui.CalcTextSize( _parameterName ).X ); // spacing on the right for the label
			if( _parameterType == typeof( float ) )
				ImGui.DragFloat( $"{_parameterName}##", ref _floatParam );
			else if( _parameterType == typeof( int ) )
				ImGui.DragInt( $"{_parameterName}##", ref _intParam );
			else if( _parameterType == typeof( bool ) )
				ImGui.Checkbox( $"{_parameterName}##", ref _boolParam );
			else if( _parameterType == typeof( string ) )
				ImGui.InputText( $"{_parameterName}##", ref _stringParam, 100 );
			else if( _parameterType == typeof( Vector2 ) )
				ImGui.DragFloat2( $"{_parameterName}##", ref _vec2Param );
			else if( _parameterType == typeof( Vector3 ) )
				ImGui.DragFloat3( $"{_parameterName}##", ref _vec3Param );
			ImGui.PopItemWidth();

			handleTooltip();
		}

		void onButtonClicked()
		{
			if( _parameterType == null )
			{
			    ( _memberInfo as MethodInfo ).Invoke( _target, new object[] { } );
			}
			else
			{
			    // extract the param and properly cast it
			    var parameters = new object[1];
			
			    try
			    {
			        if( _parameterType == typeof( float ) )
			            parameters[0] = _floatParam;
			        else if( _parameterType == typeof( int ) )
			            parameters[0] = _intParam;
			        else if( _parameterType == typeof( bool ) )
			            parameters[0] = _boolParam;
			        else if( _parameterType == typeof( string ) )
			            parameters[0] = _stringParam;
					else if( _parameterType == typeof( Vector2 ) )
						parameters[0] = _vec2Param.toXNA();
					else if( _parameterType == typeof( Vector3 ) )
						parameters[0] = _vec3Param.toXNA();
			
			        ( _memberInfo as MethodInfo ).Invoke( _target, parameters );
			    }
			    catch( Exception e )
			    {
			        Debug.error( e.ToString() );
			    }
			}
		}

	}
}

