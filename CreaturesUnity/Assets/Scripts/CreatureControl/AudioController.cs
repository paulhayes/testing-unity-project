using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour {
	
	public AudioSource[] sources;
	
	public string message;
	

	void Start () {
		
		sources = (AudioSource[])gameObject.GetComponentsInChildren<AudioSource>();
	
	}
	
	public void PlayAudio(string s)
	{
		if(s == null) 
		{
			//Debug.Log("no audio sources");
			return;
		}
		
		bool toldToPlay = false;
		
		int playingCount = 0;
		
		foreach(AudioSource a in sources)
		{
			playingCount ++;
			
			if(!a.isPlaying)
			{
				a.clip = (AudioClip)Resources.Load(s);
				
				a.pitch = Random.Range(0.8f, 1.3f);
				
				//Debug.Log("Audio file played " + s);
				
				a.Play();
				
				toldToPlay = true;
				
				break;
			}
		}
		
		message = (toldToPlay) ? s + " on source " + playingCount : " No free AudioSource";
		
		if(!toldToPlay)
		{
			Debug.LogWarning("No free AudioSource");
		}
	}
}
