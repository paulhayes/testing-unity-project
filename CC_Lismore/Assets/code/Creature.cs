using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Creature : MonoBehaviour
{


    #region Creature Attributes

    // Creature attributes
    public int id;
    public string bioName;
    public string bioDescription;
    public Nature nature;
    public float sizeMultiplier;

    public State currentState;
    public int currentTarget;

    public enum Nature
    {
        docile,
        aggressive,
    }


    public enum State
    {
        standingIdle,
        standingCalling,
        sittingIdle,
        sittingCalling,
        rotating,
        moving,
    }

    #endregion

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


    private string currentPlayingAnimation;

    // Rotation variables
    public float rotationDuration = 1.0f;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private float rotationStartTime;

    #endregion

    private Transform userTransform;

    // array to hold waypoint locations
    private List<Transform> waypoints = new List<Transform>();
    private Transform wp;

    /*
    * List of Animations
    */
    #region Animations
    // Move + Loop
    private string walk_Anim = "walk";
    private string walkLookAround_Anim = "walkLookAround";
    private string run_Anim = "run";

    // Move + End
    private string walk2Standing_Anim = "walk2Standing";
    private string run2Standing_Anim = "run2Standing";

    // Move + Start
    private string standing2Run_Anim = "standing2Run";
    private string standing2Walk_Anim = "standing2Walk";

    // Sitting + Interaction
    private string sittingLookLarge_Anim = "sittingLookLarge";
    private string sittingLookSmall_Anim = "sittingLookSmall";

    // Sitting + Transition
    private string sitting2Standing_Anim = "sitting2Standing";

    // Sitting + Idle
    private string sittingBreathe_Anim = "sittingBreathe";
    private string sittingOpenSmall_Anim = "sittingOpenSmall";
    private string sittingPrune_Anim = "sittingPrune";

    // Sitting + Call
    private string sittingCall_Anim = "sittingCall";

    // Standing + Interaction
    private string standing2LookLeft_Anim = "standing2LookLeft";
    private string standing2LookRight_Anim = "standing2LookRight";

    private string lookLeftBreathe_Anim = "lookLeftBreathe";
    private string lookLeft2Standing_Anim = "lookLeft2Standing";
    private string lookLeftLookAround_Anim = "lookLeftLookAround";
    private string lookLeftMouthOpen_Anim = "lookLeftMouthOpen";

    private string lookRight2Standing_Anim = "lookRight2Standing";
    private string lookRightBreathe_Anim = "lookRightBreathe";
    private string lookRightLookAround_Anim = "lookRightLookAround";
    private string lookRightMouthOpen_Anim = "lookRightMouthOpen";

    // Standing + Transition
    private string standing2Sit_Anim = "standing2Sit";

    // Standing + Idle
    private string standingBreathe_Anim = "standingBreathe";
    private string standingBugSnap_Anim = "standingBugSnap";
    private string standingForage_Anim = "standingForage";
    private string standingLookAround_Anim = "standingLookAround";
    private string standingOpenMouth_Anim = "standingOpenMouth";
    private string standingScratch_Anim = "standingScratch";
    private string standingShiftWeight_Anim = "standingShiftWeight";

    // Standing + Call
    private string standingCommunicate1_Anim = "standingCommunicate1";
    private string standingCommunicate2_Anim = "standingCommunicate2";

    #endregion

    #region Animation matched Sounds

    // List of Sounds matched to sounds
    public AudioClip walk_Snd;
    public AudioClip walk2Stand_Snd;
    public AudioClip walkLookAround_Snd;

    public AudioClip lookLeft2Breathe_Snd;
    public AudioClip lookLeft2Standing_Snd;
    public AudioClip lookLeftLookAround_Snd;
    public AudioClip lookLeftMouthOpen_Snd;

    public AudioClip lookRight2Standing_Snd;
    public AudioClip lookRightBreathe_Snd;
    public AudioClip lookRightLookAround_Snd;
    public AudioClip lookRightMouthOpen_Snd;

    public AudioClip run_Snd;
    public AudioClip run2Standing_Snd;

    public AudioClip sitting2Standing_Snd;
    public AudioClip sittingBreathe_Snd;
    public AudioClip sittingCall_Snd;
    public AudioClip sittingLookLarge_Snd;
    public AudioClip sittingLookSmall_Snd;
    public AudioClip sittingOpenSmall_Snd;
    public AudioClip sittingPrune_Snd;

    public AudioClip standing2LookLeft_Snd;
    public AudioClip standing2LookRight_Snd;

    public AudioClip standing2Run_Snd;
    public AudioClip standing2Sit_Snd;
    public AudioClip standing2Walk_Snd;

    public AudioClip standingBreathe_Snd;
    public AudioClip standingBugSnap_Snd;
    public AudioClip standingCommunicate1_Snd;
    public AudioClip standingCommunicate2_Snd;
    public AudioClip standingForage_Snd;
    public AudioClip standingLookAround_Snd;

    public AudioClip standingOpenMouth_Snd;
    public AudioClip standingScratch_Snd;
    public AudioClip standingShiftWeight_Snd;

    #endregion

    public bool animationPlaying;

    #region Unity Start & Update

    /// <summary>
    /// Initialisation called by Unity
    /// </summary>
    void Start()
    {
        // Initialise this creature attributes
        currentState = State.sittingIdle;
        currentTarget = -1;

        startPoint = transform.position;
        moveStartTime = Time.time;

        startRotation = transform.rotation;
        rotationStartTime = Time.time;

        userTransform = new GameObject().transform;
        userTransform.name = "userForCreature";

        // Build up the waypoints
        wp = GameObject.Find("waypoints").transform;
        foreach (Transform child in wp)
        {
            // add them to the waypoints array
            waypoints.Add(child);
        }

    }



    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // Check at start of frame is current animation is playing...
        //animation.IsPlaying
        
        // If we are moving/rotating then update position and animation if required
        // Rotations are performed before movement to ensure creature moves forward
        if (currentState == State.rotating)
        {
            UpdateRotation();
        }

        else if (currentState == State.moving)
        {
            UpdateMovement();

        }

        // If we are already doing something then dont interact or idle
        else if (animation.isPlaying)
        {
            return;
        }

        // If creature is interacting with a user then act appropriately
        else if (currentTarget != -1)
        {
            UpdateInteraction();
        }
        else
        {
            // Otherwise perform a new idle action
            UpdateIdle();
        }
    }

    #endregion

    /// <summary>
    /// Updates the Creatures rotation
    /// </summary>
    private void UpdateRotation()
    {
        // Check how much longer we need to rotate for...
        var i = (Time.time - rotationStartTime) / rotationDuration;
        transform.rotation = Quaternion.Slerp(startRotation, endRotation, i);
        // We have actually reached our destination so we need to change our state to standing
        if (i >= 1)
        {
            // Finished rotating now start moving if we need to...
            moveStartTime = Time.time;
            currentState = State.moving;

            // Start moving animation -- TODO: need to decide whether to run or walk??
            SetAnimationStart(standing2Walk_Anim);
            animationPlaying = true;
        }
    }

    private void UpdateMovement()
    {
        // Check how much longer we need to move for...
        var travelTimeLeft = moveDuration - (Time.time - moveStartTime);

        var i = (Time.time - moveStartTime) / moveDuration;
        transform.position = Vector3.Lerp(startPoint, endPoint, i);

        // We have actually reached our destination so we need to change our state to standing
        //if (i >= 1)
        if (travelTimeLeft <= 0)
        {
            currentState = State.standingIdle;
        }
        else if (travelTimeLeft <= walkStopTime)
        {
            // Almost at destination so need to change to stand...
            SetAnimationStart(walk2Standing_Anim);
            animationPlaying = true;

            //TODO: Check if we have finished an animation so begin a new one (base this on estimate of how much time we have left to travel
            // Also need to note whether we are running or walking???
        }
        else
        {
            if (!animation.isPlaying)
            {
                // Current walk cycle has finished so start a new one
                var randomNumber = Random.Range(1, 5);
                if (randomNumber <= 2)
                {
                    SetAnimationStart(walkLookAround_Anim);
                }
                else
                {
                    SetAnimationStart(walk_Anim);
                }
                animationPlaying = true;

            }
        }
    }

    private void UpdateInteraction()
    {

        //Debug.Log("Creature interacting.");
        var userTracker = GameObject.Find("NITE").GetComponent<OpenNIUserTracker>();
        var userInfo = userTracker.GetUser(currentTarget);

        if (userInfo.id != -1)
        {
            // Check for jumping or ducking...
            if (userInfo.lastVerticalDisplacement < userInfo.initialVerticalDisplacement)
            {
                // Get creature to sit down
                if (currentState == State.standingIdle)
                {
                    SetAnimationStart(standing2Sit_Anim);
                    currentState = State.sittingIdle;
                }
            }
            else if (userInfo.lastVerticalDisplacement > userInfo.initialVerticalDisplacement)
            {
                // Get creature to call out
                if (currentState == State.standingIdle)
                {
                    SetAnimationStart(standingCommunicate2_Anim);
                    currentState = State.standingIdle;
                }
            }

            // Retrieve kinectSpace and update the Unity Space
            var kinectSpace = new Vector3(userInfo.lastHorizontalDisplacement, 0, 0);
            UpdateUserTransform(kinectSpace);

            // If not 'within' range (3m) of the user than move towards user
            var distance = Vector3.Distance(transform.position, userTransform.position);
            // Determine which side of the user the creature is
            // TODO...
            var difference = userTransform.position - transform.position;
            if (distance >= 1)
            {
                Vector3 newPosition = userTransform.position;
                if(difference.x < 0)
                {
                    newPosition -= new Vector3(0.5f, 0.0f, 0.0f);
                }
                else
                {
                    newPosition += new Vector3(0.5f, 0.0f, 0.0f);
                }
                MoveToNewLocation(newPosition);
                // Rotate about 45 degrees -- just to look good
            }
            else
            {
                // Perform an interaction...
                if (currentState == State.sittingIdle)
                {
                    PlayRandomSittingInteraction();
                }
                else if (currentState == State.standingIdle)
                {
                    if (difference.x < 0) // Creature is on the left side
                    {
                        PlayRandomLeftStandingInteraction();
                    }
                    else
                    {
                        PlayRandomRightStandingInteraction();
                    }
                }
            }
            return;
        }
        else
        {
            // Can no longer find user so remove it and do something else
            currentTarget = -1;
        }
    }


    private void UpdateIdle()
    {
        // Pick a random state to move to...
        var randomNumber = Random.Range(1, 100);

        if (currentState == State.standingIdle)
        {
            if (randomNumber < 2)
            {
                // Call for other creatures
                PlayRandomStandingCall();
            }
            else if (randomNumber < 10)
            {
                // Move to random waypoint
                MoveToRandomWaypoint(false);

            }
            else if (randomNumber < 20)
            {
                // Switch to sitting state
                SetAnimationStart(standing2Sit_Anim);
                currentState = State.sittingIdle;
                animationPlaying = true;
            }
            else
            {
                // Most likely to play another standing idle
                PlayRandomStandingIdle();
            }
            return;
        }
        else if (currentState == State.standingCalling)
        {
            // Do anything special??
            // Otherwise switch to normal standing
            currentState = State.standingIdle;
        }
        else if (currentState == State.sittingIdle)
        {
            if (randomNumber < 4)
            {
                // Call for other creatures
                PlayRandomSittingCall();
            }
            else if (randomNumber < 15)
            {
                // Switch to standing state
                SetAnimationStart(sitting2Standing_Anim);
                currentState = State.standingIdle;
                animationPlaying = true;
            }
            else
            {
                // Most likely to play another sitting idle
                PlayRandomSittingIdle();
            }
            return;
        }
        else if (currentState == State.sittingCalling)
        {
            // Do anything special??
            // Otherwise switch to normal standing
            currentState = State.sittingIdle;
        }
    }

    #region Random animation/move Methods

    private void PlayRandomLeftStandingInteraction()
    {
        var randomNumber = Random.Range(2, 5);
        switch (randomNumber)
        {
                //won't happen!!!!!!
            case 1:
                {
                    SetAnimationStart(standing2LookLeft_Anim);
                    break;
                }
            case 2:
                {
                    SetAnimationStart(lookLeftBreathe_Anim);
                    break;
                }
            case 3:
               // {
                  //  animation.CrossFade(lookLeft2Standing_Anim);
                  //  break;
                //}
            case 4: 
                //{
                    //SetAnimationStart(lookLeftLookAround_Anim);
                    //break;
                //}
            case 5:
                {
                    SetAnimationStart(lookLeftMouthOpen_Anim);
                    break;
                }
        }
        animationPlaying = true;      
    }

    private void PlayRandomRightStandingInteraction()
    {
        var randomNumber = Random.Range(1, 5);
        switch (randomNumber)
        {
            case 1:
                {
                    SetAnimationStart(standing2LookRight_Anim);
                    break;
                }
            case 2:
                {
                    SetAnimationStart(lookRight2Standing_Anim);
                    break;
                }
            case 3:
                {
                    SetAnimationStart(lookRightBreathe_Anim);
                    break;
                }
            case 4:
                {
                    SetAnimationStart(lookRightLookAround_Anim);
                    break;
                }
            case 5:
                {
                    SetAnimationStart(lookRightMouthOpen_Anim);
                    break;
                }
        }
        animationPlaying = true;
    }

    private void PlayRandomSittingInteraction()
    {
        var randomNumber = Random.Range(1, 2);
        if (randomNumber == 1)
        {
            SetAnimationStart(sittingLookLarge_Anim);
        }
        else
        {
            SetAnimationStart(sittingLookSmall_Anim);
        }
        animationPlaying = true;
    }

    private void PlayRandomStandingIdle()
    {
        var randomNumber = Random.Range(1, 10);

        switch (randomNumber)
        {
            case 1:
                {
                    SetAnimationStart(standingBugSnap_Anim);
                    break;
                }
            case 2:
                {
                    SetAnimationStart(standingForage_Anim);
                    break;
                }
            case 3:
                {
                    SetAnimationStart(standingLookAround_Anim);
                    break;
                }
            case 4:
                {
                    SetAnimationStart(standingOpenMouth_Anim);
                    break;
                }
            case 5:
                {
                    SetAnimationStart(standingScratch_Anim);
                    break;
                }
            case 6:
                {
                    SetAnimationStart(standingShiftWeight_Anim);
                    break;
                }

            default:
                {
                    SetAnimationStart(standingOpenMouth_Anim);
                    //SetAnimationStart(standingBreathe_Anim); // TODO: Fix Breathe animation
                    break;
                }
        }
        currentState = State.standingIdle;
        animationPlaying = true;
    }

    private void PlayRandomSittingIdle()
    {
        var randomNumber = Random.Range(1, 10);

        switch (randomNumber)
        {
            case 1:
            case 2:
            case 3:
                {
                    SetAnimationStart(sittingOpenSmall_Anim);
                    break;
                }
            case 4:
            case 5:
                {
                    SetAnimationStart(sittingPrune_Anim);
                    break;
                }
            default:
                {
                    SetAnimationStart(sittingBreathe_Anim);
                    break;
                }
        }
        currentState = State.sittingIdle;
        animationPlaying = true;
    }

    private void PlayRandomStandingCall()
    {
        var randomNumber = Random.Range(1, 10);
        switch (randomNumber)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                {
                    SetAnimationStart(standingCommunicate2_Anim);
                    break;
                }
            default:
                {
                    SetAnimationStart(standingCommunicate1_Anim);
                    break;
                }
        }
        currentState = State.standingCalling;
        animationPlaying = true;
    }

    private void PlayRandomSittingCall()
    {
        SetAnimationStart(sittingCall_Anim);
        currentState = State.sittingCalling;
        animationPlaying = true;
    }


    private bool MoveToRandomWaypoint(bool vacantWaypointFound)
    {
        // Break out of recursive
        if (vacantWaypointFound)
        {
            return true;
        }
        var controllerScript = GameObject.Find("CreaturesController").GetComponent<CreaturesControl>();

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


    #endregion

    // Move the creature to a new location
    // TODO: Update this to check creatures current rotation and rotation at destination and rotate the creature as required (play appropriate animation)
    private void MoveToNewLocation(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);

        // Setup the rotation...
        startRotation = transform.rotation;
        Vector3 relativePos = destination - transform.position;
        endRotation = Quaternion.LookRotation(relativePos);

        // Calculate durations based on distance to travel

        rotationStartTime = Time.time;
        currentState = State.rotating;
        // Play start walk and end walk animations for the rotation
        SetAnimationStart(standing2Walk_Anim);
        //SetAnimationStartQueued(walk2Standing_Anim);

        //
        // setup the walk...
        //


        //if (distance > someValue) -- Then we choose to run instead of walking

        // Setup the movement...
        moveDuration = distance / walkSpeed;
        // set our start point to our current position
        startPoint = transform.position;
        endPoint = destination;

    }

    /// <summary>
    /// Updates the User Transform from the Kinect Space to Unity Space
    /// </summary>
    /// <param name="kinectSpace"></param>
    private void UpdateUserTransform(Vector3 kinectSpace)
    {
        //Gizmos.color = Color.red;
        //Debug.DrawLine(Vector3.zero, kinectSpace/225.0f);

        float xPos = kinectSpace.x / 225;
        userTransform.position = new Vector3(xPos, 0, creatureInteractionDistanceFromScreen);
    }


    // Return whether we can track a new user
    // TODO: update this based on our nature etc...
    public bool TrackNewUser(int userID)
    {
        //Debug.Log("Checking if creature can track a new user");
        if (currentTarget != -1)
        {
            return false;
        }
        Debug.Log("User : " + userID + " assigned to creature: " + id);
        currentTarget = userID; // Start tracking user
        return true;
    }

    #region Animation Events

    public void SetAnimationStart(string animationToPlay)
    {
        currentPlayingAnimation = animationToPlay;

        animation.Play(currentPlayingAnimation);
        //Debug.Log("playing animation: " + animationToPlay);
        //var animTime = animation.GetClip(animationToPlay).length;
        //Debug.Log("playing animation time: " + animTime);
        //Invoke("SetAnimationFinished", animTime);
    }


    /// <summary>
    /// Sets status of animatons
    /// </summary>
    public void SetAnimationFinished()
    {
        animationPlaying = false;
        //Debug.Log("SEtting animation to finished");
    }

    /// <summary>
    /// Start playing an audio clip synced on an animation
    /// </summary>
    /// <param name="soundName"></param>
    public void BeginAnimationSound(AudioClip soundName)
    {
        audio.clip = soundName;
        audio.Play();
    }

    #endregion
}
