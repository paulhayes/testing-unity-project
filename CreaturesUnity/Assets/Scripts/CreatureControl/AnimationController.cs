using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum States
{	
	WALK = 1,
	RUN,
	SITTING,
	STANDING,
	TRANSITION,
	LOOKLEFT,
	LOOKRIGHT
}


public class AnimationController : MonoBehaviour
{
	
	public List<Animation_Pool> animation_pools;
	
	public AudioController audioController;
	
	public GameObject target;
	
	public States activeState = States.WALK;
	
	public States changeToState = States.WALK;
	
	public bool inTransition = false;
	
	public bool leavingTransition = false;
	
	
	
	
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
		
		animation_pools.Sort(CompareByStates);
		
		StartPlayback(activeState, 1.0f, -1);
	}
	
	
	
	// Only used to move character between animation pools.
	// can be deleted later
	public void OnGUI()
	{
		
		GUILayout.BeginArea(new Rect(10, 10, 100, 600));
		
		string[] names = System.Enum.GetNames(typeof(States));
		
		foreach (string a in names)
		{
			if(a.Equals("TRANSITION")) continue;
			
		    if(GUILayout.Button(a))
			{
				changeToState = (States) System.Enum.Parse(typeof(States), a, true);
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
			
			TransitonAnimation(changeToState);
		}
		
		if(changeToState != activeState && leavingTransition == true)
		{	
			leavingTransition = false;
			
			ForcePlayAnimation(changeToState);

		}
	}
	
	// Used to play an animation. At the moment it's called in Update at transition end
	public void ForcePlayAnimation(States targetState)
	{
		
		float waitTimeCurrentAnimation = StopCurrentAnimation();
			
		StartPlayback(targetState, waitTimeCurrentAnimation, -1);	
	}
	
	
	
	// searches for correct transition animation and plays it after stopping current animation
	public void TransitonAnimation(States targetState)
	{
		
		int transitionID = animation_pools[(int)States.TRANSITION-1].GetTransitionID(activeState, targetState);
		
		// this is the bad guy. 
		// TODO, figure out plan for missing transitions. 
		// Could force crossfade when animation missing
		// Have to look into how time is measured during crossfades.
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
			
			StartPlayback(States.TRANSITION, waitTimeCurrentAnimation, transitionID);
			
		}
	}
	
	
	// Just passing through to an IEnumerator. Maybe I should lose this.
	public void StartPlayback(States toState, float startAtTime, int animIndex)
	{	
		StartCoroutine(RunAtTime(toState, startAtTime, animIndex));
	}
	
	
	
	
	public IEnumerator RunAtTime(States toState, float t, int animIndex)
	{
		// wait for set time
		// this should coincide with the amount of time left in the currently running animation
		yield return new WaitForSeconds(t);
		
		Debug.Log("Starting Playback for state " + toState.ToString());
		
		activeState = toState;
		
		int index = (int)toState -1;
		
		animation_pools[index].activeAnimationPool = true;
		
		StartCoroutine(animation_pools[index].StartPlayback(target, animIndex));
		
		// must remain in transition while not in the TRANSITION state
		inTransition = (activeState == States.TRANSITION);
		
	}
	
	
	// this method allows animaitons to continue to play out and their thread to terminate
	// returns the amount of time till the thread goes away.
	public float StopCurrentAnimation()
	{
		int index = (int)activeState -1;
		
		animation_pools[index].activeAnimationPool = false;	
		
		return animation_pools[index].currentAnimationFinishTime - Time.time;
	}
	
	
	// Sorts the Animation_Pool List so the (States -1) enum can be used as an index
	private static int CompareByStates(Animation_Pool one, Animation_Pool two)
    {
		int a = (int)one.state;
		int b = (int)two.state;
		
		return a.CompareTo(b);
    }
	
	
	// used as a search operation when creating the Animation_Pools
	int getIndex(List<Animation_Pool> arr, string s){
	
		for(int i = 0; i < arr.Count; i++) if(arr[i].label.Equals(s)) return i;	
		
		return -1;
	}
}

