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
		public static int SetScopeId() => _idScope++;

		public static void SmallVerticalSpace() => ImGui.Dummy( new Num.Vector2( 0, 5 ) );

		public static void MediumVerticalSpace() => ImGui.Dummy( new Num.Vector2( 0, 10 ) );

		public static void BeginBorderedGroup()
		{
			ImGui.BeginGroup();
		}

		public static void EndBorderedGroup() => EndBorderedGroup( new Num.Vector2( 3, 2 ) );

		public static void EndBorderedGroup( Num.Vector2 minPadding )
		{
			ImGui.EndGroup();

			// attempt to size the border around the content to frame it
			var color = ImGui.GetStyle().Colors[(int)ImGuiCol.Border];

			var min = ImGui.GetItemRectMin();
			var max = ImGui.GetItemRectMax();
			max.X = min.X + ImGui.GetContentRegionAvailWidth();
			max.Y += 3;
			ImGui.GetWindowDrawList().AddRect( min - minPadding, max, ImGui.ColorConvertFloat4ToU32( color ) );

			// this fits just the content, not the full width
			//ImGui.GetWindowDrawList().AddRect( ImGui.GetItemRectMin() - padding, ImGui.GetItemRectMax() + padding, packedColor );
		}

		/// <summary>
		/// aligns a button and label in the same way LabelText and regular widgets are lined up
		/// </summary>
		/// <param name="label"></param>
		/// <param name="buttonText"></param>
		/// <returns></returns>
		public static bool LabelButton( string label, string buttonText )
		{
			ImGui.AlignTextToFramePadding();

			var wasClicked = ImGui.Button( buttonText );
			ImGui.SameLine( 0, ImGui.GetWindowWidth() * 0.65f - ImGui.GetItemRectSize().X + ImGui.GetStyle().ItemInnerSpacing.X );
			ImGui.Text( label );

			return wasClicked;
		}

		/// <summary>
		/// most widgets heights are calculated using this formula. Some let you specifiy a height though.
		/// </summary>
		/// <returns></returns>
		public static float DefaultWidgetHeight()
		{
			var style = ImGui.GetStyle();
			return ImGui.GetFontSize() + style.FramePadding.Y * 2f;
		}

		/// <summary>
		/// draws an invisible button that will cover the next widget rect
		/// </summary>
		/// <param name="widgetCustomHeight"></param>
		public static void DisableNextWidget( float widgetCustomHeight = 0 )
		{
			var origCursorPos = ImGui.GetCursorPos();
			var widgetSize = new Num.Vector2( ImGui.GetContentRegionAvailWidth(), widgetCustomHeight > 0 ? DefaultWidgetHeight() : DefaultWidgetHeight() );
			ImGui.InvisibleButton( "##disabled", widgetSize );
			ImGui.SetCursorPos( origCursorPos );
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
		public static bool SimpleDialog( string name, string message, string okButton = "OK", string cxlButton = "Cancel" )
		{
            var result = false;
            var junkBool = true;
			if( ImGui.BeginPopupModal( name, ref junkBool, ImGuiWindowFlags.AlwaysAutoResize ) )
			{
                result = false;

				ImGui.Text( message );
				MediumVerticalSpace();
				ImGui.Separator();
				SmallVerticalSpace();

				if( ImGui.Button( cxlButton, new Num.Vector2( 120, 0 ) ) )
                {
                    ImGui.CloseCurrentPopup();
                }

				ImGui.SetItemDefaultFocus();
				ImGui.SameLine();
				if( ImGui.Button( okButton, new Num.Vector2( 120, 0 ) ) )
                {
                    result = true;
                    ImGui.CloseCurrentPopup();
                }

				ImGui.EndPopup();
			}
            return result;
		}
	}
}
