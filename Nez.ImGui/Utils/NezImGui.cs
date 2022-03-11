using System;
using System.Runtime.InteropServices;
using ImGuiNET;
using Num = System.Numerics;


namespace Nez.ImGuiTools
{
	public static class NezImGui
	{
		static int _idScope;

		/// <summary>
		/// gets a unique id that can be used with ImGui.PushId() to avoid conflicts with type inspectors
		/// </summary>
		/// <returns></returns>
		public static int GetScopeId() => _idScope++;

		public static void SmallVerticalSpace() => ImGui.Dummy(new Num.Vector2(0, 5));

		public static void MediumVerticalSpace() => ImGui.Dummy(new Num.Vector2(0, 10));

		/// <summary>
		/// adds a DrawList command to draw a border around the group
		/// </summary>
		public static void BeginBorderedGroup()
		{
			ImGui.BeginGroup();
		}

		public static void EndBorderedGroup() => EndBorderedGroup(new Num.Vector2(3, 2), new Num.Vector2(0, 3));

		public static void EndBorderedGroup(Num.Vector2 minPadding, Num.Vector2 maxPadding = default(Num.Vector2))
		{
			ImGui.EndGroup();

			// attempt to size the border around the content to frame it
			var color = ImGui.GetStyle().Colors[(int) ImGuiCol.Border];

			var min = ImGui.GetItemRectMin();
			var max = ImGui.GetItemRectMax();
			max.X = min.X + ImGui.GetContentRegionAvail().X;
			ImGui.GetWindowDrawList().AddRect(min - minPadding, max + maxPadding, ImGui.ColorConvertFloat4ToU32(color));

			// this fits just the content, not the full width
			//ImGui.GetWindowDrawList().AddRect( ImGui.GetItemRectMin() - padding, ImGui.GetItemRectMax() + padding, packedColor );
		}

		/// <summary>
		/// aligns a button and label in the same way LabelText and regular widgets are lined up
		/// </summary>
		/// <param name="label"></param>
		/// <param name="buttonText"></param>
		/// <returns></returns>
		public static bool LabelButton(string label, string buttonText)
		{
			ImGui.AlignTextToFramePadding();

			var wasClicked = ImGui.Button(buttonText);
			ImGui.SameLine(0,
				ImGui.GetWindowWidth() * 0.65f - ImGui.GetItemRectSize().X + ImGui.GetStyle().ItemInnerSpacing.X);
			ImGui.Text(label);

			return wasClicked;
		}

		/// <summary>
		/// most widgets heights are calculated using this formula. Some let you specifiy a height though.
		/// </summary>
		/// <returns></returns>
		public static float GetDefaultWidgetHeight() => ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2f;

		/// <summary>
		/// draws an invisible button that will cover the next widget rect
		/// </summary>
		/// <param name="widgetCustomHeight"></param>
		public static void DisableNextWidget(float widgetCustomHeight = 0)
		{
			var origCursorPos = ImGui.GetCursorPos();
			var widgetSize = new Num.Vector2(ImGui.GetContentRegionAvail().X,
                widgetCustomHeight > 0 ? widgetCustomHeight : GetDefaultWidgetHeight());
			ImGui.InvisibleButton("##disabled", widgetSize);
			ImGui.SetCursorPos(origCursorPos);
		}

		/// <summary>
		/// draws a button with the width as a percentage of the window contnet region centered.
		/// </summary>
		/// <param name="percentWidth"></param>
		/// <returns></returns>
		public static bool CenteredButton(string label, float percentWidth, float xIndent = 0)
		{
			var buttonWidth = ImGui.GetWindowContentRegionWidth() * percentWidth;
			ImGui.SetCursorPosX(xIndent + (ImGui.GetWindowContentRegionWidth() - buttonWidth) / 2f);
			return ImGui.Button(label, new System.Numerics.Vector2(buttonWidth, GetDefaultWidgetHeight()));
		}

		/// <summary>
		/// shows a tooltip informing the user they can right click
		/// </summary>
		public static void ShowContextMenuTooltip()
		{
			if (ImGui.IsItemHovered())
			{
				ImGui.BeginTooltip();
				ImGui.Text("Right click for more options");
				ImGui.EndTooltip();
			}
		}

		/// <summary>
		/// displays a simple dialog with some text and a couple buttons. Note that ImGui.OpenPopup( name ) has to be called
		/// in the same ID scope as this call.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="message"></param>
		/// <param name="okButton"></param>
		/// <param name="cxlButton"></param>
		/// <returns></returns>
		public static bool SimpleDialog(string name, string message, string okButton = "OK",
		                                string cxlButton = "Cancel")
		{
			var result = false;
			var junkBool = true;
			if (ImGui.BeginPopupModal(name, ref junkBool, ImGuiWindowFlags.AlwaysAutoResize))
			{
				result = false;

				ImGui.TextWrapped(message);
				MediumVerticalSpace();
				ImGui.Separator();
				SmallVerticalSpace();

				if (ImGui.Button(cxlButton, new Num.Vector2(120, 0)))
				{
					ImGui.CloseCurrentPopup();
				}

				ImGui.SetItemDefaultFocus();
				ImGui.SameLine();
				if (ImGui.Button(okButton, new Num.Vector2(120, 0)))
				{
					result = true;
					ImGui.CloseCurrentPopup();
				}

				ImGui.EndPopup();
			}

			return result;
		}

		#region Wrappers for unsinged Drag/SliderScaler

		/// <summary>
		/// wraps ImGui.DragScaler and handles all IntPtr conversion
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <param name="speed"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public unsafe static bool DragScaler(string label, ref ulong value, float speed, int min, int max)
		{
			var tempValue = value;
			var valuePtr = new IntPtr(&tempValue);
			var minPtr = new IntPtr(&min);
			var maxPtr = new IntPtr(&max);

			if (ImGui.DragScalar(label, ImGuiDataType.U64, valuePtr, speed, minPtr, maxPtr))
			{
				value = Marshal.PtrToStructure<ulong>(valuePtr);
				return true;
			}

			return false;
		}

		public unsafe static bool DragScaler(string label, ref ulong value, float speed)
		{
			var tempValue = value;
			var valuePtr = new IntPtr(&tempValue);

			if (ImGui.DragScalar(label, ImGuiDataType.U64, valuePtr, speed))
			{
				value = Marshal.PtrToStructure<ulong>(valuePtr);
				return true;
			}

			return false;
		}

		/// <summary>
		/// wraps ImGui.DragScaler and handles all IntPtr conversion
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <param name="speed"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public unsafe static bool DragScaler(string label, ref uint value, float speed, int min, int max)
		{
			var tempValue = value;
			var valuePtr = new IntPtr(&tempValue);
			var minPtr = new IntPtr(&min);
			var maxPtr = new IntPtr(&max);

			if (ImGui.DragScalar(label, ImGuiDataType.U32, valuePtr, speed, minPtr, maxPtr))
			{
				value = Marshal.PtrToStructure<uint>(valuePtr);
				return true;
			}

			return false;
		}

		public unsafe static bool DragScaler(string label, ref uint value, float speed)
		{
			var tempValue = value;
			var valuePtr = new IntPtr(&tempValue);

			if (ImGui.DragScalar(label, ImGuiDataType.U32, valuePtr, speed))
			{
				value = Marshal.PtrToStructure<uint>(valuePtr);
				return true;
			}

			return false;
		}

		/// <summary>
		/// wraps ImGui.SliderScalar and handles all IntPtr conversion
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <param name="speed"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public unsafe static bool SliderScalar(string label, ref ulong value, int min, int max)
		{
			var tempValue = value;
			var valuePtr = new IntPtr(&tempValue);
			var minPtr = new IntPtr(&min);
			var maxPtr = new IntPtr(&max);

			if (ImGui.SliderScalar(label, ImGuiDataType.U64, valuePtr, minPtr, maxPtr))
			{
				value = Marshal.PtrToStructure<ulong>(valuePtr);
				return true;
			}

			return false;
		}

		/// <summary>
		/// wraps ImGui.SliderScalar and handles all IntPtr conversion
		/// </summary>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <param name="speed"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public unsafe static bool SliderScalar(string label, ref uint value, int min, int max)
		{
			var tempValue = value;
			var valuePtr = new IntPtr(&tempValue);
			var minPtr = new IntPtr(&min);
			var maxPtr = new IntPtr(&max);

			if (ImGui.SliderScalar(label, ImGuiDataType.U32, valuePtr, minPtr, maxPtr))
			{
				value = Marshal.PtrToStructure<uint>(valuePtr);
				return true;
			}

			return false;
		}

		public unsafe static bool InputScaler(string label, ref ulong value)
		{
			var tempValue = value;
			var valuePtr = new IntPtr(&tempValue);

			if (ImGui.InputScalar(label, ImGuiDataType.U64, valuePtr))
			{
				value = Marshal.PtrToStructure<ulong>(valuePtr);
				return true;
			}

			return false;
		}

		public unsafe static bool InputScaler(string label, ref uint value)
		{
			var tempValue = value;
			var valuePtr = new IntPtr(&tempValue);

			if (ImGui.InputScalar(label, ImGuiDataType.U32, valuePtr))
			{
				value = Marshal.PtrToStructure<uint>(valuePtr);
				return true;
			}

			return false;
		}

		#endregion
	}
}