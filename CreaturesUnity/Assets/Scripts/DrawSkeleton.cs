using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DrawSkeleton : MonoBehaviour
{


	
	List<GameObject> kids = new List<GameObject>();
	
	List<GameObject> smoke = new List<GameObject>();

	public GameObject prefab;
	
	private float timeSinceUpdated = 0;
	
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	void BuildSkeleton()
	{
	
		foreach(Transform child in transform)
		{
			kids.Add(child.gameObject);
			
			GameObject t = (GameObject)Instantiate(prefab, child.transform.position, child.transform.rotation);
			
			t.transform.parent = child.transform;
			
			t.GetComponent<ParticleEmitter>().emit = false;
			
			smoke.Add(t);
		}
		
		kids.Sort(CompareByKinectState);
	}
	
	
	
	Transform getChildByName(string s){
		
		for(int i = 0; i < kids.Count; i++)
		{
			if(kids[i].name.Equals(s)) return kids[i].transform;
		}
		return null;
	}
	
	public void SkeletonUpdated(){
	
		timeSinceUpdated = 0f;
		
	}
	
	
	// Update is called once per frame
	void Update ()
	{
		if(timeSinceUpdated < 1.0f){
		
			GL.PushMatrix();
			GL.Begin(GL.LINES);
			
			GL.Color(Color.black);	
			
			for(int i = 1; i < kids.Count; i++)
			{
				smoke[i].GetComponent<ParticleEmitter>().emit = (kids[i].transform.position.magnitude < 1) ? false : true;
				
				if(kids[i].name.Equals("ShoulderLeft") ||
				   kids[i].name.Equals("ShoulderRight")||
				   kids[i].name.Equals("HipLeft")||
				   kids[i].name.Equals("HipRight")) continue;
				   
				Debug.DrawLine(kids[i-1].transform.position, kids[i].transform.position);	
				
				GL.Vertex(kids[i-1].transform.position);
				GL.Vertex(kids[i].transform.position);
				
				
	//			if(kids[i].name.Equals("WristLeft"))
	//			{
	//				handL.transform.parent = kids[i].transform;
	//			}
	//			
	//			if(kids[i].name.Equals("WristRight"))
	//			{
	//				handR.transform.parent = kids[i].transform;		
	//			}		
			}
			
			GL.End();	
			GL.PopMatrix();
		}
		else{
			for(int i = 1; i < smoke.Count; i++)
			{
				smoke[i].GetComponent<ParticleEmitter>().emit = false;	
			}
		}
		
		timeSinceUpdated += Time.deltaTime;
	}
	
	// Sorts the  List so the (AnimationPoolState -1) enum can be used as an index
	private static int CompareByKinectState(GameObject one, GameObject two)
    {
		int a = (int) System.Enum.Parse(typeof(KinectViaTcp.KinectJointID), one.transform.name);
		int b = (int) System.Enum.Parse(typeof(KinectViaTcp.KinectJointID), two.transform.name);
		
		return a.CompareTo(b);
    }
}

