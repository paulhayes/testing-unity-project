using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum AnimationPoolState
{	
	WALK = 1,
	RUN,
	SITTING,
	STANDING,
	ATTACKWALK,
	ATTACKSTANCE,
	LOOKLEFT,
	LOOKRIGHT,
	HEAD,
	TURN,
	PROCEDURAL,
	TRANSITION,
	COMMUNICATION, // Start of potential future bug. See below
	HIT,
	LEFT,
	RIGHT,
	IDLE,
	LOOK,
	SCALE
}

// I'm sorting the animation pool array by the order of the enums above.
// I then use the index of that enum to call the playback.
// the problem is if there is no animation pool for that enum in the middle of the list
// the order is wrong. 
// SO for the time being I'm putting unsued animaitons to the bottom of the enum list.
// A better idea is to use a dictionary but that won't display in the unity inspector. 


public enum PersonalityType
{
	BABY = 1,
	FEMALE,
	FRENETIC,
	ALPHA,
	HEAD,
	STANDARD
}


public class AnimationController : MonoBehaviour
{
	
	public AudioController audioController;
	
	public GameObject target;
	
	public AnimationPoolState activeState = AnimationPoolState.STANDING;
	
	public AnimationPoolState changeToState = AnimationPoolState.STANDING;
	
	public int activeAnimation = 0;
	
	public bool inTransition = false;
	
	public bool leavingTransition = false;
	
	public PersonalityType personality = PersonalityType.HEAD;
	
	public float timeTillAnimationEnd = 0.0f;
	
	
	public float timeOfActivePool = 0.0f;
	
	public List<Animation_Pool> animation_pools;
	
	
	void Start ()
	{
		
		anim_struct[] global_pool = gameObject.GetComponent<MakeCreature>().animClips;
		
		audioController = (AudioController)gameObject.GetComponentInChildren<AudioController>();
		
		animation_pools = new List<Animation_Pool>();
		
		foreach(anim_struct a in global_pool){
		
			int index = getIndex(animation_pools, a.labels[0]);
			
			// make new anim pool
			if(index == -1)
			{
				
				//TODO: make a damn constructor
				Animation_Pool ap = new Animation_Pool();
			
				ap.SetLabel(a.labels[0]);
			
				ap.AddAnim(a);
				
				ap.SetAudioController(audioController);
				
				ap.parent = this;
				
				animation_pools.Add(ap);
				
			}
			else
			{
				animation_pools[index].AddAnim(a);
			}
		}
		
		animation_pools.Sort(CompareByAnimationPoolState);
		
		
		StartProceduralPlayback(personality);
		
		
		//StartPlayback(activeState, 1.0f, -1);
	}
	
	
	
	// Only used to move character between animation pools.
	// can be deleted later
	public void OnGUITemp()
	{
		
		GUILayout.BeginArea(new Rect(10, 10, 100, 600));
		
		string[] names = System.Enum.GetNames(typeof(AnimationPoolState));
		
		foreach (string a in names)
		{
			if(a.Equals("TRANSITION")) continue;
			
		    if(GUILayout.Button(a))
			{
				changeToState = (AnimationPoolState) System.Enum.Parse(typeof(AnimationPoolState), a, true);
				
				
			}
		}
		
		GUILayout.EndArea();
	}
	
	
	
	// Used only for looking to transititons at the moment. 
	// Change to State can be changed at any time and it'll force a transition.
	// Issues arrise when there are no tranitions. ie. it doesn't transition
	// TODO: fix no transition case.
	public void Update(){
		
		if(changeToState != activeState && inTransition == false)
		{
			inTransition = true;
			
			timeOfActivePool = 0.0f;
			
			TransitonAnimation(changeToState);
		}
		
		if(changeToState != activeState && leavingTransition == true)
		{	
			leavingTransition = false;
			
			timeOfActivePool = 0.0f;
			
			ForcePlayAnimation(changeToState);

		}
		
		timeOfActivePool += Time.deltaTime;
		
		timeTillAnimationEnd = GetTimeRemaining();
		
	}
	
	// Used to play an animation. At the moment it's called in Update at transition end
	public void ForcePlayAnimation(AnimationPoolState targetState)
	{
		float waitTimeCurrentAnimation = StopCurrentAnimation();
			
		StartPlayback(targetState, waitTimeCurrentAnimation, -1);	
	}
	
	

	
	
	
	// searches for correct transition animation and plays it after stopping current animation
	public void TransitonAnimation(AnimationPoolState targetState)
	{
		int transIndex = (int)AnimationPoolState.TRANSITION-1;
		
		int transitionID = animation_pools[transIndex].GetTransitionID(activeState, targetState);
		
		// TODO, figure out plan for missing transitions. 
		// Could force crossfade when animation missing
		if(transitionID == -1)
		{
			Debug.LogError("Animation missing for transition from " + activeState.ToString() +" to "+ targetState.ToString() );
			
			
			// for the time being I just pretend like nothing happened
			inTransition = false;
			
			changeToState = activeState;
		}
		else
		{
			//Debug.Log("transition animation at index " + transitionID);
			
			float waitTimeCurrentAnimation = StopCurrentAnimation();
			
			StartPlayback(AnimationPoolState.TRANSITION, waitTimeCurrentAnimation, transitionID);
			
		}
	}
	
	
	// Just passing through to an IEnumerator. Maybe I should lose this.
	public void StartPlayback(AnimationPoolState toState, float startAtTime, int animIndex)
	{	
		if(toState != activeState && toState == AnimationPoolState.WALK){
			SendMessage("NowWalking");
		}
		
		
		StartCoroutine(RunRandomLoopTime(toState, startAtTime, animIndex));
	}
	

		// Just passing through to an IEnumerator. Maybe I should lose this.
	public int StartPlayback(string animName)
	{	
		
		int animationID = animation_pools[(int)activeState -1].AnimationIndex(animName);
		
		if(animationID == -1)
		{
			// fuck there is no anmation
			return -1;
			
		}
		if(timeTillAnimationEnd <= 0.0f)
		{
			RunAnimation(activeState, animationID);
			
			return 1;
		}
		
		
		bool status = animation_pools[(int)activeState -1].isPlaying(animName);
		
		if(status == false)
		{
			
			return 0;	
		}
		else
		{

			// not running, going to run next.
			//RunAnimation(activeState, animationID);
			
			return 0;
		}
	}
	
	public void RunAnimation(AnimationPoolState toState, int animIndex)
	{
		

		activeState = toState;
		
		
		int index = (int)toState -1;
		
		animation_pools[index].activeAnimationPool = true;
		
		animation_pools[index].StartPlayback(target, animIndex);
		
		// must remain in transition while not in the TRANSITION state
		inTransition = (activeState == AnimationPoolState.TRANSITION);
		
	}
	
	
	public IEnumerator RunRandomLoopTime(AnimationPoolState toState, float t, int animIndex)
	{
		// wait for set time
		// this should coincide with the amount of time left in the currently running animation
		yield return new WaitForSeconds(t);
		
		Debug.Log("Starting Playback for state " + toState.ToString());
		
		activeState = toState;
		
		int index = (int)toState -1;
		
		animation_pools[index].activeAnimationPool = true;
		
		//StartCoroutine(animation_pools[index].StartRandomPlayback(target, animIndex));
		
		animation_pools[index].StartPlayback(target, animIndex);
		
		// must remain in transition while not in the TRANSITION state
		inTransition = (activeState == AnimationPoolState.TRANSITION);
		
	}
	
	public void StartProceduralPlayback(PersonalityType type)
	{
		
		int index = (int)AnimationPoolState.PROCEDURAL -1 ;
		
		animation_pools[index].StartAdditivePlayback(target, type);
		
	}
	
	
	// this method allows animaitons to continue to play out and their thread to terminate
	// returns the amount of time till the thread goes away.
	public float StopCurrentAnimation()
	{
		int index = (int)activeState -1;
		
		animation_pools[index].activeAnimationPool = false;	
		
		return GetTimeRemaining();
	}
	
	public float GetTimeRemaining()
	{
		int index = (int)activeState -1;
		
		return animation_pools[index].currentAnimationFinishTime - Time.time;
	}
	
	
	// Sorts the Animation_Pool List so the (AnimationPoolState -1) enum can be used as an index
	private static int CompareByAnimationPoolState(Animation_Pool one, Animation_Pool two)
    {
		int a = (int)one.state;
		int b = (int)two.state;
		
		return a.CompareTo(b);
    }
	
	
	// used as a search operation when creating the Animation_Pools
	int getIndex(List<Animation_Pool> arr, AnimationPoolState a){
	
		for(int i = 0; i < arr.Count; i++) if(arr[i].state == a) return i;	
		
		return -1;
	}
}

