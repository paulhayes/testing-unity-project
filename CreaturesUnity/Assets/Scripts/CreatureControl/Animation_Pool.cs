using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// ok, ok it's not a struct. But unity can't serialise structs
[System.Serializable]
public class Animation_Pool
{
	public string label = "";
	
	public bool isTransititon = false;
	
	public int currentPlaying = 0;
	
	public int totalAnims = 0;
	
	public List<anim_struct> anims = new List<anim_struct>();
	
	public void addAnim(anim_struct a){
		anims.Add(a);
		totalAnims++;
	}
	
	
	public anim_struct getNextAnim()
	{
		return anims[ (currentPlaying++) % totalAnims ];	
	}
	
	public anim_struct getRandomAnim()
	{
		return anims[ Random.Range(0, totalAnims) ];	
	}
}

