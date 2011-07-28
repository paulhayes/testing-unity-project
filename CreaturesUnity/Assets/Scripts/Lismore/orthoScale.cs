using UnityEngine;
using System.Collections;

public class orthoScale : MonoBehaviour {

    private float defaultHeight = 1;
    public float fudgeFactor = 1;

	// Use this for initialization
	void Start () 
	{
        //defaultHeight = transform.Find("creature").GetComponent<Creature>().sizeMultiplier;
//        defaultHeight = transform.GetComponent<Creature>().sizeMultiplier;
//        if (defaultHeight == 0)
//        {
//            defaultHeight = 1;
//        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        var currentZ = transform.position.z;
        if(fudgeFactor == 0 || currentZ == 0)
        {
            Debug.Log("defaultHeight: " + defaultHeight);
            Debug.Log("fudgeFactor: " + fudgeFactor);
            Debug.Log("currentZ: " + currentZ);
            return;
        }

        var scaleAmount = defaultHeight * fudgeFactor * currentZ;
        //Debug.Log("scaleAmount: " + scaleAmount);
        transform.localScale = new Vector3(scaleAmount, scaleAmount, scaleAmount);
	}
}
