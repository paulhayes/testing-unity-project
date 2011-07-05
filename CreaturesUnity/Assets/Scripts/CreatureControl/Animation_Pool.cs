using UnityEngine;
using System.Collections;
using System.Collections.Generic;



// ok, ok it's not a struct. But unity can't serialise structs
[System.Serializable]
public class Animation_Pool
{
	// made fast and a little hacky. Should put getters and setters everywhere.
	
	public AnimationController parent;
	
	// could throw this away mostly using states
	public string label = ""; 
	
	public States state;
	
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
	
	
	public void SetLabel(string s){
		
		label = s;
		
		state = (States)System.Enum.Parse(typeof(States), label.ToUpper());
		
		if(label.ToLower().Equals("transition")){
			isTransiton = true;
		}
	}
	
	
	public int GetTransitionID(States current, States target)
	{
		if(isTransiton){
		
		// fingers crossed everything is in UPPERCASE
		// this could be avoided by converting the strings to ENUMS at load time
		// Add that to a TODO
			string a = current.ToString();
			string b = target.ToString();
			
			for(int i = 0; i < anims.Count; i++)
			{
				if(anims[i].labels[1].Equals(a))
				{
					if(anims[i].labels[2].Equals(b))
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
	
	
	
	// this will eventually be replaced by weighted random decision
	public anim_struct GetRandomAnim()
	{
		currentIndex = Random.Range(0, totalAnims);
		
		return anims[ currentIndex ];	
	}
	
	
	public IEnumerator StartPlayback(GameObject target, int ID)
	{	
		if(Application.isPlaying)
		{
			while(activeAnimationPool){
			
				anim_struct a = (ID == -1)? GetRandomAnim() : anims[ID];
				
				currentPlaying = a.name;
				
				Debug.Log("Playing " + currentPlaying + " on " + state.ToString());
				
				target.animation.Play(currentPlaying);
				
				audioController.PlayAudio(a.audioName);
				
				currentAnimationLength = target.animation[currentPlaying].length;
				
				currentAnimationFinishTime = Time.time + currentAnimationLength;
				
				if(isTransiton) parent.leavingTransition = true;
				
				yield return new WaitForSeconds( currentAnimationLength );
				
			}
		}
	}
}

