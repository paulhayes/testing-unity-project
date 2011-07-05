using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectViaTcp
{
    [Serializable]
    public class SkeletonData
    {
        public SkeletonState State;
        public List<KinectJoint> Joints;
        public Vector Position;
        public int UserIndex;
        public byte[] UserImage = new byte[320 * 240 * 4];

        public SkeletonData(int userId)
        {
            UserIndex = userId;
            Joints = new List<KinectJoint>();

            // Create the joints
            Joints.Add(new KinectJoint(KinectJointID.HipCenter, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.Spine, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.ShoulderCenter, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.Head, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.ShoulderLeft, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.ElbowLeft, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.WristLeft, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.HandLeft, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.ShoulderRight, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.ElbowRight, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.WristRight, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.HandRight, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.HipLeft, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.KneeLeft, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.AnkleLeft, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.FootLeft, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.HipRight, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.KneeRight, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.AnkleRight, new Vector(0, 0, 0)));
            Joints.Add(new KinectJoint(KinectJointID.FootRight, new Vector(0, 0, 0)));
        }

        public KinectJoint GetJoint(KinectJointID jointToUpdate)
        {
            foreach(KinectJoint joint in Joints)
            {
                if (joint.ID == jointToUpdate)
                {
                    return joint;
                }
            }
            return null;
        }

        public SkeletonData(SkeletonData copy)
        {
            UserIndex = copy.UserIndex;
            Joints = new List<KinectJoint>();
            foreach (KinectJoint joint in copy.Joints)
            {
                Joints.Add(new KinectJoint(joint.ID, joint.Position));
            }
        }
    }
}
