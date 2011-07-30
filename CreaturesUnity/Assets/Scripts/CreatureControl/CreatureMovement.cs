using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatureMovement : MonoBehaviour
{
	AnimationController ac;
	
	public AnimationPoolState currentState;
	
	public GameObject currentTarget;
	
	public bool isMoving = false;
	public bool firstMove = false;
	
    public List<Transform> waypoints = new List<Transform>();
	public Transform wp;
	
	
	
	public float creatureInteractionDistanceFromScreen;
    private Vector3 destination;

    #region move and rotation

    // Movement variables
    private float moveDuration = 1.0f;
    private float moveDistance;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private float moveStartTime;

    public float walkSpeed;
    public float runAtDistance;
    public float runSpeed;

    public float walkStopTime;

	// Rotation variables
    public float rotationDuration = 1.0f;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private float rotationStartTime;

    #endregion
	
	
	// Use this for initialization
	void Start ()
	{
		if(ac == null) ac = gameObject.GetComponent<AnimationController>();
		
 		startPoint = transform.position;
        moveStartTime = Time.time;

        startRotation = transform.rotation;
        rotationStartTime = Time.time;		
		
		
		// Build up the waypoints
        wp = GameObject.Find("waypoints").transform;
        foreach (Transform child in wp)
        {
            // add them to the waypoints array
            waypoints.Add(child);
			
			//Debug.Log("Added " + child.position);
        }
		
	}
	
	public void SetTarget(GameObject g){
		currentTarget = g;	
	}
	
	
	
	// Update is called once per frame
	void Update ()
	{
		currentState = ac.activeState;
		
		isMoving = (currentState == AnimationPoolState.WALK || currentState == AnimationPoolState.ATTACKWALK || currentState == AnimationPoolState.RUN ) ? true : false;
				
		Debug.DrawLine(startPoint, endPoint);
		
		if (isMoving)
        {
			if(firstMove){
				//MoveToRandomWaypoint(false);
				firstMove = false;
			}
			
            //UpdateMovement();
        }
	}
	
	
	public void NowWalking(){
		
		Debug.LogWarning("now walking");
		firstMove = true;
		
	}
	
	
	private void UpdateMovement()
    {
        // Check how much longer we need to move for...
        var travelTimeLeft = moveDuration - (Time.time - moveStartTime);

        var i = (Time.time - moveStartTime) / moveDuration;
		//i = Time.deltaTime;
        transform.position = Vector3.Lerp(startPoint, endPoint, i);

        // We have actually reached our destination so we need to change our state to standing
        //if (i >= 1)
        if (travelTimeLeft <= 0)
        {
            SendMessage("Arrived", SendMessageOptions.DontRequireReceiver);
        }
    }
	
	
    private bool MoveToRandomWaypoint(bool vacantWaypointFound)
    {
        // Break out of recursive
        if (vacantWaypointFound)
        {
            return true;
        }
        var controllerScript = GameObject.Find("Controller").GetComponent<CreatureController>();

        // Pick random waypoint
        var randomNumber = Random.Range(0, (waypoints.Count - 1));
        var waypoint = waypoints[randomNumber].position;

        foreach (var creature in controllerScript.creatures)
        {
            // Creature already at that waypoint
            if (creature.transform.position == waypoint)
            {
                return MoveToRandomWaypoint(false);
            }
        }
        // Vacant waypoint found
        MoveToNewLocation(waypoint);
        return true;

    }	
	
	
	private void MoveToNewLocation(Vector3 destination)
    {
		Debug.LogWarning("MoveToNewLocation");
		
        float distance = Vector3.Distance(transform.position, destination);

        // Setup the rotation...
        startRotation = transform.rotation;
        Vector3 relativePos = destination - transform.position;
        endRotation = Quaternion.LookRotation(relativePos);

        // Calculate durations based on distance to travel

        rotationStartTime = Time.time;

		//currentState = State.rotating;
        //Play start walk and end walk animations for the rotation
//        SetAnimationStart(standing2Walk_Anim);
//        //SetAnimationStartQueued(walk2Standing_Anim);
//
//        //
//        // setup the walk...
//        //
//

        //if (distance > someValue) -- Then we choose to run instead of walking

        // Setup the movement...
        moveDuration = distance / walkSpeed;
        // set our start point to our current position
		
		moveStartTime = Time.time;
        startPoint = transform.position;
        endPoint = destination;

    }
}
