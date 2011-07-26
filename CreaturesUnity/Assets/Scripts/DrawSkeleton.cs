using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DrawSkeleton : MonoBehaviour
{

//public enum KinectJointID {
//
// 	HipCenter,
// 	Spine,
// 	ShoulderCenter,
// 	Head,
// 	ShoulderLeft,
// 	ElbowLeft,
// 	WristLeft,
// 	HandLeft,
// 	ShoulderRight,
// 	ElbowRight,
// 	WristRight,
// 	HandRight,
// 	HipLeft,
// 	KneeLeft,
// 	AnkleLeft,
// 	FootLeft,
// 	HipRight,
// 	KneeRight,
// 	AnkleRight,
// 	FootRight
//}
	
	List<GameObject> kids = new List<GameObject>();
	
	GameObject handR;
	GameObject handL;
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	void BuildSkeleton()
	{
	
		foreach(Transform child in transform)
		{
			kids.Add(child.gameObject);
		}
		
		kids.Sort(CompareByKinectState);
		
		handR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		handL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		
		handL.transform.parent = getChildByName("WristLeft");
		handR.transform.parent = getChildByName("WristRight");	
	}
	
	
	
	Transform getChildByName(string s){
		
		for(int i = 0; i < kids.Count; i++)
		{
			if(kids[i].name.Equals(s)) return kids[i].transform;
		}
		return null;
	}
	
	
	// Update is called once per frame
	void Update ()
	{
		for(int i = 1; i < kids.Count; i++)
		{
			if(kids[i].name.Equals("ShoulderLeft") ||
			   kids[i].name.Equals("ShoulderRight")||
			   kids[i].name.Equals("HipLeft")||
			   kids[i].name.Equals("HipRight")) continue;
			   
			Debug.DrawLine(kids[i-1].transform.position, kids[i].transform.position);	
		}
	}
	
	// Sorts the  List so the (AnimationPoolState -1) enum can be used as an index
	private static int CompareByKinectState(GameObject one, GameObject two)
    {
		int a = (int) System.Enum.Parse(typeof(KinectViaTcp.KinectJointID), one.transform.name);
		int b = (int) System.Enum.Parse(typeof(KinectViaTcp.KinectJointID), two.transform.name);
		
		return a.CompareTo(b);
    }
}

