using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatureDemo : MonoBehaviour {

public enum Nature
{
		docile,
		aggressive,
	
}

public enum State
{
	sitting,
	standing,
	calling,
	moving,	
}
	
		// Creature attributes
        public int id;
		public string bioName;
		public string bioDescription;
		public Nature nature;  
        public double sizeMultiplier;

		private State currentState;
		private int currentTarget;


public float creatureInteractionDistanceFromScreen;
private Vector3 destination;
// variable to control time taken to travel between points
float duration = 1.0f;


public GUIText nameObject; 


    // Use this for initialization
	void Start () 
	{
		// Initialise this creature attributes
		currentState = State.sitting;
		
		//nameObject = GUIText.Find("NameText");
		
		//if( !nameObject )
		//{
			//print ("nameObject not found!");
			//enabled = false;
			//return;
		//}
		//nameObject.Text = bioName;

	}
	
	
    // Update is called once per frame
	void Update () 
	{
		
		animation.PlayQueued("walk");
		animation.PlayQueued("standScratch");
		animation.PlayQueued("sitCall");
	
	}
	

}
