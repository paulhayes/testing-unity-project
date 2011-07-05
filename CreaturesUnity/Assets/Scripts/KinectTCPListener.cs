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
    public static event UpdatedSkeletonDataHandler onUnityUpdatedSkeletonData;

	// Use this for initialization
	void Start () 
    {
        skeletonBuffer = new List<SkeletonData>();

        // Create a new Kinect Data receiver
        KinectDataReceiver dataReceiver = new KinectDataReceiver(IPAddress);
        dataReceiver.UpdatedSkeletonDataEvent += new KinectViaTcp.EventHandler(OnKinectUpdatedSkeletonData);
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}

    // Respond to the Kinect events
    private void OnKinectUpdatedSkeletonData(object sender, SkeletonData skeleton)
    {
        lock (skeletonBuffer)
        {
            skeletonBuffer.Add(skeleton);
            print("Updating skeleton");
        }
        //print("Possible data collision");
    }
}
