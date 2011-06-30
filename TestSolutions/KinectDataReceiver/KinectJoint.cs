using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectViaTcp
{
    public class KinectJoint
    {
        public KinectJointID ID { get; set; }
        public Vector Position; 
        //public JointTrackingState TrackingState { get; set; }

        public KinectJoint(KinectJointID jointID, Vector position)
        {
            ID = jointID;
            Position = position;
        }
    }
}
