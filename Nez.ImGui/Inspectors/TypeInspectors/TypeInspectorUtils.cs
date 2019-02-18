using System.Collections.Generic;
using Nez.IEnumerableExtensions;
using System.Reflection;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TI = Nez.ImGuiTools.TypeInspectors;

namespace Nez.ImGuiTools.TypeInspectors
{
    public static class TypeInspectorUtils
    {
        /// <summary>
        /// fetches all the relevant AbstractTypeInspectors for target including fields, properties and methods.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
		public static List<AbstractTypeInspector> getInspectableProperties( object target )
		{
			var props = new List<AbstractTypeInspector>();
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
					inspector.initialize();
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
					inspector.initialize();
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
				inspector.initialize();
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
		public static AbstractTypeInspector getInspectorForType( Type valueType, object target, MemberInfo memberInfo )
		{
			// built-in types
			if( valueType == typeof( int ) )
				return new TI.IntInspector();
			if( valueType == typeof( float ) )
				return new TI.FloatInspector();
			if( valueType == typeof( bool ) )
				return new TI.BoolInspector();
			if( valueType == typeof( string ) )
				return new TI.StringInspector();
			if( valueType == typeof( Vector2 ) )
				return new TI.Vector2Inspector();
			if( valueType == typeof( Vector3 ) )
				return new TI.Vector3Inspector();
			if( valueType == typeof( Color ) )
				return new TI.ColorInspector();
			if( valueType.GetTypeInfo().IsEnum )
				return new TI.EnumInspector();
			if( valueType.GetTypeInfo().IsValueType )
				return new TI.StructInspector();

			// check for custom inspectors before checking Nez types in case a subclass implemented one
			var customInspectorType = valueType.GetTypeInfo().GetCustomAttribute<CustomInspectorAttribute>();
			if( customInspectorType != null )
			{
				if( customInspectorType.inspectorType.GetTypeInfo().IsSubclassOf( typeof( AbstractTypeInspector ) ) )
					return (AbstractTypeInspector)Activator.CreateInstance( customInspectorType.inspectorType );
				Debug.warn( $"found CustomInspector {customInspectorType.inspectorType} but it is not a subclass of Inspector" );
			}

			// Nez types
			if( valueType == typeof( Material ) )
				return getMaterialInspector( target );
			if( valueType.GetTypeInfo().IsSubclassOf( typeof( Effect ) ) )
				return getEffectInspector( target, memberInfo );

			Debug.log( $"no inspector found for type {valueType}" );

			return null;
		}

		/// <summary>
		/// null checks the Material and Material.effect and ony returns an Inspector if we have data
		/// </summary>
		/// <returns>The material inspector.</returns>
		/// <param name="target">Target.</param>
		static AbstractTypeInspector getMaterialInspector( object target )
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
		static AbstractTypeInspector getEffectInspector( object target, MemberInfo memberInfo )
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

    }
}
