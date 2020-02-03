using ImGuiNET;
using Num = System.Numerics;


namespace Nez.ImGuiTools
{
	public static class NezImGuiThemes
	{
		public static void DefaultDarkTheme()
		{
			ImGui.StyleColorsDark();
		}

		public static void DefaultLightTheme()
		{
			ImGui.StyleColorsLight();
		}

		public static void DefaultClassic()
		{
			ImGui.StyleColorsClassic();
		}

		public static void DarkHighContrastTheme()
		{
			var style = ImGui.GetStyle();

			style.WindowPadding = new Num.Vector2(15, 15);
			style.WindowRounding = 5.0f;
			style.FramePadding = new Num.Vector2(5, 5);
			style.FrameRounding = 4.0f;
			style.ItemSpacing = new Num.Vector2(12, 8);
			style.ItemInnerSpacing = new Num.Vector2(8, 6);
			style.ScrollbarSize = 15.0f;
			style.ScrollbarRounding = 9.0f;
			style.GrabMinSize = 5.0f;
			style.GrabRounding = 3.0f;

			style.Colors[(int) ImGuiCol.Text] = new Num.Vector4(0.80f, 0.80f, 0.83f, 1.00f);
			style.Colors[(int) ImGuiCol.TextDisabled] = new Num.Vector4(0.24f, 0.23f, 0.29f, 1.00f);
			style.Colors[(int) ImGuiCol.WindowBg] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
			style.Colors[(int) ImGuiCol.ChildBg] = new Num.Vector4(0.07f, 0.07f, 0.09f, 1.00f);
			style.Colors[(int) ImGuiCol.PopupBg] = new Num.Vector4(0.07f, 0.07f, 0.09f, 1.00f);
			style.Colors[(int) ImGuiCol.Border] = new Num.Vector4(0.80f, 0.80f, 0.83f, 0.88f);
			style.Colors[(int) ImGuiCol.BorderShadow] = new Num.Vector4(0.92f, 0.91f, 0.88f, 0.00f);
			style.Colors[(int) ImGuiCol.FrameBg] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
			style.Colors[(int) ImGuiCol.FrameBgHovered] = new Num.Vector4(0.24f, 0.23f, 0.29f, 1.00f);
			style.Colors[(int) ImGuiCol.FrameBgActive] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
			style.Colors[(int) ImGuiCol.TitleBg] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
			style.Colors[(int) ImGuiCol.TitleBgCollapsed] = new Num.Vector4(1.00f, 0.98f, 0.95f, 0.75f);
			style.Colors[(int) ImGuiCol.TitleBgActive] = new Num.Vector4(0.07f, 0.07f, 0.09f, 1.00f);
			style.Colors[(int) ImGuiCol.MenuBarBg] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
			style.Colors[(int) ImGuiCol.ScrollbarBg] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
			style.Colors[(int) ImGuiCol.ScrollbarGrab] = new Num.Vector4(0.80f, 0.80f, 0.83f, 0.31f);
			style.Colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
			style.Colors[(int) ImGuiCol.ScrollbarGrabActive] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
			style.Colors[(int) ImGuiCol.CheckMark] = new Num.Vector4(0.80f, 0.80f, 0.83f, 0.31f);
			style.Colors[(int) ImGuiCol.SliderGrab] = new Num.Vector4(0.80f, 0.80f, 0.83f, 0.31f);
			style.Colors[(int) ImGuiCol.SliderGrabActive] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
			style.Colors[(int) ImGuiCol.Button] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
			style.Colors[(int) ImGuiCol.ButtonHovered] = new Num.Vector4(0.24f, 0.23f, 0.29f, 1.00f);
			style.Colors[(int) ImGuiCol.ButtonActive] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
			style.Colors[(int) ImGuiCol.Header] = new Num.Vector4(0.10f, 0.09f, 0.12f, 1.00f);
			style.Colors[(int) ImGuiCol.HeaderHovered] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
			style.Colors[(int) ImGuiCol.HeaderActive] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
			style.Colors[(int) ImGuiCol.ResizeGrip] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			style.Colors[(int) ImGuiCol.ResizeGripHovered] = new Num.Vector4(0.56f, 0.56f, 0.58f, 1.00f);
			style.Colors[(int) ImGuiCol.ResizeGripActive] = new Num.Vector4(0.06f, 0.05f, 0.07f, 1.00f);
			style.Colors[(int) ImGuiCol.PlotLines] = new Num.Vector4(0.40f, 0.39f, 0.38f, 0.63f);
			style.Colors[(int) ImGuiCol.PlotLinesHovered] = new Num.Vector4(0.25f, 1.00f, 0.00f, 1.00f);
			style.Colors[(int) ImGuiCol.PlotHistogram] = new Num.Vector4(0.40f, 0.39f, 0.38f, 0.63f);
			style.Colors[(int) ImGuiCol.PlotHistogramHovered] = new Num.Vector4(0.25f, 1.00f, 0.00f, 1.00f);
			style.Colors[(int) ImGuiCol.TextSelectedBg] = new Num.Vector4(0.25f, 1.00f, 0.00f, 0.43f);
			style.Colors[(int) ImGuiCol.ModalWindowDimBg] = new Num.Vector4(1.00f, 0.98f, 0.95f, 0.73f);
		}

		public static void DarkTheme1()
		{
			var colors = ImGui.GetStyle().Colors;

			colors[(int) ImGuiCol.Text] = new Num.Vector4(1.00f, 1.00f, 1.00f, 1.00f);
			colors[(int) ImGuiCol.TextDisabled] = new Num.Vector4(0.50f, 0.50f, 0.50f, 1.00f);
			colors[(int) ImGuiCol.WindowBg] = new Num.Vector4(0.06f, 0.06f, 0.06f, 0.94f);
			colors[(int) ImGuiCol.ChildBg] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.00f);
			colors[(int) ImGuiCol.PopupBg] = new Num.Vector4(0.08f, 0.08f, 0.08f, 0.94f);
			colors[(int) ImGuiCol.Border] = new Num.Vector4(0.43f, 0.43f, 0.50f, 0.50f);
			colors[(int) ImGuiCol.BorderShadow] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			colors[(int) ImGuiCol.FrameBg] = new Num.Vector4(0.20f, 0.21f, 0.22f, 0.54f);
			colors[(int) ImGuiCol.FrameBgHovered] = new Num.Vector4(0.40f, 0.40f, 0.40f, 0.40f);
			colors[(int) ImGuiCol.FrameBgActive] = new Num.Vector4(0.18f, 0.18f, 0.18f, 0.67f);
			colors[(int) ImGuiCol.TitleBg] = new Num.Vector4(0.04f, 0.04f, 0.04f, 1.00f);
			colors[(int) ImGuiCol.TitleBgActive] = new Num.Vector4(0.29f, 0.29f, 0.29f, 1.00f);
			colors[(int) ImGuiCol.TitleBgCollapsed] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.51f);
			colors[(int) ImGuiCol.MenuBarBg] = new Num.Vector4(0.14f, 0.14f, 0.14f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarBg] = new Num.Vector4(0.02f, 0.02f, 0.02f, 0.53f);
			colors[(int) ImGuiCol.ScrollbarGrab] = new Num.Vector4(0.31f, 0.31f, 0.31f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Num.Vector4(0.41f, 0.41f, 0.41f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarGrabActive] = new Num.Vector4(0.51f, 0.51f, 0.51f, 1.00f);
			colors[(int) ImGuiCol.CheckMark] = new Num.Vector4(0.94f, 0.94f, 0.94f, 1.00f);
			colors[(int) ImGuiCol.SliderGrab] = new Num.Vector4(0.51f, 0.51f, 0.51f, 1.00f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Num.Vector4(0.86f, 0.86f, 0.86f, 1.00f);
			colors[(int) ImGuiCol.Button] = new Num.Vector4(0.44f, 0.44f, 0.44f, 0.40f);
			colors[(int) ImGuiCol.ButtonHovered] = new Num.Vector4(0.46f, 0.47f, 0.48f, 1.00f);
			colors[(int) ImGuiCol.ButtonActive] = new Num.Vector4(0.42f, 0.42f, 0.42f, 1.00f);
			colors[(int) ImGuiCol.Header] = new Num.Vector4(0.70f, 0.70f, 0.70f, 0.31f);
			colors[(int) ImGuiCol.HeaderHovered] = new Num.Vector4(0.70f, 0.70f, 0.70f, 0.80f);
			colors[(int) ImGuiCol.HeaderActive] = new Num.Vector4(0.48f, 0.50f, 0.52f, 1.00f);
			colors[(int) ImGuiCol.Separator] = new Num.Vector4(0.43f, 0.43f, 0.50f, 0.50f);
			colors[(int) ImGuiCol.SeparatorHovered] = new Num.Vector4(0.72f, 0.72f, 0.72f, 0.78f);
			colors[(int) ImGuiCol.SeparatorActive] = new Num.Vector4(0.51f, 0.51f, 0.51f, 1.00f);
			colors[(int) ImGuiCol.ResizeGrip] = new Num.Vector4(0.91f, 0.91f, 0.91f, 0.25f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Num.Vector4(0.81f, 0.81f, 0.81f, 0.67f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Num.Vector4(0.46f, 0.46f, 0.46f, 0.95f);
			colors[(int) ImGuiCol.PlotLines] = new Num.Vector4(0.61f, 0.61f, 0.61f, 1.00f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Num.Vector4(1.00f, 0.43f, 0.35f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogram] = new Num.Vector4(0.73f, 0.60f, 0.15f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Num.Vector4(1.00f, 0.60f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Num.Vector4(0.87f, 0.87f, 0.87f, 0.35f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Num.Vector4(0.80f, 0.80f, 0.80f, 0.35f);
			colors[(int) ImGuiCol.DragDropTarget] = new Num.Vector4(1.00f, 1.00f, 0.00f, 0.90f);
			colors[(int) ImGuiCol.NavHighlight] = new Num.Vector4(0.60f, 0.60f, 0.60f, 1.00f);
			colors[(int) ImGuiCol.NavWindowingHighlight] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.70f);
		}

		public static void DarkTheme2()
		{
			var st = ImGui.GetStyle();
			var colors = st.Colors;

			st.FrameBorderSize = 1.0f;
			st.FramePadding = new Num.Vector2(4.0f, 2.0f);
			st.ItemSpacing = new Num.Vector2(8.0f, 2.0f);
			st.WindowBorderSize = 1.0f;
			st.TabBorderSize = 1.0f;
			st.WindowRounding = 1.0f;
			st.ChildRounding = 1.0f;
			st.FrameRounding = 1.0f;
			st.ScrollbarRounding = 1.0f;
			st.GrabRounding = 1.0f;
			st.TabRounding = 1.0f;

			colors[(int) ImGuiCol.Text] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.95f);
			colors[(int) ImGuiCol.TextDisabled] = new Num.Vector4(0.50f, 0.50f, 0.50f, 1.00f);
			colors[(int) ImGuiCol.WindowBg] = new Num.Vector4(0.13f, 0.12f, 0.12f, 1.00f);
			colors[(int) ImGuiCol.ChildBg] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.00f);
			colors[(int) ImGuiCol.PopupBg] = new Num.Vector4(0.05f, 0.05f, 0.05f, 0.94f);
			colors[(int) ImGuiCol.Border] = new Num.Vector4(0.53f, 0.53f, 0.53f, 0.46f);
			colors[(int) ImGuiCol.BorderShadow] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			colors[(int) ImGuiCol.FrameBg] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.85f);
			colors[(int) ImGuiCol.FrameBgHovered] = new Num.Vector4(0.22f, 0.22f, 0.22f, 0.40f);
			colors[(int) ImGuiCol.FrameBgActive] = new Num.Vector4(0.16f, 0.16f, 0.16f, 0.53f);
			colors[(int) ImGuiCol.TitleBg] = new Num.Vector4(0.00f, 0.00f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.TitleBgActive] = new Num.Vector4(0.00f, 0.00f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.TitleBgCollapsed] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.51f);
			colors[(int) ImGuiCol.MenuBarBg] = new Num.Vector4(0.12f, 0.12f, 0.12f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarBg] = new Num.Vector4(0.02f, 0.02f, 0.02f, 0.53f);
			colors[(int) ImGuiCol.ScrollbarGrab] = new Num.Vector4(0.31f, 0.31f, 0.31f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Num.Vector4(0.41f, 0.41f, 0.41f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarGrabActive] = new Num.Vector4(0.48f, 0.48f, 0.48f, 1.00f);
			colors[(int) ImGuiCol.CheckMark] = new Num.Vector4(0.79f, 0.79f, 0.79f, 1.00f);
			colors[(int) ImGuiCol.SliderGrab] = new Num.Vector4(0.48f, 0.47f, 0.47f, 0.91f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Num.Vector4(0.56f, 0.55f, 0.55f, 0.62f);
			colors[(int) ImGuiCol.Button] = new Num.Vector4(0.50f, 0.50f, 0.50f, 0.63f);
			colors[(int) ImGuiCol.ButtonHovered] = new Num.Vector4(0.67f, 0.67f, 0.68f, 0.63f);
			colors[(int) ImGuiCol.ButtonActive] = new Num.Vector4(0.26f, 0.26f, 0.26f, 0.63f);
			colors[(int) ImGuiCol.Header] = new Num.Vector4(0.54f, 0.54f, 0.54f, 0.58f);
			colors[(int) ImGuiCol.HeaderHovered] = new Num.Vector4(0.64f, 0.65f, 0.65f, 0.80f);
			colors[(int) ImGuiCol.HeaderActive] = new Num.Vector4(0.25f, 0.25f, 0.25f, 0.80f);
			colors[(int) ImGuiCol.Separator] = new Num.Vector4(0.58f, 0.58f, 0.58f, 0.50f);
			colors[(int) ImGuiCol.SeparatorHovered] = new Num.Vector4(0.81f, 0.81f, 0.81f, 0.64f);
			colors[(int) ImGuiCol.SeparatorActive] = new Num.Vector4(0.81f, 0.81f, 0.81f, 0.64f);
			colors[(int) ImGuiCol.ResizeGrip] = new Num.Vector4(0.87f, 0.87f, 0.87f, 0.53f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Num.Vector4(0.87f, 0.87f, 0.87f, 0.74f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Num.Vector4(0.87f, 0.87f, 0.87f, 0.74f);
			colors[(int) ImGuiCol.Tab] = new Num.Vector4(0.01f, 0.01f, 0.01f, 0.86f);
			colors[(int) ImGuiCol.TabHovered] = new Num.Vector4(0.29f, 0.29f, 0.29f, 1.00f);
			colors[(int) ImGuiCol.TabActive] = new Num.Vector4(0.31f, 0.31f, 0.31f, 1.00f);
			colors[(int) ImGuiCol.TabUnfocused] = new Num.Vector4(0.02f, 0.02f, 0.02f, 1.00f);
			colors[(int) ImGuiCol.TabUnfocusedActive] = new Num.Vector4(0.19f, 0.19f, 0.19f, 1.00f);
			colors[(int) ImGuiCol.PlotLines] = new Num.Vector4(0.61f, 0.61f, 0.61f, 1.00f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Num.Vector4(0.68f, 0.68f, 0.68f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogram] = new Num.Vector4(0.90f, 0.77f, 0.33f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Num.Vector4(0.87f, 0.55f, 0.08f, 1.00f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Num.Vector4(0.47f, 0.60f, 0.76f, 0.47f);
			colors[(int) ImGuiCol.DragDropTarget] = new Num.Vector4(0.58f, 0.58f, 0.58f, 0.90f);
			colors[(int) ImGuiCol.NavHighlight] = new Num.Vector4(0.60f, 0.60f, 0.60f, 1.00f);
			colors[(int) ImGuiCol.NavWindowingHighlight] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.70f);
			colors[(int) ImGuiCol.NavWindowingDimBg] = new Num.Vector4(0.80f, 0.80f, 0.80f, 0.20f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Num.Vector4(0.80f, 0.80f, 0.80f, 0.35f);
		}

		public static void PhotoshopDark()
		{
			var style = ImGui.GetStyle();
			var colors = style.Colors;

			colors[(int) ImGuiCol.Text] = new Num.Vector4(1.000f, 1.000f, 1.000f, 1.000f);
			colors[(int) ImGuiCol.TextDisabled] = new Num.Vector4(0.500f, 0.500f, 0.500f, 1.000f);
			colors[(int) ImGuiCol.WindowBg] = new Num.Vector4(0.180f, 0.180f, 0.180f, 1.000f);
			colors[(int) ImGuiCol.ChildBg] = new Num.Vector4(0.280f, 0.280f, 0.280f, 0.000f);
			colors[(int) ImGuiCol.PopupBg] = new Num.Vector4(0.313f, 0.313f, 0.313f, 1.000f);
			colors[(int) ImGuiCol.Border] = new Num.Vector4(0.266f, 0.266f, 0.266f, 1.000f);
			colors[(int) ImGuiCol.BorderShadow] = new Num.Vector4(0.000f, 0.000f, 0.000f, 0.000f);
			colors[(int) ImGuiCol.FrameBg] = new Num.Vector4(0.160f, 0.160f, 0.160f, 1.000f);
			colors[(int) ImGuiCol.FrameBgHovered] = new Num.Vector4(0.200f, 0.200f, 0.200f, 1.000f);
			colors[(int) ImGuiCol.FrameBgActive] = new Num.Vector4(0.280f, 0.280f, 0.280f, 1.000f);
			colors[(int) ImGuiCol.TitleBg] = new Num.Vector4(0.148f, 0.148f, 0.148f, 1.000f);
			colors[(int) ImGuiCol.TitleBgActive] = new Num.Vector4(0.148f, 0.148f, 0.148f, 1.000f);
			colors[(int) ImGuiCol.TitleBgCollapsed] = new Num.Vector4(0.148f, 0.148f, 0.148f, 1.000f);
			colors[(int) ImGuiCol.MenuBarBg] = new Num.Vector4(0.195f, 0.195f, 0.195f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarBg] = new Num.Vector4(0.160f, 0.160f, 0.160f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarGrab] = new Num.Vector4(0.277f, 0.277f, 0.277f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Num.Vector4(0.300f, 0.300f, 0.300f, 1.000f);
			colors[(int) ImGuiCol.ScrollbarGrabActive] = new Num.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.CheckMark] = new Num.Vector4(1.000f, 1.000f, 1.000f, 1.000f);
			colors[(int) ImGuiCol.SliderGrab] = new Num.Vector4(0.391f, 0.391f, 0.391f, 1.000f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Num.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.Button] = new Num.Vector4(1.000f, 1.000f, 1.000f, 0.000f);
			colors[(int) ImGuiCol.ButtonHovered] = new Num.Vector4(1.000f, 1.000f, 1.000f, 0.156f);
			colors[(int) ImGuiCol.ButtonActive] = new Num.Vector4(1.000f, 1.000f, 1.000f, 0.391f);
			colors[(int) ImGuiCol.Header] = new Num.Vector4(0.313f, 0.313f, 0.313f, 1.000f);
			colors[(int) ImGuiCol.HeaderHovered] = new Num.Vector4(0.469f, 0.469f, 0.469f, 1.000f);
			colors[(int) ImGuiCol.HeaderActive] = new Num.Vector4(0.469f, 0.469f, 0.469f, 1.000f);
			colors[(int) ImGuiCol.Separator] = colors[(int) ImGuiCol.Border];
			colors[(int) ImGuiCol.SeparatorHovered] = new Num.Vector4(0.391f, 0.391f, 0.391f, 1.000f);
			colors[(int) ImGuiCol.SeparatorActive] = new Num.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.ResizeGrip] = new Num.Vector4(1.000f, 1.000f, 1.000f, 0.250f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Num.Vector4(1.000f, 1.000f, 1.000f, 0.670f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Num.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.Tab] = new Num.Vector4(0.098f, 0.098f, 0.098f, 1.000f);
			colors[(int) ImGuiCol.TabHovered] = new Num.Vector4(0.352f, 0.352f, 0.352f, 1.000f);
			colors[(int) ImGuiCol.TabActive] = new Num.Vector4(0.195f, 0.195f, 0.195f, 1.000f);
			colors[(int) ImGuiCol.TabUnfocused] = new Num.Vector4(0.098f, 0.098f, 0.098f, 1.000f);
			colors[(int) ImGuiCol.TabUnfocusedActive] = new Num.Vector4(0.195f, 0.195f, 0.195f, 1.000f);
			colors[(int) ImGuiCol.PlotLines] = new Num.Vector4(0.469f, 0.469f, 0.469f, 1.000f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Num.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.PlotHistogram] = new Num.Vector4(0.586f, 0.586f, 0.586f, 1.000f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Num.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Num.Vector4(1.000f, 1.000f, 1.000f, 0.156f);
			colors[(int) ImGuiCol.DragDropTarget] = new Num.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.NavHighlight] = new Num.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.NavWindowingHighlight] = new Num.Vector4(1.000f, 0.391f, 0.000f, 1.000f);
			colors[(int) ImGuiCol.NavWindowingDimBg] = new Num.Vector4(0.000f, 0.000f, 0.000f, 0.586f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Num.Vector4(0.000f, 0.000f, 0.000f, 0.586f);

			style.ChildRounding = 4.0f;
			style.FrameBorderSize = 1.0f;
			style.FrameRounding = 2.0f;
			style.GrabMinSize = 7.0f;
			style.PopupRounding = 2.0f;
			style.ScrollbarRounding = 12.0f;
			style.ScrollbarSize = 13.0f;
			style.TabBorderSize = 1.0f;
			style.TabRounding = 0.0f;
			style.WindowRounding = 4.0f;
		}

		public static void LightGreenMiniDart()
		{
			var style = ImGui.GetStyle();
			var colors = style.Colors;

			style.WindowRounding = 2.0f; // Radius of window corners rounding. Set to 0.0f to have rectangular windows
			style.ScrollbarRounding = 3.0f; // Radius of grab corners rounding for scrollbar
			style.GrabRounding =
				2.0f; // Radius of grabs corners rounding. Set to 0.0f to have rectangular slider grabs.
			style.AntiAliasedLines = true;
			style.AntiAliasedFill = true;
			style.WindowRounding = 2;
			style.ChildRounding = 2;
			style.ScrollbarSize = 16;
			style.ScrollbarRounding = 3;
			style.GrabRounding = 2;
			style.ItemSpacing.X = 10;
			style.ItemSpacing.Y = 4;
			style.FramePadding.X = 6;
			style.FramePadding.Y = 4;
			style.Alpha = 1.0f;
			style.FrameRounding = 3.0f;

			colors[(int) ImGuiCol.Text] = new Num.Vector4(0.00f, 0.00f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.TextDisabled] = new Num.Vector4(0.60f, 0.60f, 0.60f, 1.00f);
			colors[(int) ImGuiCol.WindowBg] = new Num.Vector4(0.86f, 0.86f, 0.86f, 1.00f);

			//colors[(int)ImGuiCol.ChildWindowBg]         = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			colors[(int) ImGuiCol.ChildBg] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			colors[(int) ImGuiCol.PopupBg] = new Num.Vector4(0.93f, 0.93f, 0.93f, 0.98f);
			colors[(int) ImGuiCol.Border] = new Num.Vector4(0.71f, 0.71f, 0.71f, 0.08f);
			colors[(int) ImGuiCol.BorderShadow] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.04f);
			colors[(int) ImGuiCol.FrameBg] = new Num.Vector4(0.71f, 0.71f, 0.71f, 0.55f);
			colors[(int) ImGuiCol.FrameBgHovered] = new Num.Vector4(0.94f, 0.94f, 0.94f, 0.55f);
			colors[(int) ImGuiCol.FrameBgActive] = new Num.Vector4(0.71f, 0.78f, 0.69f, 0.98f);
			colors[(int) ImGuiCol.TitleBg] = new Num.Vector4(0.85f, 0.85f, 0.85f, 1.00f);
			colors[(int) ImGuiCol.TitleBgCollapsed] = new Num.Vector4(0.82f, 0.78f, 0.78f, 0.51f);
			colors[(int) ImGuiCol.TitleBgActive] = new Num.Vector4(0.78f, 0.78f, 0.78f, 1.00f);
			colors[(int) ImGuiCol.MenuBarBg] = new Num.Vector4(0.86f, 0.86f, 0.86f, 1.00f);
			colors[(int) ImGuiCol.ScrollbarBg] = new Num.Vector4(0.20f, 0.25f, 0.30f, 0.61f);
			colors[(int) ImGuiCol.ScrollbarGrab] = new Num.Vector4(0.90f, 0.90f, 0.90f, 0.30f);
			colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Num.Vector4(0.92f, 0.92f, 0.92f, 0.78f);
			colors[(int) ImGuiCol.ScrollbarGrabActive] = new Num.Vector4(1.00f, 1.00f, 1.00f, 1.00f);
			colors[(int) ImGuiCol.CheckMark] = new Num.Vector4(0.184f, 0.407f, 0.193f, 1.00f);
			colors[(int) ImGuiCol.SliderGrab] = new Num.Vector4(0.26f, 0.59f, 0.98f, 0.78f);
			colors[(int) ImGuiCol.SliderGrabActive] = new Num.Vector4(0.26f, 0.59f, 0.98f, 1.00f);
			colors[(int) ImGuiCol.Button] = new Num.Vector4(0.71f, 0.78f, 0.69f, 0.40f);
			colors[(int) ImGuiCol.ButtonHovered] = new Num.Vector4(0.725f, 0.805f, 0.702f, 1.00f);
			colors[(int) ImGuiCol.ButtonActive] = new Num.Vector4(0.793f, 0.900f, 0.836f, 1.00f);
			colors[(int) ImGuiCol.Header] = new Num.Vector4(0.71f, 0.78f, 0.69f, 0.31f);
			colors[(int) ImGuiCol.HeaderHovered] = new Num.Vector4(0.71f, 0.78f, 0.69f, 0.80f);
			colors[(int) ImGuiCol.HeaderActive] = new Num.Vector4(0.71f, 0.78f, 0.69f, 1.00f);
			colors[(int) ImGuiCol.Separator] = new Num.Vector4(0.39f, 0.39f, 0.39f, 1.00f);
			colors[(int) ImGuiCol.SeparatorHovered] = new Num.Vector4(0.14f, 0.44f, 0.80f, 0.78f);
			colors[(int) ImGuiCol.SeparatorActive] = new Num.Vector4(0.14f, 0.44f, 0.80f, 1.00f);
			colors[(int) ImGuiCol.ResizeGrip] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.00f);
			colors[(int) ImGuiCol.ResizeGripHovered] = new Num.Vector4(0.26f, 0.59f, 0.98f, 0.45f);
			colors[(int) ImGuiCol.ResizeGripActive] = new Num.Vector4(0.26f, 0.59f, 0.98f, 0.78f);
			colors[(int) ImGuiCol.PlotLines] = new Num.Vector4(0.39f, 0.39f, 0.39f, 1.00f);
			colors[(int) ImGuiCol.PlotLinesHovered] = new Num.Vector4(1.00f, 0.43f, 0.35f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogram] = new Num.Vector4(0.90f, 0.70f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.PlotHistogramHovered] = new Num.Vector4(1.00f, 0.60f, 0.00f, 1.00f);
			colors[(int) ImGuiCol.TextSelectedBg] = new Num.Vector4(0.26f, 0.59f, 0.98f, 0.35f);
			colors[(int) ImGuiCol.ModalWindowDimBg] = new Num.Vector4(0.20f, 0.20f, 0.20f, 0.35f);
			colors[(int) ImGuiCol.DragDropTarget] = new Num.Vector4(0.26f, 0.59f, 0.98f, 0.95f);
			colors[(int) ImGuiCol.NavHighlight] = colors[(int) ImGuiCol.HeaderHovered];
			colors[(int) ImGuiCol.NavWindowingHighlight] = new Num.Vector4(0.70f, 0.70f, 0.70f, 0.70f);
		}

		public static void HighContrast()
		{
			var style = ImGui.GetStyle();

			style.WindowRounding = 5.3f;
			style.FrameRounding = 2.3f;
			style.ScrollbarRounding = 0;

			style.Colors[(int) ImGuiCol.Text] = new Num.Vector4(0.90f, 0.90f, 0.90f, 0.90f);
			style.Colors[(int) ImGuiCol.TextDisabled] = new Num.Vector4(0.60f, 0.60f, 0.60f, 1.00f);
			style.Colors[(int) ImGuiCol.WindowBg] = new Num.Vector4(0.09f, 0.09f, 0.15f, 1.00f);
			style.Colors[(int) ImGuiCol.ChildBg] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			style.Colors[(int) ImGuiCol.PopupBg] = new Num.Vector4(0.05f, 0.05f, 0.10f, 0.85f);
			style.Colors[(int) ImGuiCol.Border] = new Num.Vector4(0.70f, 0.70f, 0.70f, 0.65f);
			style.Colors[(int) ImGuiCol.BorderShadow] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
			style.Colors[(int) ImGuiCol.FrameBg] = new Num.Vector4(0.00f, 0.00f, 0.01f, 1.00f);
			style.Colors[(int) ImGuiCol.FrameBgHovered] = new Num.Vector4(0.90f, 0.80f, 0.80f, 0.40f);
			style.Colors[(int) ImGuiCol.FrameBgActive] = new Num.Vector4(0.90f, 0.65f, 0.65f, 0.45f);
			style.Colors[(int) ImGuiCol.TitleBg] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.83f);
			style.Colors[(int) ImGuiCol.TitleBgCollapsed] = new Num.Vector4(0.40f, 0.40f, 0.80f, 0.20f);
			style.Colors[(int) ImGuiCol.TitleBgActive] = new Num.Vector4(0.00f, 0.00f, 0.00f, 0.87f);
			style.Colors[(int) ImGuiCol.MenuBarBg] = new Num.Vector4(0.01f, 0.01f, 0.02f, 0.80f);
			style.Colors[(int) ImGuiCol.ScrollbarBg] = new Num.Vector4(0.20f, 0.25f, 0.30f, 0.60f);
			style.Colors[(int) ImGuiCol.ScrollbarGrab] = new Num.Vector4(0.55f, 0.53f, 0.55f, 0.51f);
			style.Colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Num.Vector4(0.56f, 0.56f, 0.56f, 1.00f);
			style.Colors[(int) ImGuiCol.ScrollbarGrabActive] = new Num.Vector4(0.56f, 0.56f, 0.56f, 0.91f);
			style.Colors[(int) ImGuiCol.CheckMark] = new Num.Vector4(0.90f, 0.90f, 0.90f, 0.83f);
			style.Colors[(int) ImGuiCol.SliderGrab] = new Num.Vector4(0.70f, 0.70f, 0.70f, 0.62f);
			style.Colors[(int) ImGuiCol.SliderGrabActive] = new Num.Vector4(0.30f, 0.30f, 0.30f, 0.84f);
			style.Colors[(int) ImGuiCol.Button] = new Num.Vector4(0.48f, 0.72f, 0.89f, 0.49f);
			style.Colors[(int) ImGuiCol.ButtonHovered] = new Num.Vector4(0.50f, 0.69f, 0.99f, 0.68f);
			style.Colors[(int) ImGuiCol.ButtonActive] = new Num.Vector4(0.80f, 0.50f, 0.50f, 1.00f);
			style.Colors[(int) ImGuiCol.Header] = new Num.Vector4(0.30f, 0.69f, 1.00f, 0.53f);
			style.Colors[(int) ImGuiCol.HeaderHovered] = new Num.Vector4(0.44f, 0.61f, 0.86f, 1.00f);
			style.Colors[(int) ImGuiCol.HeaderActive] = new Num.Vector4(0.38f, 0.62f, 0.83f, 1.00f);
			style.Colors[(int) ImGuiCol.ResizeGrip] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.85f);
			style.Colors[(int) ImGuiCol.ResizeGripHovered] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.60f);
			style.Colors[(int) ImGuiCol.ResizeGripActive] = new Num.Vector4(1.00f, 1.00f, 1.00f, 0.90f);
			style.Colors[(int) ImGuiCol.PlotLines] = new Num.Vector4(1.00f, 1.00f, 1.00f, 1.00f);
			style.Colors[(int) ImGuiCol.PlotLinesHovered] = new Num.Vector4(0.90f, 0.70f, 0.00f, 1.00f);
			style.Colors[(int) ImGuiCol.PlotHistogram] = new Num.Vector4(0.90f, 0.70f, 0.00f, 1.00f);
			style.Colors[(int) ImGuiCol.PlotHistogramHovered] = new Num.Vector4(1.00f, 0.60f, 0.00f, 1.00f);
			style.Colors[(int) ImGuiCol.TextSelectedBg] = new Num.Vector4(0.00f, 0.00f, 1.00f, 0.35f);
			style.Colors[(int) ImGuiCol.ModalWindowDimBg] = new Num.Vector4(0.20f, 0.20f, 0.20f, 0.35f);
		}
	}
}