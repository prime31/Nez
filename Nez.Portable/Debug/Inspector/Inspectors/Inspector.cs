using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.IEnumerableExtensions;
using Nez.UI;


#if DEBUG
namespace Nez
{
	/// <summary>
	/// the heart of the inspector system. Subclasses of Inspector are responsible for setting up and managing the UI. Currently,
	/// custom type handling is not yet implemented.
	/// </summary>
	public abstract class Inspector
	{
		protected object _target;
		protected string _name;
		protected Type _valueType;
		protected Func<object> _getter;
		protected Action<object> _setter;
		protected MemberInfo _memberInfo;


		public static List<Inspector> getInspectableProperties( object target )
		{
			var props = new List<Inspector>();
			var targetType = target.GetType();

			var fields = ReflectionUtils.getFields( targetType );
			foreach( var field in fields )
			{
				if( !field.IsPublic && IEnumerableExt.count( field.GetCustomAttributes<InspectableAttribute>() ) == 0 )
					continue;

				if( field.IsInitOnly )
					continue;

				// skip enabled which is handled elsewhere
				if( field.Name == "enabled" )
					continue;

				var inspector = getInspectorForType( field.FieldType, target, field );
				if( inspector != null )
				{
					inspector.setTarget( target, field );
					props.Add( inspector );
				}
			}

			var properties = ReflectionUtils.getProperties( targetType );
			foreach( var prop in properties )
			{
				if( !prop.CanRead || !prop.CanWrite )
					continue;

				if( ( !prop.GetMethod.IsPublic || !prop.SetMethod.IsPublic ) && IEnumerableExt.count( prop.GetCustomAttributes<InspectableAttribute>() ) == 0 )
					continue;

				// skip Component.enabled which is handled elsewhere
				if( prop.Name == "enabled" )
					continue;

				var inspector = getInspectorForType( prop.PropertyType, target, prop );
				if( inspector != null )
				{
					inspector.setTarget( target, prop );
					props.Add( inspector );
				}
			}

			var methods = ReflectionUtils.getMethods( targetType );
			foreach( var method in methods )
			{
				var attr = method.GetCustomAttribute<InspectorCallableAttribute>();
				if( attr == null )
					continue;

				if( !MethodInspector.areParametersValid( method.GetParameters() ) )
					continue;

				var inspector = new MethodInspector();
				inspector.setTarget( target, method );
				props.Add( inspector );
			}

			return props;
		}


		public static List<Inspector> getTransformProperties( object transform )
		{
			var props = new List<Inspector>();
			var type = transform.GetType();

			var allowedProps = new string[] { "localPosition", "localRotationDegrees", "localScale" };
			var properties = ReflectionUtils.getProperties( type );
			foreach( var prop in properties )
			{
				if( !allowedProps.contains( prop.Name ) )
					continue;

				var inspector = getInspectorForType( prop.PropertyType, transform, prop );
				inspector.setTarget( transform, prop );
				props.Add( inspector );
			}

			return props;
		}


		/// <summary>
		/// gets an Inspector subclass that can handle valueType. If no default Inspector is available the memberInfo custom attributes
		/// will be checked for the CustomInspectorAttribute.
		/// </summary>
		/// <returns>The inspector for type.</returns>
		/// <param name="valueType">Value type.</param>
		/// <param name="memberInfo">Member info.</param>
		protected static Inspector getInspectorForType( Type valueType, object target, MemberInfo memberInfo )
		{
			// built-in types
			if( valueType == typeof( int ) )
				return new IntInspector();
			if( valueType == typeof( float ) )
				return new FloatInspector();
			if( valueType == typeof( bool ) )
				return new BoolInspector();
			if( valueType == typeof( string ) )
				return new StringInspector();
			if( valueType == typeof( Vector2 ) )
				return new Vector2Inspector();
			if( valueType == typeof( Color ) )
				return new ColorInspector();
			if( valueType.GetTypeInfo().IsEnum )
				return new EnumInspector();
			if( valueType.GetTypeInfo().IsValueType )
				return new StructInspector();

			// check for custom inspectors before checking Nez types in case a subclass implemented one
			var customInspectorType = valueType.GetTypeInfo().GetCustomAttribute<CustomInspectorAttribute>();
			if( customInspectorType != null )
			{
				if( customInspectorType.inspectorType.GetTypeInfo().IsSubclassOf( typeof( Inspector ) ) )
					return (Inspector)Activator.CreateInstance( customInspectorType.inspectorType );
				Debug.warn( $"found CustomInspector {customInspectorType.inspectorType} but it is not a subclass of Inspector" );
			}

			// Nez types
			if( valueType == typeof( Material ) )
				return getMaterialInspector( target );
			if( valueType.GetTypeInfo().IsSubclassOf( typeof( Effect ) ) )
				return getEffectInspector( target, memberInfo );

			//Debug.log( $"no inspector for type {valueType}" );

			return null;
		}


		/// <summary>
		/// null checks the Material and Material.effect and ony returns an Inspector if we have data
		/// </summary>
		/// <returns>The material inspector.</returns>
		/// <param name="target">Target.</param>
		static Inspector getMaterialInspector( object target )
		{
			var materialProp = ReflectionUtils.getPropertyInfo( target, "material" );
			var materialMethod = ReflectionUtils.getPropertyGetter( materialProp );
			var material = materialMethod.Invoke( target, new object[] { } ) as Material;
			if( material == null || material.effect == null )
				return null;

			// we only want subclasses of Effect. Effect itself is not interesting
			if( material.effect.GetType().GetTypeInfo().IsSubclassOf( typeof( Effect ) ) )
				return new EffectInspector();

			return null;
		}


		/// <summary>
		/// null checks the Effect and creates an Inspector only if it is not null
		/// </summary>
		/// <returns>The effect inspector.</returns>
		/// <param name="target">Target.</param>
		/// <param name="memberInfo">Member info.</param>
		static Inspector getEffectInspector( object target, MemberInfo memberInfo )
		{
			var fieldInfo = memberInfo as FieldInfo;
			if( fieldInfo != null )
			{
				if( fieldInfo.GetValue( target ) != null )
					return new EffectInspector();
			}

			var propInfo = memberInfo as PropertyInfo;
			if( propInfo != null )
			{
				var getter = ReflectionUtils.getPropertyGetter( propInfo );
				if( getter.Invoke( target, new object[] {} ) != null )
					return new EffectInspector();
			}

			return null;
		}


		public void setTarget( object target, FieldInfo field )
		{
			_target = target;
			_memberInfo = field;
			_name = field.Name;
			_valueType = field.FieldType;

			_getter = () =>
			{
				return field.GetValue( target );
			};
			_setter = ( val ) =>
			{
				field.SetValue( target, val );
			};
		}


		/// <summary>
		/// this version will first fetch the struct before getting/setting values on it when invoking the getter/setter
		/// </summary>
		/// <returns>The struct target.</returns>
		/// <param name="target">Target.</param>
		/// <param name="structName">Struct name.</param>
		/// <param name="field">Field.</param>
		public void setStructTarget( object target, Inspector parentInspector, FieldInfo field )
		{
			_target = target;
			_memberInfo = field;
			_name = field.Name;
			_valueType = field.FieldType;

			_getter = () =>
			{
				var structValue = parentInspector.getValue();
				return field.GetValue( structValue );
			};
			_setter = ( val ) =>
			{
				var structValue = parentInspector.getValue();
				field.SetValue( structValue, val );
				parentInspector.setValue( structValue );
			};
		}


		public void setTarget( object target, PropertyInfo prop )
		{
			_memberInfo = prop;
			_target = target;
			_name = prop.Name;
			_valueType = prop.PropertyType;

			_getter = () =>
			{
				return ReflectionUtils.getPropertyGetter( prop ).Invoke( target, null );
			};
			_setter = ( val ) =>
			{
				ReflectionUtils.getPropertySetter( prop ).Invoke( target, new object[] { val } );
			};
		}


		/// <summary>
		/// this version will first fetch the struct before getting/setting values on it when invoking the getter/setter
		/// </summary>
		/// <returns>The struct target.</returns>
		/// <param name="target">Target.</param>
		/// <param name="structName">Struct name.</param>
		/// <param name="field">Field.</param>
		public void setStructTarget( object target, Inspector parentInspector, PropertyInfo prop )
		{
			_target = target;
			_memberInfo = prop;
			_name = prop.Name;
			_valueType = prop.PropertyType;

			_getter = () =>
			{
				var structValue = parentInspector.getValue();
				return ReflectionUtils.getPropertyGetter( prop ).Invoke( structValue, null );
			};
			_setter = ( val ) =>
			{
				var structValue = parentInspector.getValue();
				prop.SetValue( structValue, val );
				parentInspector.setValue( structValue );
			};
		}


		public void setTarget( object target, MethodInfo method )
		{
			_memberInfo = method;
			_target = target;
			_name = method.Name;
		}


		protected T getValue<T>()
		{
			return (T)_getter.Invoke();
		}


		protected object getValue()
		{
			return _getter.Invoke();
		}


		protected void setValue( object value )
		{
			_setter.Invoke( value );
		}


		protected T getFieldOrPropertyAttribute<T>() where T : Attribute
		{
			var attributes = _memberInfo.GetCustomAttributes<T>();
			foreach( var attr in attributes )
			{
				if( attr is T )
					return attr;
			}
			return null;
		}


		/// <summary>
		/// creates the name label and adds a tooltip if present
		/// </summary>
		/// <returns>The name label.</returns>
		/// <param name="table">Table.</param>
		/// <param name="skin">Skin.</param>
		protected Label createNameLabel( Table table, Skin skin, float leftCellWidth = -1 )
		{
			var label = new Label( _name, skin );
			label.setTouchable( Touchable.Enabled );

			// set a width on the cell so long labels dont cause issues if we have a leftCellWidth set
			if( leftCellWidth > 0 )
				label.setEllipsis( "..." ).setWidth( leftCellWidth );

			var tooltipAttribute = getFieldOrPropertyAttribute<TooltipAttribute>();
			if( tooltipAttribute != null )
			{
				var tooltip = new TextTooltip( tooltipAttribute.tooltip, label, skin );
				table.getStage().addElement( tooltip );
			}

			return label;
		}


		/// <summary>
		/// used to setup the UI for the Inspector
		/// </summary>
		/// <param name="table">Table.</param>
		/// <param name="skin">Skin.</param>
		public abstract void initialize( Table table, Skin skin, float leftCellWidth );


		/// <summary>
		/// used to update the UI for the Inspector
		/// </summary>
		public abstract void update();

	}
}
#endif