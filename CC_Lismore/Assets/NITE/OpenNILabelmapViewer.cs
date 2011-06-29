// OpenNILabelmapViewer
// Author: Noisecrime
// Date:   18.06.11

// Summary
// Display Kinect User Label as image using Unity's native methods (SetPixels) to copy and resize the source data into a texture.
// Scaling image is acheived by skipping rows/cols, so it is either fullsize, half-size, quater-size etc.
// Optimised version to get best performance.

// Implementation:
// Mipmaps are disable as default to avoid re-calculating them each time the texture is updated.
// Only enable if you really need mipmaps as their creation each time adveserly affects performance.

// Texture for the User Label is forced to next power-of-two as Unity does not natively support non-pot textures.
// If you use a non-pot dimension then internally Unity will create two versions which is in-efficient!
// Ensuring our texture is a power of two can gain 30% performance, sometimes more.

// Further performance can be gained if you set the texture format to RGB24 instead of ARGB32.
// This will have the downside that the texture will not have an alpha and therefore any border due to POT will be visible.

// Further Optimisations
// Could try building a POT color array and avoid cropped SetPixels() function?


using UnityEngine;
using System;
using System.Runtime.InteropServices;
using OpenNI;

public class OpenNILabelmapViewer : MonoBehaviour 
{
	private OpenNIContext 		Context;
	public 	OpenNIUserTracker 	UserTracker;
	
	public 	int 				XPos			= 20;
	public 	int 				YPos 			= 20;
	
    public 	int					desiredFactor   = 4;	        // User determined scaling of source (kinect) image.
	public	bool				forcePowerOfTwo = true;			// Default: True (faster), False is much slower.
	public	bool				useMipmaps 		= false;		// Default: False (faster), True is slower, but lets you scale texture.

	private	Texture2D 			labelMapTexture;	            // Unity Texture for displaying Kinect label.
	private	Color[] 			labelMapColors;		            // Unity colors array for kinect label.
	private	short[]				labelMapRaw;		            // Array of shorts to hold Kinect label source. 
		
	private int					actualFactor = 4;	            // User determined scaled forced to power-of-two, i.e. 1,2,4,8 etc	
	private	int 				rawWidth;			            // Width of kinect source image in pixels.
	private	int 				rawHeight;			            // Height of kinect source image in pixels.
	private	int 				potWidth;			            // Width of Power-of-two Unity Texture.
	private	int 				potHeight;			            // Height of Power-of-two Unity Texture.
	private	int 				dstWidth;			            // Width of scaled Image in Unity.
	private	int 				dstHeight;			            // Height of scaled Image in Unity.

	void Start () 
	{
		Context = OpenNIContext.Instance;
		
		// Force Factor to a power of two 1,2,4,8 etc
		actualFactor 		= getNextPowerOfTwo(desiredFactor);

		// Get DepthMap size - assume same for labelmap - assuming depth is needed for label???
		MapOutputMode mom 	= Context.Depth.MapOutputMode;
		rawWidth			= mom.XRes;
		rawHeight 			= mom.YRes;
		dstWidth			= rawWidth/actualFactor;
		dstHeight			= rawHeight/actualFactor;
			
		// Force Texture to be Power-Of-Two to avid Unity making two copies
        potWidth            = forcePowerOfTwo ? getNextPowerOfTwo(dstWidth) : dstWidth;
        potHeight           = forcePowerOfTwo ? getNextPowerOfTwo(dstHeight) : dstHeight;
		labelMapTexture 	= new Texture2D(potWidth, potHeight, TextureFormat.ARGB32 , useMipmaps); 

         // Force filter mode to Bilinear to avoid (trilinear) mipmaps		
        labelMapTexture.filterMode =  useMipmaps ? FilterMode.Trilinear : FilterMode.Bilinear;

		// depthmap data
		labelMapRaw 		= new short[rawWidth * rawHeight];
		labelMapColors 		= new Color[dstWidth*dstHeight];
		
		 // Pre-fill Unity Texture to use no alpha. To hide the area not used in the POT texture.
        Color[] txColorArray = labelMapTexture.GetPixels();
		
		for(int i=0; i< txColorArray.Length;i++)
			txColorArray[i].a = 0.0f;
			
		labelMapTexture.SetPixels(txColorArray);
		
		// Pre-fill Unity color array to use full alpha.
		for (int i=0; i<(dstWidth*dstHeight); i++)		
        {
			labelMapColors[i].a = 1.0f;
		}
		
		print(	"Label: Factor: " + actualFactor + " (" + desiredFactor+ ") - Src Width: " + rawWidth + "   Src Height: " + rawHeight + 
				"  Dst Width: " + dstWidth + "   Dst Height: " + dstHeight + "  Tx Width: " + potWidth + "   Tx Height: " + potHeight +
				"     Filter: " + labelMapTexture.filterMode + "  Format: " + labelMapTexture.format + "  MipMaps:" +labelMapTexture.mipmapCount);
		}
	
	// Update is called once per frame
	void Update () 
    {
		Context.Update();
	}
	
	void UpdateLabelMapTexture()
    {
		// flip the depthmap as we create the texture	
		int i 			= dstWidth*dstHeight-1;
		int rawIndex 	= 0;
		
		for (int y = 0; y < dstHeight; ++y)
		{
			for (int x = 0; x < dstWidth; ++x, --i, rawIndex += actualFactor)
			{	
				// What is max user count?
				switch (labelMapRaw[rawIndex]) 
				{
					case 0: labelMapColors[i] = Color.clear; break;
					case 1: labelMapColors[i] = Color.red; break;
					case 2: labelMapColors[i] = Color.green; break;
					case 3: labelMapColors[i] = Color.blue; break;
					case 4: labelMapColors[i] = Color.magenta; break;
					case 5: labelMapColors[i] = Color.cyan; break;
					case 6: labelMapColors[i] = Color.yellow; break;
					default:
						labelMapColors[i] = Color.white; break;
				}														
			}
			rawIndex += (actualFactor-1)*rawWidth; // Skip lines
		}
		
		labelMapTexture.SetPixels(0, 0, dstWidth, dstHeight, labelMapColors, 0);
        labelMapTexture.Apply( useMipmaps );
   }
   
   void OnGUI()
   { 
	   if (Event.current.type == EventType.Repaint)
		{
            Marshal.Copy(UserTracker.userGenerator.GetUserPixels(0).LabelMapPtr, labelMapRaw, 0, labelMapRaw.Length);
			UpdateLabelMapTexture();
		
			GUI.Box(new Rect(Screen.width-dstWidth-XPos, Screen.height-dstHeight-YPos, dstWidth+8, dstHeight+8), "");
			GUI.DrawTexture(new Rect(Screen.width-dstWidth-XPos+4, Screen.height-dstHeight-YPos+4-((potHeight-dstHeight)), potWidth, potHeight), labelMapTexture);
		}
   }
   
   
   
	// Support function: Finds the next HIGHEST POT
	int getNextPowerOfTwo (int val)
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
