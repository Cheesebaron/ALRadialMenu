using System;

namespace DK.Ostebaronen.Touch.RadialMenu
{
    internal struct Angle
    {
        public float Degrees { get; set; }
        public float ToRadians() { return Degrees * ((float)Math.PI / 180.0f); }
    }
}