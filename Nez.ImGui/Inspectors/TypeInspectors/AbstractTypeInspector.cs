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
		public string Name => _name;

		/// <summary>
		/// parent inspectors that also keep a list sub-inspectors can check this to ensure the object the sub-inspectors was inspecting
		/// is still around. Of course, child inspectors must be dilgent about setting it when the remove themselves!
		/// </summary>
		public bool IsTargetDestroyed => _isTargetDestroyed;

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
		public virtual void Initialize()
		{
			_tooltip = _memberInfo.GetAttribute<TooltipAttribute>()?.Tooltip;
		}

		/// <summary>
		/// used to draw the UI for the Inspector. Calls either drawMutable or drawReadOnly depending on the _isReadOnly bool
		/// </summary>
		public void Draw()
		{
			if (_wantsIndentWhenDrawn)
				ImGui.Indent();

			ImGui.PushID(_scopeId);
			if (_isReadOnly)
			{
				ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
				DrawReadOnly();
				ImGui.PopStyleVar();
			}
			else
			{
				DrawMutable();
			}

			ImGui.PopID();

			if (_wantsIndentWhenDrawn)
				ImGui.Unindent();
		}

		public abstract void DrawMutable();

		/// <summary>
		/// default implementation disables the next widget and calls through to drawMutable. If specialy drawing needs to
		/// be done (such as a multi-widget setup) this can be overridden.
		/// </summary>
		public virtual void DrawReadOnly()
		{
			NezImGui.DisableNextWidget();
			DrawMutable();
		}

		/// <summary>
		/// if there is a tooltip and the item is hovered this will display it
		/// </summary>
		protected void HandleTooltip()
		{
			if (!string.IsNullOrEmpty(_tooltip) && ImGui.IsItemHovered())
			{
				ImGui.BeginTooltip();
				ImGui.Text(_tooltip);
				ImGui.EndTooltip();
			}
		}


		#region Set target methods

		public void SetTarget(object target, FieldInfo field)
		{
			_target = target;
			_memberInfo = field;
			_name = field.Name;
			_valueType = field.FieldType;
			_isReadOnly = field.IsInitOnly;

			if (target == null)
				return;

			_getter = obj => { return field.GetValue(obj); };

			if (!_isReadOnly)
			{
				_setter = (val) => { field.SetValue(target, val); };
			}
		}

		public void SetTarget(object target, PropertyInfo prop)
		{
			_memberInfo = prop;
			_target = target;
			_name = prop.Name;
			_valueType = prop.PropertyType;
			_isReadOnly = !prop.CanWrite;

			if (target == null)
				return;

			_getter = obj => { return prop.GetMethod.Invoke(obj, null); };

			if (!_isReadOnly)
			{
				_setter = (val) => { prop.SetMethod.Invoke(target, new object[] {val}); };
			}
		}

		/// <summary>
		/// this version will first fetch the struct before getting/setting values on it when invoking the getter/setter
		/// </summary>
		/// <returns>The struct target.</returns>
		/// <param name="target">Target.</param>
		/// <param name="structName">Struct name.</param>
		/// <param name="field">Field.</param>
		public void SetStructTarget(object target, AbstractTypeInspector parentInspector, FieldInfo field)
		{
			_target = target;
			_memberInfo = field;
			_name = field.Name;
			_valueType = field.FieldType;
			_isReadOnly = field.IsInitOnly || parentInspector._isReadOnly;

			_getter = obj =>
			{
				var structValue = parentInspector.GetValue();
				return field.GetValue(structValue);
			};

			if (!_isReadOnly)
			{
				_setter = val =>
				{
					var structValue = parentInspector.GetValue();
					field.SetValue(structValue, val);
					parentInspector.SetValue(structValue);
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
		public void SetStructTarget(object target, AbstractTypeInspector parentInspector, PropertyInfo prop)
		{
			_target = target;
			_memberInfo = prop;
			_name = prop.Name;
			_valueType = prop.PropertyType;
			_isReadOnly = !prop.CanWrite || parentInspector._isReadOnly;

			_getter = obj =>
			{
				var structValue = parentInspector.GetValue();
				return ReflectionUtils.GetPropertyGetter(prop).Invoke(structValue, null);
			};

			if (!_isReadOnly)
			{
				_setter = (val) =>
				{
					var structValue = parentInspector.GetValue();
					prop.SetValue(structValue, val);
					parentInspector.SetValue(structValue);
				};
			}
		}

		public void SetTarget(object target, MethodInfo method)
		{
			_memberInfo = method;
			_target = target;
			_name = method.Name;
		}

		#endregion


		#region Get/set values

		protected T GetValue<T>()
		{
			return (T) _getter(_target);
		}

		protected object GetValue()
		{
			return _getter(_target);
		}

		protected void SetValue(object value)
		{
			_setter.Invoke(value);
		}

		#endregion
	}
}