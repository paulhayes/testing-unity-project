using UnityEngine;
using UnityEditor;
using System.Collections;


// this class must extend the AssetPostprocessor
// http://unity3d.com/support/documentation/ScriptReference/AssetPostprocessor.html


class CustomImportSettings : AssetPostprocessor {
	
	// looks like this didn't need to change anyway
    float customImportScale  = 0.1f;
    
    void OnPreprocessModel() 
	{
		
        ModelImporter importer = (ModelImporter) assetImporter;
	
		importer.globalScale = customImportScale;
	
    }
}