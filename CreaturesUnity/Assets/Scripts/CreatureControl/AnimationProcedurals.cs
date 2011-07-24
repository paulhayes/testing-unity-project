using UnityEngine;
using System.Collections;

public class AnimationProcedurals : MonoBehaviour {
	
	public AnimationState lookAround;
	
	public GameObject target;
	
	public float val;
	public float angle;
	
	public bool looking  = false;
	
	
	IEnumerator Start () {
		
		float scaleFactor = Random.Range( 0.7f, 1.1f);
		
		transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
		
		
		yield return new WaitForEndOfFrame();
		
		if(target == null) target = gameObject;
		
		lookAround = target.animation["LookAround"];
		
		lookAround.layer = 10;
		lookAround.wrapMode = WrapMode.ClampForever;
		lookAround.enabled = true;
		lookAround.weight = 1.0f;
		
		val = 0.5f;
		
		looking = false;

	}
	
	
	void LateUpdate () 
	{
		if(looking)
		{
			Vector3 lookAtPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Camera.main.pixelHeight/2, Camera.main.transform.position.z));
			
			lookAtPosition -= target.transform.position;
			
			angle = Vector3.Angle(lookAtPosition, target.transform.right);
			
			angle -= 90.0f;
			
			if(angle > -30 && angle < 30)
			{
				val -= (val - Mathf.Clamp(((angle / 30.0f) + 1) * 0.5f, 0.0f, 1.0f))  * 0.1f;
			}
			else{
				val -= (val - 0.5f) * 0.01f;	
			}
			
			lookAround.normalizedTime = val;
		}
		else{
			
			val -= (val - 0.5f) * 0.01f;	
			
			lookAround.normalizedTime = val;
		}
   
		
		if(lookAround == null) looking = false;
	}
}
