/*

using UnityEngine;
using System.Collections;


// Just testing the imagemapViewer texture being used as a GUITexture
// This is a roundabout way of doing it, but its just a test.

// For this script to work you'll need to make the following public in OpenNIImagemapViewer
// imageMapTexture
// potWidth
// potHeight

// OpenNIImagemapViewer on the NITE object must be active/enabled.
// Attach this script to a guiTexture gameobject.

// NOTE
// If you want to minimise you'll want to enable mipmaps


public class TestImageonGuiTexture : MonoBehaviour 
{	
	public	OpenNIImagemapViewer	imagemapViewer;
	private	bool assigned = false;
	
	
	void Update()
	{
		if (assigned) return;
		
		if (imagemapViewer.imageMapTexture == null) return;
			assigned =  true;
		guiTexture.texture = imagemapViewer.imageMapTexture;
		guiTexture.pixelInset = new Rect(0, 0, imagemapViewer.potWidth, imagemapViewer.potHeight);
		
	}

}
*/