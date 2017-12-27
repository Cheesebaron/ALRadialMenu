using System;

namespace DK.Ostebaronen.Touch.RadialMenu
{
    /// <summary>
    /// Class to represent angle along with conversion helpers
    /// </summary>
    public struct Angle
    {
        /// <summary>
        /// Angle in degrees
        /// </summary>
        public float Degrees { get; set; }

        /// <summary>
        /// Angle in radians
        /// </summary>
        public float Radians => Degrees* ((float) Math.PI / 180.0f);
    }
}