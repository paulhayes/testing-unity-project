
// 	Built for GFS Games by Matt Ditton 2010
// 	mattditton@gmail.com
//
//	Sets the importer to only create a material from a model material 
//
//	Prevents textures from creating unique materials
//
// 	-----------------------------------------------------------


using UnityEngine;
using UnityEditor;

public class DisableTextureMaterials : AssetPostprocessor
{	
	 void OnPreprocessModel () 
	{
		ModelImporter modelImporter = assetImporter as ModelImporter;
		modelImporter.generateMaterials = ModelImporterGenerateMaterials.PerSourceMaterial;
	}
}
