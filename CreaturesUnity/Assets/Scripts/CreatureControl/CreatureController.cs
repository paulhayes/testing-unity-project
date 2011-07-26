using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatureController : MonoBehaviour
{
	
	public GameObject[] creatures;
	
	public List <GameObject>targets = new List<GameObject>();

	// Use this for initialization
	void Start ()
	{
		creatures = GameObject.FindGameObjectsWithTag("CREATURE");
	}
	
	void AddTarget(GameObject g){
		
		if(!targets.Contains(g)) targets.Add(g);
		
	}
	
	void RemoveTarget(GameObject g){
		
		if(targets.Contains(g)) targets.Remove(g);
		
	}	

	// Update is called once per frame
	void Update ()
	{
		
		for(int i =  0; i < creatures.Length; i++)
		{
			Debug.DrawLine(transform.position, creatures[i].transform.position, Color.blue);	
			
		}
		
		
		
		for(int i =  0; i < targets.Count; i++)
		{
			Debug.DrawLine(transform.position, targets[i].transform.position, Color.red);	
		}
	}
}

