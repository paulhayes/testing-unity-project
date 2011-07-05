using UnityEngine;
using System.Collections;
using System.Collections.Generic;



// ok, ok it's not a struct. But unity can't serialise structs
[System.Serializable]
public class Animation_Pool
{
	public string label = "";
	
	public States state;
	
	public bool isTransititon = false;
	
	public string currentPlaying = "";
	
	public int currentIndex = 0;
	
	public int totalAnims = 0;
	
	public List<anim_struct> anims = new List<anim_struct>();
	
	public bool activeAnimationPool = false;
	
	public float currentAnimationLength = 0.0f;
	
	public float currentAnimationFinishTime = 0.0f;
	
	AudioController audioController;
	
	
	public void SetAudioController(AudioController a){
		audioController = a;	
	}
	
	
	public void SetLabel(string s){
		
		label = s;
		
		state = (States)System.Enum.Parse(typeof(States), label.ToUpper());
		
		if(label.ToLower().Equals("transition")){
			isTransititon = true;
		}
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
	
	
	public IEnumerator StartRandomPlayback(GameObject target)
	{	
		if(Application.isPlaying)
		{
			while(activeAnimationPool){
			
				anim_struct a = GetRandomAnim();
				
				currentPlaying = a.name;
				
				Debug.Log("Playing " + currentPlaying + " on " + state.ToString());
				
				target.animation.Play(currentPlaying);
				
				audioController.PlayAudio(a.audioName);
				
				currentAnimationLength = target.animation[currentPlaying].length;
				
				currentAnimationFinishTime = Time.time + currentAnimationLength;
				
				yield return new WaitForSeconds( currentAnimationLength );
				
			}
		}
	}
}

