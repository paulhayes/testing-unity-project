using UnityEngine;
using System.Collections;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;

public class FirstBehaviourTree : MonoBehaviour, IAgent
{
	
	public GameObject target;
	
	Tree m_Tree;
	
	public AnimationController ac;
	
		
	public bool stopMoving = false;
	public bool isWalking = false;
	public bool moveToTarget = false;
	
	
	public GameObject movementTarget;
	
	

	
	
	IEnumerator Start () 
	{
		
		if(target != null) ac = (AnimationController)target.GetComponent<AnimationController>();
		
		m_Tree = BLtestbehavelibrary.InstantiateTree(BLtestbehavelibrary.TreeType.Standing_Bored, this);
	
		
		// this is just to get them out of sync
		yield return new WaitForSeconds( Random.Range(0f, 1.0f) );
		
		while (Application.isPlaying && m_Tree != null)
		{
			yield return new WaitForSeconds(1.0f / m_Tree.Frequency);
			
			AIUpdate();			
		}
	}
	
	
	
	void AIUpdate()
	{
		m_Tree.Tick();
	}
	
	public BehaveResult Tick (Tree sender, bool init)
	{
		
		bool isAction = (BLtestbehavelibrary.IsAction(sender.ActiveID));
		
		Debug.LogWarning(
		          "Got Ticked by unhandled " +
		
		          (isAction ? "action " : "decorator ") + 
		          
		          (isAction ?
		          	((BLtestbehavelibrary.ActionType)sender.ActiveID).ToString() 
		           	:
		          	((BLtestbehavelibrary.DecoratorType)sender.ActiveID).ToString() 
		          )
		);
		
		return BehaveResult.Success;
	}
	
	
	
	
	#region DescideToDoStuff
	
	public BehaveResult TickShouldSitDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		if(isWalking) return BehaveResult.Failure;
		
		float roll = Random.value;
		
		if(roll > 0.9f && ac.timeOfActivePool > 10.0f)
		{
			return BehaveResult.Success;
		}
		
		return BehaveResult.Failure;
		
	}

	
	
	
	
	public BehaveResult TickShouldStandDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		if(stopMoving) return BehaveResult.Success;
		
		if(isWalking) return BehaveResult.Failure;
		
		if(isLooking) return BehaveResult.Success;
		
		float roll = Random.value;
		
		if(roll > 0.95f && ac.timeOfActivePool > 10.0f){
	
			return BehaveResult.Success;
		}
		
		return BehaveResult.Failure;
	}
	
	
	public BehaveResult TickShouldWalkDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		if(moveToTarget) return BehaveResult.Success;
		
//		float roll = Random.value;
//		
//		if(roll > 0.5f && ac.timeOfActivePool > 10.0f)
//		{
//			return BehaveResult.Success;
//		}
		
		return BehaveResult.Failure;
	}
	
	
	public BehaveResult TickShouldAttackDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		float roll = Random.value;
		
		if(roll > 0.5f && ac.timeOfActivePool > 10.0f)
		{
			return BehaveResult.Success;
		}
		
		return BehaveResult.Failure;
	}	
	
	
	
	public BehaveResult TickShouldCommunicateDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		return BehaveResult.Success;
	}
	
	
	public bool newPerson = false;
	
	Transform newUserPosition;
	
	public bool lookLeft = true;
	
	public bool isLooking = false;
	
	
	
	public void LookToNewUser(Transform t){
	
		newUserPosition = t;
		
		if(transform.position.x > t.position.x) lookLeft = false;
		
		newPerson = true;
	}
	
	public BehaveResult TickLookDirectionDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		if(stringParameter.Equals("LEFT"))
		{
			return (lookLeft) ? BehaveResult.Success : BehaveResult.Failure;
		}
		else{
		
			return (lookLeft == false) ? BehaveResult.Success : BehaveResult.Failure;
		}
	}
	
	public BehaveResult TickShouldLookDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		if(newPerson)
		{
			newPerson = false;	
			return BehaveResult.Success;
		}
		
		return BehaveResult.Failure;
	}
	
	#endregion
	
	
	#region Misc Decorators
	
	
	public BehaveResult TickRunForeverDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		return BehaveResult.Running;
	}		
	
	
	public BehaveResult TickCheckActiveStateDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		AnimationPoolState check = (AnimationPoolState)System.Enum.Parse(typeof(AnimationPoolState), stringParameter);
		
		return (ac.activeState == check) ? BehaveResult.Success : BehaveResult.Failure;
	}		
	
	
	
	public BehaveResult TickCheckIsntActiveStateDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		AnimationPoolState check = (AnimationPoolState)System.Enum.Parse(typeof(AnimationPoolState), stringParameter);
		
		return (ac.activeState != check) ? BehaveResult.Success : BehaveResult.Failure;
	}
	
	#endregion
	
	
	#region Actions
	
	public BehaveResult TickPlayAnimAction(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{

		
		
		float roll = Random.value;
		
		// Debug.Log(roll + " " + floatParameter + " " + stringParameter);
		
		if( floatParameter >= roll  || floatParameter < 0 )
		{
			int val = ac.StartPlayback(stringParameter);
			
			switch (val)
			{
				case -1:
					//Debug.LogWarning(stringParameter + " returned a fail");
					return BehaveResult.Failure;
				
				case 0:
					//Debug.Log(stringParameter + " returned a running");
					return BehaveResult.Running;
				
				case 1:
					//Debug.Log(stringParameter + " returned a success");
					return BehaveResult.Success;
			}
		}
		
		return BehaveResult.Success;
		
	}
	
	
	
	
	public BehaveResult TickDoTransitionAction(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		//Debug.Log("Time to transition out " + stringParameter);
		
		string[] words = stringParameter.ToUpper().Split(' ');
				
		AnimationPoolState[] a = new AnimationPoolState[words.Length];
		
		for(int i = 0; i < words.Length; i++){
			a[i] = (AnimationPoolState) System.Enum.Parse(typeof(AnimationPoolState), words[i]);	
		}
		
		if(ac.activeState == a[0]){
	
			ac.changeToState = a[1];
			
			return BehaveResult.Success;
			
		}
		
		return BehaveResult.Failure;	
	}	
	
	
	
	public BehaveResult TickMoveToLocationAction(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		if(Vector3.Distance(transform.position, movementTarget.transform.position)  > 4)
		{
			NowWalking();
			return BehaveResult.Success;
			
		}
		
		Arrived();
		
		return BehaveResult.Failure;
		
	}
	
	#endregion
	
	void Update()
	{
		if(isWalking)
		{
		
			Vector3 dir = movementTarget.transform.position - transform.position;
			
			Quaternion toAngle = Quaternion.LookRotation(dir);
			
			transform.rotation = Quaternion.Slerp(transform.rotation, toAngle, Time.deltaTime);
			
			transform.position += transform.forward * Time.deltaTime * 2;
			
			Debug.DrawLine(transform.position, movementTarget.transform.position);
			
			if(Vector3.Distance(transform.position, movementTarget.transform.position)  < 4) Arrived();
			
		}
		
		isWalking = (ac.activeState == AnimationPoolState.WALK || ac.activeState == AnimationPoolState.ATTACKWALK) ? true : false;
		
		isLooking = (ac.activeState == AnimationPoolState.LOOKLEFT || ac.activeState == AnimationPoolState.LOOKRIGHT) ? true : false;
	}
	
	
	public void MoveToNewTarget(GameObject g)
	{
		
		movementTarget = g; 
		moveToTarget = true;
		
	}
	
	
	public void NowWalking()
	{
		isWalking = true;
		stopMoving = false;
	}
	
	public void Arrived()
	{
		
		moveToTarget = false;
		stopMoving = true;
	}
	
	
	
	
	public void Reset (Tree sender)
	{	
	}
	
	
	
	
	// used to select top priority
	public int SelectTopPriority (Tree sender, params int[] IDs)
	{
		return IDs[0];
	}
}
