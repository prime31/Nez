using System;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
    class CoreWindow
    {
        string[] _textureFilters;
        float[] _frameRateArray = new float[100];
        int _frameRateArrayIndex = 0;

        public CoreWindow()
        {
            _textureFilters = Enum.GetNames( typeof( TextureFilter ) );
        }

        public void show( ref bool isOpen )
        {
            if( !isOpen )
                return;

            ImGui.SetNextWindowPos( new Num.Vector2( Screen.width - 300, Screen.height - 240 ), ImGuiCond.FirstUseEver );
            ImGui.SetNextWindowSize( new Num.Vector2( 300, 240 ), ImGuiCond.FirstUseEver );
            ImGui.Begin( "Nez Core", ref isOpen );
            drawSettings();
            ImGui.End();
        }

        void drawSettings()
        {
            _frameRateArray[_frameRateArrayIndex] = ImGui.GetIO().Framerate;
            _frameRateArrayIndex = ( _frameRateArrayIndex + 1 ) % _frameRateArray.Length;

            ImGui.PlotLines( "##hidelabel", ref _frameRateArray[0], _frameRateArray.Length, _frameRateArrayIndex, $"FPS: {ImGui.GetIO().Framerate:0}", 0, 60, new Num.Vector2( ImGui.GetContentRegionAvail().X, 50 ) );

            NezImGui.SmallVerticalSpace();

            if( ImGui.CollapsingHeader( "Core Settings", ImGuiTreeNodeFlags.DefaultOpen ) )
            {
                ImGui.Checkbox( "exitOnEscapeKeypress", ref Core.exitOnEscapeKeypress );
                ImGui.Checkbox( "pauseOnFocusLost", ref Core.pauseOnFocusLost );
                ImGui.Checkbox( "debugRenderEnabled", ref Core.debugRenderEnabled );
            }

            if( ImGui.CollapsingHeader( "Core.defaultSamplerState", ImGuiTreeNodeFlags.DefaultOpen ) )
            {
                #if !FNA
                ImGui.PushStyleVar( ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f );
                NezImGui.DisableNextWidget();
                #endif
                
                var currentTextureFilter = (int)Core.defaultSamplerState.Filter;
                if( ImGui.Combo( "Filter", ref currentTextureFilter, _textureFilters, _textureFilters.Length ) )
                    Core.defaultSamplerState.Filter = (TextureFilter)Enum.Parse( typeof( TextureFilter ), _textureFilters[currentTextureFilter] );
                
                #if !FNA
                ImGui.PopStyleVar();
                #endif
            }
            
        }
    
    }
}
