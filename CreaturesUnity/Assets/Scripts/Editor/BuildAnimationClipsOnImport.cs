
// 	Built by Matt Ditton 2011
// 	mattditton@gmail.com
//
//	Creates animation clips on a model based on a text file
//	
//	- only works on models in an "animated" subfolder
//	- looks for a file called "list.txt"
//	- makes a lot of assumptions on the make up of the rows and columns
//
//	TODO: fix those assumptions
//
// 	-----------------------------------------------------------


using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;



public class BuildAnimationClipsOnImport : AssetPostprocessor
{	
	
	void OnPostprocessModel (GameObject g) 
	{
		if(assetPath.Contains("/animated/"))
		{
			
			string[] path = (string[])assetPath.Split('/');
			
			path[path.Length-1] = "list.txt";
			
			string filePath = string.Join("/", path);
			
			List<anim_struct> anims = ParseFile(filePath, g.name);
			
			ModelImporter mi = (assetImporter as ModelImporter);
			ModelImporterClipAnimation[] newClip = new ModelImporterClipAnimation[anims.Count];
	
			mi.splitAnimations = true;
			
			for(int i = 0; i < anims.Count; i++){
						
				newClip[i] = new ModelImporterClipAnimation();
				
				newClip[i].name = anims[i].name;
				newClip[i].firstFrame = anims[i].start;
				newClip[i].lastFrame = anims[i].end;
				newClip[i].loop = anims[i].loop;
				newClip[i].wrapMode = (anims[i].loop) ? WrapMode.Loop : WrapMode.Once;
				
				//AssetDatabase.SetLabels(newClip[i], anims[i].labels);
				
			}
		
			mi.clipAnimations = newClip;
			
		}
	}
	
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
	{
		foreach (string str in importedAssets)
            Debug.Log("Reimported Asset: " + str);

        foreach (string str in deletedAssets)
            Debug.Log("Deleted Asset: " + str);
		
	}
	
	private List<anim_struct> ParseFile(string fileName, string filter)
	{
	    
	    TextAsset textAsset = (TextAsset)Resources.LoadAssetAtPath(fileName, typeof(TextAsset));
		
		if (textAsset == null) 
		{
			Debug.Log(fileName + " file not found.");
			return null;
		}
		
	    StringReader reader = new StringReader(textAsset.text);
		
		List<anim_struct> lines = new List<anim_struct>();
		
	    string temp = reader.ReadLine();
	
	    while(temp != null)
	    {
			if(temp.ToLower().Contains(filter.ToLower()))
			{
				string[] vals = temp.Split(',');
				
				anim_struct a  = new anim_struct();
				a.name = vals[3];
				a.start = int.Parse(vals[5]);
				a.end = int.Parse(vals[6]);
				a.loop = bool.Parse(vals[7]);
				a.labels = vals[10].Split(' ');
				
				// turn this on if looped animations are out one frame
				if(a.loop) a.end--;
				
		        lines.Add(a);
			}
			
			temp = reader.ReadLine();
			
			if(temp.Length == 0){
				break;	
			}
	    }
				
	    return lines;
	}
}
