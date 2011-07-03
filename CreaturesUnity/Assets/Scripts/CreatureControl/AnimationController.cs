using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationController : MonoBehaviour
{
	
	public List<Animation_Pool> animation_pools;
		
	void Start ()
	{
		
		anim_struct[] global_pool = gameObject.GetComponent<MakeCreature>().animClips;
		
		animation_pools = new List<Animation_Pool>();
		
		foreach(anim_struct a in global_pool){
		
			int index = getIndex(animation_pools, a.labels[0]);
			
			Debug.Log(index);
			
			if(index == -1)
			{
				Animation_Pool ap = new Animation_Pool();
				ap.label = a.labels[0];
				
				if(ap.label.ToLower().Equals("transition"))ap.isTransititon = true;
				
				ap.addAnim(a);
				
				animation_pools.Add(ap);
				
			}
			else
			{
				animation_pools[index].addAnim(a);
			}
		}		
	}
	
	int getIndex(List<Animation_Pool> arr, string s){
	
		for(int i = 0; i < arr.Count; i++){
				
			if(arr[i].label.Equals(s))return i;
			
		}
		
		return -1;
	}
	

}

