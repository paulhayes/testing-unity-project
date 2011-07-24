using UnityEngine;
using System.Collections;

public class CreatureController : MonoBehaviour
{
	
	GameObject[] creatures;

	// Use this for initialization
	void Start ()
	{
		creatures = GameObject.FindGameObjectsWithTag("CREATURE");
		
	}

	// Update is called once per frame
	void Update ()
	{
		
		for(int i =  0; i < creatures.Length; i++)
		{
			Debug.DrawLine(transform.position, creatures[i].transform.position);	
			
		}
	}
}

