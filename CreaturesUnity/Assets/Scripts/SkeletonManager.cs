using KinectViaTcp;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkeletonManager : MonoBehaviour {

    private Dictionary<int, GameObject[]> users;
    private KinectTCPListener associatedListener;

    public float kinectOffset = 0f;
	
	public GameObject emitter;

//    public bool forcePowerOfTwo = true;			// Default: True (faster), False is much slower.
//    public bool useMipmaps = false;		        // Default: False (faster), True is slower, but lets you scale texture.
//	
//
//    private Texture2D depthMapTexture;	        // Unity Texture for displaying Kinect depth.
//    private Color[] depthMapColors;             // Unity colors array for kinect depth.
//    public Color depthColor = Color.yellow;
//
//    public GameObject depthCube;
//
//    private int actualFactor = 4;	            // User determined scaled forced to power-of-two, i.e. 1,2,4,8 etc	
//    private int rawWidth = 320;			            // Width of kinect source image in pixels.
//    private int rawHeight = 240;                      // Height of kinect source image in pixels.
//    private int potWidth;			            // Width of Power-of-two Unity Texture.
//    private int potHeight;			            // Height of Power-of-two Unity Texture.
//    private int dstWidth;			            // Width of scaled Image in Unity.
//    private int dstHeight;			            // Height of scaled Image in Unity.



    // Use this for initialization
	void Start () 
    {
        users = new Dictionary<int, GameObject[]>();

        associatedListener = (KinectTCPListener)gameObject.GetComponent<KinectTCPListener>();

/*

        dstWidth = rawWidth / actualFactor;
        dstHeight = rawHeight / actualFactor;

        

        // Force Texture to be Power-Of-Two to avid Unity making two copies
        potWidth = forcePowerOfTwo ? getNextPowerOfTwo(dstWidth) : dstWidth;
        potHeight = forcePowerOfTwo ? getNextPowerOfTwo(dstHeight) : dstHeight;
        depthMapTexture = new Texture2D(potWidth, potHeight, TextureFormat.ARGB32, useMipmaps);

        // Force filter mode to Bilinear to avoid (trilinear) mipmaps		
        depthMapTexture.filterMode = useMipmaps ? FilterMode.Trilinear : FilterMode.Bilinear;

        depthMapColors = new Color[dstWidth * dstHeight];

        // Pre-fill Unity Texture to use no alpha. To hide the area not used in the POT texture.
        Color[] txColorArray = depthMapTexture.GetPixels();

        for (int i = 0; i < txColorArray.Length; i++)
            txColorArray[i].a = 0.0f;

        depthMapTexture.SetPixels(txColorArray);

        depthCube.renderer.material.mainTexture = depthMapTexture;
 * */
	}
	
	// Update is called once per frame
	void Update () 
    {
        lock (associatedListener.skeletonBuffer)
        {
			
			//Debug.Log("Locking for listening");
            if (associatedListener.skeletonBuffer.Count > 0)
            {
				Debug.Log("There is a skeleton");
            	
				for(int i = 0; i < associatedListener.skeletonBuffer.Count; i++){
	                SkeletonData skeleton = associatedListener.skeletonBuffer[0];
	                associatedListener.skeletonBuffer.RemoveAt(0);
	                ProcessSkeleton(skeleton, associatedListener);
				}
            }
        }
	}

    void ProcessSkeleton(SkeletonData skeleton, KinectTCPListener associatedListener)
    {
		Debug.Log("ProcessSkeleton");
		
        if (skeleton.State == SkeletonState.New)
        {
			if (!users.ContainsKey(skeleton.UserIndex))
			{
			
	            // Build up new skeleton prefab
	       		// print("Creating user: " + skeleton.UserIndex);
	            GameObject[] cubeSkeleton = new GameObject[skeleton.Joints.Count];
				GameObject parent = new GameObject();
				parent.name = "User_Index_" + skeleton.UserIndex;
				parent.transform.parent = gameObject.transform;
				parent.AddComponent<DrawSkeleton>();
				parent.GetComponent<DrawSkeleton>().prefab = emitter;
				
	            // Create cubes at each point...
	            for (int i = 0; i < skeleton.Joints.Count; i++)
	            {
	                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
	               	
					cube.transform.position = new Vector3(kinectOffset, 0.0f, 0.0f);
	                
					cube.transform.localScale *= 0.1f;
	                
					cube.transform.parent = parent.transform;
					
					cubeSkeleton[i] = cube;
					
	            }
				
				parent.SendMessage("BuildSkeleton");
	            
                users.Add(skeleton.UserIndex, cubeSkeleton);
            }
			else{
				skeleton.State = SkeletonState.Updated;	
			}
			
        }
        
		if (skeleton.State == SkeletonState.Removed)
        {
    	//print("Removinguser: " + skeleton.UserIndex);
            GameObject[] cubes;
            if (users.TryGetValue(skeleton.UserIndex, out cubes))
            {
                foreach (GameObject cube in cubes)
                {
                    //Destroy(cube);
                }

				//users.Remove(skeleton.UserIndex);
				
				//associatedListener.skeletonBuffer.Remove(skeleton);

            }
        }
        
		
		if (skeleton.State == SkeletonState.Updated) // Update the skeleton
        {
            GameObject[] cubes;
            if (users.TryGetValue(skeleton.UserIndex, out cubes))
            {
                for (int i = 0; i < skeleton.Joints.Count; i++)
                {
                    // Get position of joint
                    Vector3 v3pos = new Vector3(skeleton.Joints[i].Position.X + kinectOffset, skeleton.Joints[i].Position.Y, skeleton.Joints[i].Position.Z);
       //             print("X: "+ v3pos.x +", Y: "+ v3pos.y + ", Z: " + v3pos.z);
					
					//v3pos *= 10;
                    cubes[i].transform.position += (v3pos - cubes[i].transform.position ) * 0.2f;
					
					cubes[i].name = skeleton.Joints[i].ID.ToString();
                }
            }
			
			cubes[0].transform.parent.SendMessage("SkeletonUpdated");
        }
        else if (skeleton.State == SkeletonState.ImageOnly)
        {
			
			Debug.Log("Got an image");
          /*  print("Setting Texture");
            
            // flip the depthmap as we create the texture	
            int i = dstWidth * dstHeight - 1;
            int depthIndex = 0;

           // depthHistogramMap[0] = 0; //Force rawdepth = 0 to black in histogram, avoids conditional check in loop.

            for (int y = 0; y < dstHeight; ++y)
            {
                for (int x = 0; x < dstWidth; ++x, --i, depthIndex += actualFactor)
                {
                    // Fast Method - 39 fps
                    float depthValue = skeleton.UserImage[depthIndex];
                    depthMapColors[i].r = depthColor.r * depthValue;
                    depthMapColors[i].g = depthColor.g * depthValue;
                    depthMapColors[i].b = depthColor.b * depthValue;
                    // depthMapColors[i].a = 1.0f;

                    /*
                    // Slower Method - 31 fps				
                    short pixel = depthMapRaw[depthIndex];
                    Color c = new Color(depthHistogramMap[pixel], depthHistogramMap[pixel], depthHistogramMap[pixel], 1.0f);
                    depthMapColors[i] = depthColor * c;	
                    */
            /*    }
                depthIndex += (actualFactor - 1) * rawWidth; // Skip lines
            }

            depthMapTexture.SetPixels(0, 0, dstWidth, dstHeight, depthMapColors, 0);
            depthMapTexture.Apply(useMipmaps);

*/
        }
    }



    // Support function: Finds the next HIGHEST POT
    int getNextPowerOfTwo(int val)
    {
        val--;
        val = (val >> 1) | val;
        val = (val >> 2) | val;
        val = (val >> 4) | val;
        val = (val >> 8) | val;
        val = (val >> 16) | val;
        val++; // Val is now the next highest power of 2.
        return (val);
    }
}
