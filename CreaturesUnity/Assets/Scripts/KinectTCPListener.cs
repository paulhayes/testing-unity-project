using KinectViaTcp;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinectTCPListener : MonoBehaviour 
{
    public string IPAddress = "127.0.0.1"; // Default is loopback

    public List<SkeletonData> skeletonBuffer;
    public bool usingBuffer = false;

    public delegate void UpdatedSkeletonDataHandler(SkeletonData skeleton);
    //public static event UpdatedSkeletonDataHandler onUnityUpdatedSkeletonData;
	
	KinectDataReceiver dataReceiver;
	
	
	// Use this for initialization
	void Awake() 
    {
        skeletonBuffer = new List<SkeletonData>();

        // Create a new Kinect Data receiver
        dataReceiver = new KinectDataReceiver(IPAddress);
		dataReceiver.UpdatedSkeletonDataEvent += delegate(object sender, SkeletonData e) {};
        dataReceiver.UpdatedSkeletonDataEvent += new KinectViaTcp.EventHandler(OnKinectUpdatedSkeletonData);
		
	}


    // Respond to the Kinect events
    public void OnKinectUpdatedSkeletonData(object sender, SkeletonData skeleton)
    {
        lock (skeletonBuffer)
        {
            skeletonBuffer.Add(skeleton);
            //Debug.Log("Updating skeleton");
        }
        //print("Possible data collision");
    }
	
	public void OnApplicationQuit(){
		dataReceiver.UpdatedSkeletonDataEvent -= new KinectViaTcp.EventHandler(OnKinectUpdatedSkeletonData);
	}
	
	
}
