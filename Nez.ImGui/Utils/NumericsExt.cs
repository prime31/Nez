using System;
using Microsoft.Xna.Framework;
using Num = System.Numerics;

namespace Nez.ImGuiTools
{
    /// <summary>
    /// helpers to convert to/from System.Numberics
    /// </summary>
    public static class NumericsExt
    {
        public static Vector2 toXNA( this Num.Vector2 self )
        {
            return new Vector2( self.X, self.Y );
        }

        public static Num.Vector2 toNumerics( this Vector2 self )
        {
            return new Num.Vector2( self.X, self.Y );
        }

        public static Vector3 toXNA( this Num.Vector3 self )
        {
            return new Vector3( self.X, self.Y, self.Z );
        }

        public static Num.Vector3 toNumerics( this Vector3 self )
        {
            return new Num.Vector3( self.X, self.Y, self.Z );
        }

        public static Vector4 toXNA( this Num.Vector4 self )
        {
            return new Vector4( self.X, self.Y, self.Z, self.W );
        }

        public static Num.Vector4 toNumerics( this Vector4 self )
        {
            return new Num.Vector4( self.X, self.Y, self.Z, self.W );
        }

        public static Num.Vector4 toNumerics( this Color self )
        {
            return new Num.Vector4( self.R / 255.0f, self.G / 255.0f, self.B / 255.0f, self.A / 255.0f );
        }

        public static Color toXNAColor( this Num.Vector4 self )
        {
            return new Color( self.X * 1.0f, self.Y * 1.0f, self.Z * 1.0f, self.W * 1.0f );
        }
    }
}
