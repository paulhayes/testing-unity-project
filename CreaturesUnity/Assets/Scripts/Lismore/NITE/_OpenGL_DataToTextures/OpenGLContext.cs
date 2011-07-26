using UnityEngine;
using System;
using System.Runtime.InteropServices; 

public class OpenGLContext
{
	static readonly OpenGLContext instance = new OpenGLContext();
	
	static OpenGLContext()
	{ 
		MonoBehaviour.print("Static constructor - OpenGLContext");
	}
	
	OpenGLContext()
	{
		MonoBehaviour.print("normal constructor - OpenGLContext");
		Init();
	}
	
	~OpenGLContext()
	{
		MonoBehaviour.print("Destroying context - OpenGLContext");
	}
	
	public static OpenGLContext Instance
	{
		get {return instance;}
	}
	
		// OpenGL enums as const ints.
	public  int GL_TEXTURE_2D 				= 0x0DE1;
	public  int GL_RGB 						= 0x1907;
	public  int GL_RGBA 					= 0x1908;
	public  int GL_UNSIGNED_BYTE 			= 0x1401;
	public  int GL_UNSIGNED_SHORT_4_4_4_4 	= 0x8033;
		
	[System.Runtime.InteropServices.DllImport("opengl32.dll", EntryPoint = "glBindTexture", ExactSpelling = true)]
    internal extern static void BindTexture(int target, uint textureID);
       
    [System.Runtime.InteropServices.DllImport("opengl32.dll", EntryPoint = "glTexSubImage2D", ExactSpelling = true)]
    internal extern static void TexSubImage2D(int target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, int format, int type, IntPtr pixels);
			
	private void Init()
	{				
		MonoBehaviour.print("OpenGLContext inited");
	}
	
	public void SetOpenGLTexture2 (int target, Int32 level, Int32 xoffset, Int32 yoffset, Int32 width, Int32 height, int format, int type, IntPtr pixels,  uint index)
	{
		BindTexture(GL_TEXTURE_2D,  index);
		TexSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixels); 		
	}
	
	public void SetOpenGLTexture (IntPtr p,  uint index, int width, int height)
	{
		BindTexture(GL_TEXTURE_2D,  index);
		TexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, width, height, GL_RGB, GL_UNSIGNED_BYTE, p); 		
	}
}
