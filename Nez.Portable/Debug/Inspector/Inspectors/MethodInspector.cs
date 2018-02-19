using System;
using System.Reflection;
using Nez.UI;


#if DEBUG
namespace Nez
{
	public class MethodInspector : Inspector
	{
		// the TextField for our parameter if we have one
		TextField _textField;
		Type _parameterType;


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
			if( paramType == typeof( int ) || paramType == typeof( float ) || paramType == typeof( string ) || paramType == typeof( bool ) )
				return true;

			Debug.warn( $"method {parameters[0].Member.Name} has InspectorCallableAttribute but it has an invalid paraemter type {paramType}" );

			return false;
		}


		public override void initialize( Table table, Skin skin, float leftCellWidth )
		{
			var button = new TextButton( _name, skin );
			button.onClicked += onButtonClicked;

			// we could have zero or 1 param
			var parameters = ( _memberInfo as MethodInfo ).GetParameters();
			if( parameters.Length == 0 )
			{
				table.add( button );
				return;
			}

			var parameter = parameters[0];
			_parameterType = parameter.ParameterType;

			_textField = new TextField( _parameterType.GetTypeInfo().IsValueType ? Activator.CreateInstance( _parameterType ).ToString() : "", skin );
			_textField.shouldIgnoreTextUpdatesWhileFocused = false;

			// add a filter for float/int
			if( _parameterType == typeof( float ) )
				_textField.setTextFieldFilter( new FloatFilter() );
			if( _parameterType == typeof( int ) )
				_textField.setTextFieldFilter( new DigitsOnlyFilter() );
			if( _parameterType == typeof( bool ) )
				_textField.setTextFieldFilter( new BoolFilter() );

			table.add( button );
			table.add( _textField ).setMaxWidth( 70 );
		}


		public override void update()
		{}


		void onButtonClicked( Button button )
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
						parameters[0] = float.Parse( _textField.getText() );
					else if( _parameterType == typeof( int ) )
						parameters[0] = int.Parse( _textField.getText() );
					else if( _parameterType == typeof( bool ) )
						parameters[0] = bool.Parse( _textField.getText() );
					else
						parameters[0] = _textField.getText();

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
#endif
