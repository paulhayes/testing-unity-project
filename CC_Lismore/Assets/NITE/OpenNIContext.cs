using UnityEngine;
using System;
using System.IO;
//using System.Collections;
using OpenNI;

public class OpenNIContext
{
	static readonly OpenNIContext instance = new OpenNIContext();
	
	static OpenNIContext()
	{ 
		MonoBehaviour.print("Static constructor - OpenNIContext");
	}
	
	OpenNIContext()
	{
		MonoBehaviour.print("normal constructor - OpenNIContext");
		Init();
	}
	
	~OpenNIContext()
	{
		MonoBehaviour.print("Destroying context - OpenNIContext");
	}
	
	public static OpenNIContext Instance
	{
		get {return instance;}
	}
	
	public string 				OpenNIXMLFilename = ".\\OpenNI_ImageDepth.xml";
	public Context 				context;	
	public DepthGenerator 		Depth;
	public ImageGenerator 		Image;

	private MirrorCapability 	mirror;
	private bool 				validContext = false;
	
	
	public bool Mirror
	{
		get { return mirror.IsMirrored(); }
		set { mirror.SetMirror(value); }
	}
		
	public bool ValidContext
	{
		get { return validContext; }
	}
		
	private void Init()
	{
		// Check file exists
		if (File.Exists(OpenNIXMLFilename))
			this.context = new Context(OpenNIXMLFilename);
		else
		{
			MonoBehaviour.print("OpenNI Error: Unable to find openNI xml file");
			Application.Quit();
		}
		
		if (null == context)
		{
			return;
		}
		
		this.Depth	= new DepthGenerator(this.context);
		this.Image	= new ImageGenerator(this.context);		
		this.mirror	= this.Depth.MirrorCapability;
		
		MonoBehaviour.print("OpenNI inited");
		
		validContext = true;
		
		Start();
	}

	
	void Start () 
	{
		if (validContext)	this.context.StartGeneratingAll();
	}
	
	// Update is called once per frame
	public void Update () 
	{
		if (validContext)
		{
			// MonoBehaviour.print("Update Context");
			this.context.WaitNoneUpdateAll();
		}
	}
}
