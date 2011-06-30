using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KinectViaTcp
{
    public class KinectJointsCollection : IEnumerable
    {
       

        //public KinectJoint this[KinectJointID i] { get; set; }

        #region Private Members

//private readonly string _id;

private ArrayList _joints;

#endregion

#region Properties

public int Count { get { return _joints.Count; } }

#endregion

#region Constructors

public KinectJointsCollection(string id)
{
//_id = id;

_joints = new ArrayList();

}

#endregion


        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return (_joints as IEnumerable).GetEnumerator();
        }

        #endregion
    }
}
