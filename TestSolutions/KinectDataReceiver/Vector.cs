using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectViaTcp
{
    /// <summary>
    /// Vector contains x,y,z position information.
    /// w is the confidence - 0-1
    /// </summary>
    [Serializable]
    public struct Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector(float x, float y, float z) :this()
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
