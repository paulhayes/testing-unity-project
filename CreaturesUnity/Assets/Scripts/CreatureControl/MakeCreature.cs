using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class MakeCreature : MonoBehaviour {
	
	public GameObject[] sourceModels;
	
	public GameObject target;
	
	public anim_struct[] animClips;
	
	public string currentAnimation;
	
	public int animationIndex = 0;
	
	public TextAsset animationInformation;
	
	AudioController audioController;
	
	
	
	// Use this for initialization
	void Awake () 
	{
		audioController = (AudioController)gameObject.GetComponentInChildren<AudioController>();
		
		// manually triggered if you kill the array
		if(animClips.Length == 0 || animClips == null) Populate();
		
		StartCoroutine(PlayAnimation());
		
	}
	
	
	// This will go through every animation on the target and play it once
	public IEnumerator PlayAnimation()
	{
		while(Application.isPlaying){
		
			currentAnimation = animClips[animationIndex].name;
						
			target.animation.Play(currentAnimation);
			
			audioController.PlayAudio(animClips[animationIndex].audioName);
			
			// get animaton length through accessing the AnimationState
			yield return new WaitForSeconds( target.animation[currentAnimation].length );
			
			animationIndex = (animationIndex +1) % animClips.Length;
			
		}
	}
		               
	// Update is called once per frame
//	void Update () 
//	{
//	
//	}
	
	
	// this should only be called if the animation name array is null
	// Only needs to happen if the prefab needs recreating.
	// maybe not the best idea but it's a speed up really
	void Populate()
	{
	
		//List<string> names = new List<string>();
		
		List<AnimationClip> clips = new List<AnimationClip>();
		
		List<anim_struct> anims = new List<anim_struct>();
		
		// go over all sources and get out all animation clips
		for( int i = 0; i < sourceModels.Length; i ++)
		{
			
			Animation a = sourceModels[i].GetComponent<Animation>();
			
			foreach (AnimationState state in a) 
			{
            	clips.Add(state.clip);
				
				ParseFile(state.clip.name, ref anims);
        	}
		}
		
		// the names array. Used to call into animations states and get the length
		animClips = anims.ToArray();
		
		
		Animation targetAnim = target.GetComponent<Animation>();
		
		// And add all clips to the the target model
		foreach (AnimationClip state in clips) 
		{	
			targetAnim.AddClip(state, state.name);	
		}
		
		animationIndex = Random.Range(0, animClips.Length);
		
	}
	
	
	// It'll search through the csv looking for the animation name. 
	// unity doesn' allow name collisions so fingers crossed it won't be an issue
	private void ParseFile(string filter, ref List<anim_struct> anims)
	{
	    		
		if (animationInformation == null) 
		{
			Debug.Log("no animation information. Check you've included the file");
			return;
		}
		
	    StringReader reader = new StringReader(animationInformation.text);
				
	    string temp = reader.ReadLine();
	
	    while(temp != null)
	    {
			
			string[] vals = temp.Split(',');
			
			if(filter.Equals(vals[3]))
			{	
				anim_struct a  = new anim_struct();
				a.name = vals[3];
				a.start = int.Parse(vals[5]);
				a.end = int.Parse(vals[6]);
				a.loop = bool.Parse(vals[7]);
				a.labels = vals[10].Split(' ');
				a.audioName = vals[8];
				
				// turn this on if looped animations are out one frame
				if(a.loop) a.end--;
				
		        anims.Add(a);
				
				break;
			}
			
			temp = reader.ReadLine();
			
			if(temp.Length == 0){
				break;	
			}
	    }
	}
}
