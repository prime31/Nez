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
        public static int getIdScope() => _idScope++;
        
		public static void smallVerticalSpace() => ImGui.Dummy( new Num.Vector2( 0, 5 ) );

        public static void mediumVerticalSpace() => ImGui.Dummy( new Num.Vector2( 0, 10 ) );
	}
}
