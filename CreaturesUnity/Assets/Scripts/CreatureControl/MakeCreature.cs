using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MakeCreature : MonoBehaviour {
	
	public GameObject[] sourceModels;
	
	public GameObject target;
	
	public string[] AnimNames;
	
	public string currentAnimation;
	
	public int animationIndex = 0;
	
	// Use this for initialization
	void Start () 
	{
		// manually triggered if you kill the array
		if(AnimNames.Length == 0 || AnimNames == null) Populate();
		
		StartCoroutine(PlayAnimation());
		
	}
	
	
	// This will go through every animation on the target and play it once
	public IEnumerator PlayAnimation()
	{
		while(Application.isPlaying){
		
			currentAnimation = AnimNames[animationIndex];
						
			target.animation.Play(currentAnimation);
			
			// get animaton length through accessing the AnimationState
			yield return new WaitForSeconds( target.animation[currentAnimation].length );
			
			animationIndex = (animationIndex +1) % AnimNames.Length;
			
		}
	}
		               
	// Update is called once per frame
//	void Update () 
//	{
//	
//	}
	
	
	// this should only be called if the animation name array is null
	// Only needs to happen if te prefab needs recreating.
	// maybe not the best idea but it's a speed up really
	void Populate()
	{
	
		List<string> names = new List<string>();
		
		List<AnimationClip> clips = new List<AnimationClip>();
		
		// go over all sources and get out all animation clips
		for( int i = 0; i < sourceModels.Length; i ++)
		{
			
			Animation a = sourceModels[i].GetComponent<Animation>();
			
			foreach (AnimationState state in a) {
            	clips.Add(state.clip);
				names.Add(state.clip.name);
        	}
		}
		
		// the names array. Used to call into animations states and get the length
		AnimNames = names.ToArray();
		
		Animation targetAnim = target.GetComponent<Animation>();
		
		// And add all clips to the the target model
		foreach (AnimationClip state in clips) {
			
			targetAnim.AddClip(state, state.name);
			
		}
		
	}
}
