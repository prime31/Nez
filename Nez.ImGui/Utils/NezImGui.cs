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
        public static int getScopeId() => _idScope++;
        
		public static void smallVerticalSpace() => ImGui.Dummy( new Num.Vector2( 0, 5 ) );

        public static void mediumVerticalSpace() => ImGui.Dummy( new Num.Vector2( 0, 10 ) );

        public static void beginBorderedGroup()
        {
			ImGui.BeginGroup();
        }

        public static void endBorderedGroup() => endBorderedGroup( new Num.Vector2( 3, 2 ) );

        public static void endBorderedGroup( Num.Vector2 minPadding )
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
	}
}
