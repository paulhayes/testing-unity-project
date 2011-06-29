using UnityEngine;
using System.Collections;

public class RenderVersion : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		guiText.text = SystemInfo.graphicsDeviceVersion;
	}
}
