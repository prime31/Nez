using System;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
    static class CoreWindow
    {
        static string[] _textureFilters;
        static string[] _textureAddressModes;
        static float[] _frameRateArray = new float[100];
        static int _frameRateArrayIndex = 0;

        static CoreWindow()
        {
            _textureFilters = Enum.GetNames( typeof( TextureFilter ) );
            _textureAddressModes = Enum.GetNames( typeof( TextureAddressMode ) );
        }

        public static void show( ref bool isOpen )
        {
            if( !isOpen )
                return;

            ImGui.SetNextWindowPos( new Num.Vector2( Screen.width - 315, Screen.height - 315 ), ImGuiCond.FirstUseEver );
            ImGui.SetNextWindowSize( new Num.Vector2( 315, 315 ), ImGuiCond.FirstUseEver );
            ImGui.Begin( "Nez Core", ref isOpen );
            drawSettings();
            ImGui.End();
        }


        static void drawSettings()
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
                var currentTextureFilter = (int)Core.defaultSamplerState.Filter;
                if( ImGui.Combo( "Filter", ref currentTextureFilter, _textureFilters, _textureFilters.Length ) )
                    Core.defaultSamplerState.Filter = (TextureFilter)Enum.Parse( typeof( TextureFilter ), _textureFilters[currentTextureFilter] );

                var anisotropy = Core.defaultSamplerState.MaxAnisotropy;
                if( ImGui.InputInt( "MaxAnisotropy", ref anisotropy ) )
                    Core.defaultSamplerState.MaxAnisotropy = anisotropy;

                var addressU = (int)Core.defaultSamplerState.AddressU;
                if( ImGui.Combo( "AddressU", ref addressU, _textureAddressModes, _textureAddressModes.Length ) )
                    Core.defaultSamplerState.AddressU = (TextureAddressMode)addressU;

                var addressV = (int)Core.defaultSamplerState.AddressV;
                if( ImGui.Combo( "AddressV", ref addressV, _textureAddressModes, _textureAddressModes.Length ) )
                    Core.defaultSamplerState.AddressV = (TextureAddressMode)addressV;

                var addressW = (int)Core.defaultSamplerState.AddressW;
                if( ImGui.Combo( "AddressW", ref addressW, _textureAddressModes, _textureAddressModes.Length ) )
                    Core.defaultSamplerState.AddressW = (TextureAddressMode)addressW;
            }
        }
    }
}
