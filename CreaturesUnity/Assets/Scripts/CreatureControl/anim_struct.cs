
using UnityEngine;
using System.Collections.Generic;
using System.IO;

// ok, ok it's not a struct. But unity can't serialise structs
[System.Serializable]
public class anim_struct
{
	
	public int start;
	public int end;
	public int layer;
	public string name;
	public bool loop;
	// TODO: replace with States.enem[]
	public string[] labels;
	public string audioName;
	public string mixingbone;
	
}
