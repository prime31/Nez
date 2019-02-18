using System;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
    static class CoreWindow
    {
        static string[] _textureFilters;
        static int _currentTextureFilter;
        static string[] _textureAddressModes;
        static int _addressU, _addressV, _addressW;

        static CoreWindow()
        {
            _textureFilters = Enum.GetNames( typeof( TextureFilter ) );
            _currentTextureFilter = Array.IndexOf( _textureFilters, Core.defaultSamplerState.Filter.ToString() );

            _textureAddressModes = Enum.GetNames( typeof( TextureAddressMode ) );
            _addressU = Array.IndexOf( _textureAddressModes, Core.defaultSamplerState.AddressU.ToString() );
            _addressV = Array.IndexOf( _textureAddressModes, Core.defaultSamplerState.AddressV.ToString() );
            _addressW = Array.IndexOf( _textureAddressModes, Core.defaultSamplerState.AddressW.ToString() );
        }

        public static void show( ref bool isOpen )
        {
            if( !isOpen )
                return;

            ImGui.SetNextWindowPos( new Num.Vector2( 10, 10 ), ImGuiCond.FirstUseEver );
            ImGui.SetNextWindowSize( new Num.Vector2( 350, Screen.height / 2 ), ImGuiCond.FirstUseEver );
            ImGui.Begin( "Nez Core", ref isOpen );
            drawSettings();
            ImGui.End();
        }


        static void drawSettings()
        {
            ImGui.Text( "Core Settings" );
            ImGui.Checkbox( "exitOnEscapeKeypress", ref Core.exitOnEscapeKeypress );
            ImGui.Checkbox( "pauseOnFocusLost", ref Core.pauseOnFocusLost );
            ImGui.Checkbox( "debugRenderEnabled", ref Core.debugRenderEnabled );
            ImGui.Separator();

            ImGui.Text( "Core.defaultSamplerState" );
            if( ImGui.Combo( "Filter", ref _currentTextureFilter, _textureFilters, _textureFilters.Length ) )
                Core.defaultSamplerState.Filter = (TextureFilter)Enum.Parse( typeof( TextureFilter ), _textureFilters[_currentTextureFilter] );

            var anisotropy = Core.defaultSamplerState.MaxAnisotropy;
            if( ImGui.InputInt( "MaxAnisotropy", ref anisotropy ) )
                Core.defaultSamplerState.MaxAnisotropy = anisotropy;

            if( ImGui.Combo( "AddressU", ref _addressU, _textureAddressModes, _textureAddressModes.Length ) )
                Core.defaultSamplerState.AddressU = (TextureAddressMode)Enum.Parse( typeof( TextureAddressMode ), _textureAddressModes[_addressU] );

            if( ImGui.Combo( "AddressV", ref _addressV, _textureAddressModes, _textureAddressModes.Length ) )
                Core.defaultSamplerState.AddressV = (TextureAddressMode)Enum.Parse( typeof( TextureAddressMode ), _textureAddressModes[_addressV] );

            if( ImGui.Combo( "AddressW", ref _addressW, _textureAddressModes, _textureAddressModes.Length ) )
                Core.defaultSamplerState.AddressW = (TextureAddressMode)Enum.Parse( typeof( TextureAddressMode ), _textureAddressModes[_addressW] );
        }
    }
}
