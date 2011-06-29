using UnityEngine;
using System.Collections;

public class CreaturesControl : MonoBehaviour {

	// link to main kinect script
	//private kinectUser2Unity kinectScript;
	
	public OpenNIUserTracker userTracker;
	
	public GameObject[] creatures; 
	
	public int numberOfCreatures;
	
	//private int kinectEnabled;
	
	// Use this for initialization
	void Start () 
	{
		userTracker =  GameObject.Find("NITE").GetComponent<OpenNIUserTracker>();
		creatures = new GameObject[numberOfCreatures];
		
		// Connect the required number of creatures to their gameobjects
		for(int i=0; i < numberOfCreatures; i++)
		{
			var creature = GameObject.Find("Creature" + (i + 1) + "/creature");
			creatures[i] = creature;
		}
		
		//creatures[0].GetComponent<Creature>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		for(int i = 0; i < userTracker.users.Count; i++)
		{
			var userInfo = userTracker.users[i];
			
			if(userInfo.creatureAssigned == -1 )
			{
				
				// Need to assign a creature to the user
				// Find a creature that does not have a target user
				for(int j = 0; j < numberOfCreatures; j++)
				{
					// Ask creature if it wants to track a user
					var creatureScript = creatures[j].GetComponent<Creature>();
					//Debug.Log("Creature name is: " + creatureScript.bioName);
					
					if(creatureScript.TrackNewUser(userInfo.id))
					{
						userInfo.creatureAssigned = creatureScript.id;
						userTracker.users[i] = userInfo;
						break;
					}
				}
				//Debug.Log("Creature name is: " + creatures[i].GetComponent<Creature>().bioName);
			}
		}
	}
}
