using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class Animation_Pool
{
	// made fast and a little hacky. Should put getters and setters everywhere.
	
	public AnimationController parent;
	
	public AnimationPoolState state;
	
	// Very important, used for the special case TRANSITION Pool
	public bool isTransiton = false;
	
	public string currentPlaying = "";
	
	public int currentIndex = 0;
	
	public int totalAnims = 0;
	
	public List<anim_struct> anims = new List<anim_struct>();
	
	public bool activeAnimationPool = false;
	
	public float currentAnimationLength = 0.0f;
	
	public float currentAnimationFinishTime = 0.0f;
	
	// audio playback
	AudioController audioController;
	
	
	public void SetAudioController(AudioController a){
		audioController = a;	
	}
	
	
	public void SetLabel(AnimationPoolState s){
				
		state = s;
		
		if(state == AnimationPoolState.TRANSITION){
			isTransiton = true;
		}
	}
	
	public bool isPlaying(string s)
	{
		return (currentPlaying.ToLower().Equals(s.ToLower()));
		
	}
	
	public int GetTransitionID(AnimationPoolState current, AnimationPoolState target)
	{
		if(isTransiton){
			
			for(int i = 0; i < anims.Count; i++)
			{
				if(anims[i].labels[1] == current)
				{
					if(anims[i].labels[2] == target)
					{
						return i;
					}
				}
			}
		}
		
		return -1;
	}
	
	
	public void AddAnim(anim_struct a){
		anims.Add(a);
		totalAnims = anims.Count;
	}

	
	public void StartPlayback(GameObject target, int ID)
	{	
			
		anim_struct a = (ID == -1)? GetRandomAnim() : anims[ID];
		
		currentPlaying = a.name;
		
		//Debug.Log("Playing " + currentPlaying + " on " + state.ToString());				
		
		target.animation[currentPlaying].layer = 1; // a.layer;
		
//		if(a.loop){ target.animation[currentPlaying].wrapMode = WrapMode.Loop;
//		}
//		else{
//			target.animation[currentPlaying].wrapMode = WrapMode.Once;
//		}
		
		target.animation[currentPlaying].wrapMode = WrapMode.Once;
		
		target.animation.CrossFade(currentPlaying);

		audioController.PlayAudio(a.audioName[Random.Range(0, a.audioName.Length)]);

		
		currentAnimationLength = target.animation[currentPlaying].length;
						
		currentAnimationFinishTime = Time.time + currentAnimationLength;
		
		
		parent.timeTillAnimationEnd = currentAnimationLength;
		
		
		if(isTransiton) parent.leavingTransition = true;
	
	}
	
	
	// this will eventually be replaced by weighted random decision
	public anim_struct GetRandomAnim()
	{
		currentIndex = Random.Range(0, totalAnims);
		
		return anims[ currentIndex ];	
	}
	

	
	
	public IEnumerator StartRandomPlayback(GameObject target, int ID)
	{	
		if(Application.isPlaying)
		{
			while(activeAnimationPool){
			
				anim_struct a = (ID == -1)? GetRandomAnim() : anims[ID];
				
				currentPlaying = a.name;
				
				Debug.Log("Playing " + currentPlaying + " on " + state.ToString());
				
				//target.animation[currentPlaying].layer = a.layer;
				
				
				target.animation.CrossFade(currentPlaying);


				audioController.PlayAudio(a.audioName[Random.Range(0, a.audioName.Length)]);
				
				currentAnimationLength = target.animation[currentPlaying].length;
								
				currentAnimationFinishTime = Time.time + currentAnimationLength;
				
				if(isTransiton) parent.leavingTransition = true;
				
				yield return new WaitForSeconds( currentAnimationLength );
				
			}
		}
	}
	
	public anim_struct GetNamedAnimation(string s )
	{
		
		int index = AnimationIndex(s);
		
		return (index == -1) ? null : anims[index];
		
	}
	
	
	public int AnimationIndex(string s )
	{
		for(int i = 0; i < totalAnims; i++){
			if(anims[i].name.ToLower().Contains(s.ToLower())){
				return i;
			}
		}
		
		Debug.LogWarning("no animations called " + s + " in pool " + state.ToString());
		
		return -1;
	}
	
	
	public void StartAdditivePlayback(GameObject target, PersonalityType ID)
	{	
		if(Application.isPlaying)
		{
		 
			anim_struct a = GetNamedAnimation(ID.ToString());
			
			if(a == null) return;
			
			currentPlaying = a.name;
			
			Debug.Log("Playing " + currentPlaying + " on " + state.ToString());
			
			target.animation[currentPlaying].layer = 10;//a.layer;
			//target.animation[currentPlaying].blendMode = AnimationBlendMode.Additive;
			target.animation[currentPlaying].wrapMode = WrapMode.ClampForever;
			target.animation[currentPlaying].enabled = true;
			target.animation[currentPlaying].weight = 1.0f;
			
			target.animation.Play(currentPlaying);

			//audioController.PlayAudio(a.audioName);
		
		}
	}
}

