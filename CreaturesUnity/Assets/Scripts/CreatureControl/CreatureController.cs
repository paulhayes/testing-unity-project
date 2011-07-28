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
		
		Transform wp = GameObject.Find("waypoints").transform;
		
        foreach (Transform child in wp)
        {
            targets.Add(child.gameObject);
        }
		
		
		
		InvokeRepeating("MoveYouFucker", 5.0f, 50.0f);
		
	}
	
	void AddTarget(GameObject g){
		
		if(!targets.Contains(g)) targets.Add(g);
		
	}
	
	void RemoveTarget(GameObject g){
		
		if(targets.Contains(g)) targets.Remove(g);
		
	}
	
	
	public void UpdatedSkeletonPosition(float x){
	
		float dist = 1000000.0f;
		
		int index = 0;
		
		for(int i = 0; i < creatures.Length; i++){
			
			float val = Vector2.Distance(new Vector2(x, 0), new Vector2(creatures[i].transform.position.x, 0));
			
			
			Debug.Log(val);
			
			
			if( val <= dist )
			{	
				dist = val;
				index = i;	
			}
		}
		
		if(dist < 2)
		{
			MoveYouFucker(index);
		}
		
	}
	
	
	void MoveYouFucker(){
		FirstBehaviourTree fbt = (FirstBehaviourTree)creatures[Random.Range(0, creatures.Length)].GetComponent<FirstBehaviourTree>();
		fbt.MoveToNewTarget(targets[Random.Range(0, targets.Count)]);
		
	}
	
	void MoveYouFucker(int number){
		FirstBehaviourTree fbt = (FirstBehaviourTree)creatures[number].GetComponent<FirstBehaviourTree>();
		fbt.MoveToNewTarget(targets[Random.Range(0, targets.Count)]);
		
	}

	// Update is called once per frame
	void Update ()
	{
		
		for(int i =  0; i < creatures.Length; i++)
		{
			Debug.DrawLine(transform.position, creatures[i].transform.position, Color.blue);	
			
		}
		
		
		for(int i =  1; i < targets.Count; i++)
		{
			Debug.DrawLine(targets[i-1].transform.position, targets[i].transform.position, Color.red);	
		}
		
		
		
		
	}
}

