using System;
using System.Reflection;
using ImGuiNET;

namespace Nez.ImGuiTools.TypeInspectors
{
	/// <summary>
	/// subclasses are used to inspect various built-in types. A bit of care has to be taken when we are dealing with any non-value types. Objects
	/// can be null and we don't want to inspect a null object. Having a null value for an inspected class when initialize is called means we
	/// cant create the AbstractTypeInspectors for the fields of the object since we need an object to wrap the getter/setter with.
	/// </summary>
	public abstract class AbstractTypeInspector
	{
		public string name => _name;

		/// <summary>
		/// parent inspectors that also keep a list sub-inspectors can check this to ensure the object the sub-inspectors was inspecting
		/// is still around. Of course, child inspectors must be dilgent about setting it when the remove themselves!
		/// </summary>
		public bool isTargetDestroyed => _isTargetDestroyed;

		protected int _scopeId = NezImGui.GetScopeId();
		protected bool _wantsIndentWhenDrawn;

		protected bool _isTargetDestroyed;
		protected object _target;
		protected string _name;
		protected Type _valueType;
		protected Func<object, object> _getter;
		protected Action<object> _setter;
		protected MemberInfo _memberInfo;
		protected bool _isReadOnly;
		protected string _tooltip;

		
		/// <summary>
		/// used to prep the inspector
		/// </summary>
		public virtual void initialize()
		{
			_tooltip = _memberInfo.getCustomAttribute<TooltipAttribute>()?.tooltip;
		}

		/// <summary>
		/// used to draw the UI for the Inspector. Calls either drawMutable or drawReadOnly depending on the _isReadOnly bool
		/// </summary>
		public void draw()
		{
			if( _wantsIndentWhenDrawn )
				ImGui.Indent();

			ImGui.PushID( _scopeId );
			if( _isReadOnly )
			{
				ImGui.PushStyleVar( ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f );
				drawReadOnly();
				ImGui.PopStyleVar();
			}
			else
			{
				drawMutable();
			}
			ImGui.PopID();

			if( _wantsIndentWhenDrawn )
				ImGui.Unindent();
		}

		public abstract void drawMutable();

		/// <summary>
		/// default implementation disables the next widget and calls through to drawMutable. If specialy drawing needs to
		/// be done (such as a multi-widget setup) this can be overridden.
		/// </summary>
		public virtual void drawReadOnly()
		{
			NezImGui.DisableNextWidget();
			drawMutable();
		}

		/// <summary>
		/// if there is a tooltip and the item is hovered this will display it
		/// </summary>
		protected void handleTooltip()
		{
			if( !string.IsNullOrEmpty( _tooltip ) && ImGui.IsItemHovered() )
			{
				ImGui.BeginTooltip();
				ImGui.Text( _tooltip );
				ImGui.EndTooltip();
			}
		}


		#region Set target methods

		public void setTarget( object target, FieldInfo field )
		{
			_target = target;
			_memberInfo = field;
			_name = field.Name;
			_valueType = field.FieldType;
			_isReadOnly = field.IsInitOnly;

			if( target == null )
				return;

			_getter = obj =>
			{
				return field.GetValue( obj );
			};

			if( !_isReadOnly )
			{
				_setter = ( val ) =>
				{
					field.SetValue( target, val );
				};
			}
		}

		public void setTarget( object target, PropertyInfo prop )
		{
			_memberInfo = prop;
			_target = target;
			_name = prop.Name;
			_valueType = prop.PropertyType;
			_isReadOnly = !prop.CanWrite;

			if( target == null )
				return;

			_getter = obj =>
			{
				return prop.GetMethod.Invoke( obj, null );
			};

			if( !_isReadOnly )
			{
				_setter = ( val ) =>
				{
					prop.SetMethod.Invoke( target, new object[] { val } );
				};
			}
		}

		/// <summary>
		/// this version will first fetch the struct before getting/setting values on it when invoking the getter/setter
		/// </summary>
		/// <returns>The struct target.</returns>
		/// <param name="target">Target.</param>
		/// <param name="structName">Struct name.</param>
		/// <param name="field">Field.</param>
		public void setStructTarget( object target, AbstractTypeInspector parentInspector, FieldInfo field )
		{
			_target = target;
			_memberInfo = field;
			_name = field.Name;
			_valueType = field.FieldType;
			_isReadOnly = field.IsInitOnly || parentInspector._isReadOnly;

			_getter = obj =>
			{
				var structValue = parentInspector.getValue();
				return field.GetValue( structValue );
			};

			if( !_isReadOnly )
			{
				_setter = val =>
				{
					var structValue = parentInspector.getValue();
					field.SetValue( structValue, val );
					parentInspector.setValue( structValue );
				};
			}
		}

		/// <summary>
		/// this version will first fetch the struct before getting/setting values on it when invoking the getter/setter
		/// </summary>
		/// <returns>The struct target.</returns>
		/// <param name="target">Target.</param>
		/// <param name="structName">Struct name.</param>
		/// <param name="field">Field.</param>
		public void setStructTarget( object target, AbstractTypeInspector parentInspector, PropertyInfo prop )
		{
			_target = target;
			_memberInfo = prop;
			_name = prop.Name;
			_valueType = prop.PropertyType;
			_isReadOnly = !prop.CanWrite || parentInspector._isReadOnly;

			_getter = obj =>
			{
				var structValue = parentInspector.getValue();
				return ReflectionUtils.getPropertyGetter( prop ).Invoke( structValue, null );
			};

			if( !_isReadOnly )
			{
				_setter = ( val ) =>
				{
					var structValue = parentInspector.getValue();
					prop.SetValue( structValue, val );
					parentInspector.setValue( structValue );
				};
			}
		}

		public void setTarget( object target, MethodInfo method )
		{
			_memberInfo = method;
			_target = target;
			_name = method.Name;
		}
	
		#endregion


		#region Get/set values

		protected T getValue<T>()
		{
			return (T)_getter( _target );
		}

		protected object getValue()
		{
			return _getter( _target );
		}

		protected void setValue( object value )
		{
			_setter.Invoke( value );
		}

		#endregion

	}
}
