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
	
		
	void Start ()
	{
		
		anim_struct[] global_pool = gameObject.GetComponent<MakeCreature>().animClips;
		
		audioController = (AudioController)gameObject.GetComponentInChildren<AudioController>();
		
		animation_pools = new List<Animation_Pool>();
		
		foreach(anim_struct a in global_pool){
		
			int index = getIndex(animation_pools, a.labels[0]);
			
			//Debug.Log(index);
			
			// make new anim pool
			if(index == -1)
			{
				Animation_Pool ap = new Animation_Pool();
			
				ap.SetLabel(a.labels[0]);
			
				ap.AddAnim(a);
				
				ap.SetAudioController(audioController);
				
				animation_pools.Add(ap);
				
			}
			else
			{
				animation_pools[index].AddAnim(a);
			}
		}
		
		animation_pools.Sort(CompareByStates);
		
		StartPlayback(activeState, 1.0f);
	}
	
	
	public void OnGUI()
	{
		
		GUILayout.BeginArea(new Rect(10, 10, 100, 600));
		
		string[] names = System.Enum.GetNames(typeof(States));
		
		foreach (string a in names)
		{
		
		    if(GUILayout.Button(a))
			{
				changeToState = (States) System.Enum.Parse(typeof(States), a, true);
				
			}
		}
		
		GUILayout.EndArea();
		
	}
	
	

	public void Update(){
		
		if(changeToState != activeState && inTransition == false)
		{
			
			inTransition = true;
			
			TransitonAnimation(changeToState);	
			
		}
	}
	
	
	
	public void TransitonAnimation(States toState)
	{
		float waitTimeCurrentAnimation = StopCurrentAnimation();
		
		Debug.Log("time to wait  = " + waitTimeCurrentAnimation);
		
		StartPlayback(toState, waitTimeCurrentAnimation);	
	}
	

	public void StartPlayback(States state, float startAtTime)
	{	
		StartCoroutine(RunAtTime(state, startAtTime));
	}
	
	public IEnumerator RunAtTime(States state, float t){
		
		Debug.Log("Got to StartCoroutineAtTime");

		yield return new WaitForSeconds(t);
		
		Debug.Log("Starting Playback");
		
		activeState = state;
		
		int index = (int)state -1;
		
		animation_pools[index].activeAnimationPool = true;		
		
		StartCoroutine(animation_pools[index].StartRandomPlayback(target));
		
		inTransition = false;
		
	}
	
	public float StopCurrentAnimation()
	{
		int index = (int)activeState -1;
		
		animation_pools[index].activeAnimationPool = false;	
		
		return animation_pools[index].currentAnimationFinishTime - Time.time;
	}
	
		
	private static int CompareByStates(Animation_Pool one, Animation_Pool two)
    {
		int a = (int)one.state;
		int b = (int)two.state;
		
		return a.CompareTo(b);
    }
	
	
	
	int getIndex(List<Animation_Pool> arr, string s){
	
		for(int i = 0; i < arr.Count; i++) if(arr[i].label.Equals(s)) return i;	
		
		return -1;
	}
}

