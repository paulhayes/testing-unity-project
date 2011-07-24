using UnityEngine;
using System.Collections;
using Behave.Runtime;
using Tree = Behave.Runtime.Tree;

public class FirstBehaviourTree : MonoBehaviour, IAgent
{
	
	public GameObject target;
	
	Tree m_Tree;
	
	public AnimationController ac;
	
	
	IEnumerator Start () 
	{
		
		if(target != null) ac = (AnimationController)target.GetComponent<AnimationController>();
		
		m_Tree = BLTestBehaveLibrary.InstantiateTree(BLTestBehaveLibrary.TreeType.Standing_Bored, this);
	
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
		
		bool isAction = (BLTestBehaveLibrary.IsAction(sender.ActiveID));
		
		Debug.LogWarning(
		          "Got Ticked by unhandled " +
		
		          (isAction ? "action " : "decorator ") + 
		          
		          (isAction ?
		          	((BLTestBehaveLibrary.ActionType)sender.ActiveID).ToString() 
		           	:
		          	((BLTestBehaveLibrary.DecoratorType)sender.ActiveID).ToString() 
		          )
		);
		
		return BehaveResult.Success;
	}
	
	
	public BehaveResult TickPlayAnimAction(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		// use action chance to decide on running animation

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
	
	
	public BehaveResult TickShouldSitDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
			
		float roll = Random.value;
		
		if(roll > 0.9f && ac.timeOfActivePool > 10.0f)
		{
			return BehaveResult.Success;
		}
		
		return BehaveResult.Failure;
		
	}

	
	public BehaveResult TickShouldStandDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		float roll = Random.value;
		
		if(roll > 0.95f && ac.timeOfActivePool > 10.0f){
	
			return BehaveResult.Success;
		}
		
		return BehaveResult.Failure;
	}
	
	
	
	public BehaveResult TickShouldWalkDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		float roll = Random.value;
		
		if(roll > 0.5f && ac.timeOfActivePool > 10.0f)
		{
			return BehaveResult.Success;
		}
		
		return BehaveResult.Failure;
	}
	
	
	public BehaveResult TickShouldAttackDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
//		float roll = Random.value;
//		
//		if(roll > 0.5f && ac.timeOfActivePool > 10.0f)
//		{
//			return BehaveResult.Success;
//		}
		
		return BehaveResult.Failure;
	}	
	
	
	
	public BehaveResult TickShouldCommunicateDecorator(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		return BehaveResult.Success;
	}
	
	
	
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
	
	
	
	
	
	public BehaveResult TickDoTransitionAction(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
	{
		
		Debug.Log("Time to transition out " + stringParameter);
		
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
	
	
	public void Reset (Tree sender)
	{	
	}
	
	
	
	
	// used to select top priority
	public int SelectTopPriority (Tree sender, params int[] IDs)
	{
		return IDs[0];
	}
}
