using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectViaTcp
{
    /// <summary>
    /// A joint in the skeleton detected by Kinect
    /// </summary>
    [Serializable]
    public class KinectJoint
    {
        public KinectJointID ID { get; set; }
        public Vector Position; 

        public KinectJoint(KinectJointID jointID, Vector position)
        {
            ID = jointID;
            Position = position;
        }
    }
}
