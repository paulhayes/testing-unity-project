// Opengl_LabelToTexture
// Author: Noisecrime
// Date:   18.06.11

// Summary
// Transfer kinect label data directly into a Unity Texture when using the opengl renderer.
// This is done purely to enhance performance over Unity's Native setPixels.
// Requires special shader to render it with any meaning.

// Implementation:
// User Label type in openNI is uShort (16bits), so we use a Unity texture of ARGB4444 to match type (i.e 16bits).
// In opengl we use GL_RGBA and GL_UNSIGNED_SHORT_4_4_4_4 to match Unity format ARGB4444.
// However it does mean the texture produced is difficult to interpret when rendered normally.
// It requires a specialised shader to rebuild the depth value from being split into 4 channels/components of 4 bits each.
// This means the texture can no longer be used for GUI elements and must be placed on a mesh.

// Image can not be resized as we are transfering all data via a pointer, resizing it would defeat the purpose.
// Obviously the final texture can be scaled and manipulated like any other texture when applied to a material.

// Enforced Power-Of-Two dimensions for the unity texture to avoid Unity making two internal copies.
// Mipmap updating is not currently supported, though it might be possible to add in the future using onboard gpu features.

// How to force opengl renderer:
// 		Create a shortcut to the exe.
// 		Append '-force-opengl' to the end of the shortcut Target property. 'Click Apply'
// 		Run exe from shortcut, in the Editor (3.0) it will display <opengl> in the windows title bar.

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using OpenNI;

public class Opengl_LabelToTexture : MonoBehaviour 
{
	private OpenNIContext 		Context;
	private OpenGLContext 		openGL;
	
	public 	OpenNIUserTracker 	UserTracker;	
	public	Material			targetMaterial;
	
	[NonSerialized]
	public	Texture2D 			labelMapTexture;			// Unity Texture for displaying Kinect image.
	
	private	int 				rawWidth;					// Width of kinect source image  in pixels.
	private	int 				rawHeight;					// Height of kinect source image in pixels.
	private	int 				potWidth;					// Width of Unity Texture as next Power-of-two.
	private	int 				potHeight;					// Height of Unity Texture as next Power-of-two.
	
	void Start () 
	{
		Context = OpenNIContext.Instance;
		openGL  = OpenGLContext.Instance;

		// Get DepthMap size - assume same for labelmap - assuming depth is needed for label???
		MapOutputMode mom 	= Context.Depth.MapOutputMode;
		rawWidth			= mom.XRes;
		rawHeight 			= mom.YRes;
				
		// Force Texture to be Power-Of-Two to avid Unity making two copies
		potWidth 			= getNextPowerOfTwo( rawWidth  );
        potHeight           = getNextPowerOfTwo( rawHeight );
		labelMapTexture 	= new Texture2D(potWidth, potHeight, TextureFormat.ARGB4444, false);
        
        // Force filter mode to Bilinear to avoid (trilinear) mipmaps
		labelMapTexture.filterMode = FilterMode.Bilinear;		

		// Set up texture in material
		targetMaterial.mainTexture = labelMapTexture;
		
		// Flip and scale texture
		float uScale =  rawWidth/(float)potWidth;
		float vScale =  rawHeight/(float)potHeight;		
		targetMaterial.SetTextureScale ("_MainTex", new Vector2 (-uScale, -vScale) );   // (0.625, 0 ) (-0.937, -0.063)
		targetMaterial.SetTextureOffset("_MainTex", new Vector2 (uScale - 1.0f, vScale-1.0f) );
				
		print("Depth Opengl: Src Width: " + rawWidth + "   Src Height: " + rawHeight + "  Tx Width: " + potWidth + "   Tx Height: " + potHeight);
	}
	
	// Update is called once per frame
	void Update () 
	{
		Context.Update();
		UpdateMapTexture();
	}
	
	void UpdateMapTexture()
    {
		IntPtr 	pixels = UserTracker.userGenerator.GetUserPixels(0).LabelMapPtr;
		uint	index  = (uint)(labelMapTexture.GetNativeTextureID()+0);		
		openGL.SetOpenGLTexture2(openGL.GL_TEXTURE_2D, 0, 0, 0, rawWidth, rawHeight, openGL.GL_RGBA, openGL.GL_UNSIGNED_SHORT_4_4_4_4, pixels, index ); 
	}
	
/*
	// Just left for debuging
   void OnGUI()
   {
	   // print ("GUI - ImageMap");
	   
	   if (Event.current.type == EventType.Repaint)
		{
			UpdateMapTexture();
			   	
			GUI.Box(new Rect(Screen.width-rawWidth-20, Screen.height-rawHeight-20, rawWidth+8, rawHeight+8), "");
			GUI.DrawTexture(new Rect(Screen.width-rawWidth-20+4, Screen.height-rawHeight-20+4-((potHeight-rawHeight)), potWidth, potHeight), labelMapTexture);
		}
   }
*/
   
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
