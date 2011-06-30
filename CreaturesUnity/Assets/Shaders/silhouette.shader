Shader "CC/silhouette" 
{
//    Properties 
//    {
//        _Color ("Main Color", COLOR) = (1,1,1,1)
//    }
    SubShader 
    {
    	Tags { "RenderType"="Opaque" }
    	
        Pass 
        {
//            Material 
//            {
//                Diffuse [_Color]
//            }
            
            Lighting Off
            
            Cull Off
        }
    }
} 

