Shader "ncp/openNI/kinectLabelShader" 
{
Properties {
	_MainTex ("Base (RGBA)", 2D) = "white" {}
}

SubShader {
	Pass {
		Fog { Mode off }
				
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"

			struct v2f {
	    		float4  pos : SV_POSITION;
	    		float2  uv : TEXCOORD0;
			};
	
			float4 _MainTex_ST;
	
			v2f vert (appdata_base v)
			{
	   		 v2f o;
	   		 o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	   		 o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
	   		 return o;
			}
			
			uniform sampler2D _MainTex;
			float4 output;
			
			float4 frag (v2f_img i) : COLOR 
			{
				float4 pixcol = tex2D(_MainTex, i.uv);
				
				// openNI label Map is uShort, each value being a unique user index, where 0=background.
				// As openNI probably can't identify more than a few poeple in fov this is overkill
				// Might just get away with using pixcol.a*15 that would give up to 15 users.
				int label = (pixcol.r*15.0*4096.0 + pixcol.g*15.0*256.0 + pixcol.b*15.0*16.0 + pixcol.a*15.0);
				output.r = 1.0 * (label % 4);
				
				if(output.r == 0)
				{
					output.r = 1;
					output.g = 1;
					output.b = 1;
				}
				else
				{
					output.r = 0;
				}
				
				
				// output.r = any(pixcol);
				output.a = 1;   
				return (output);
			}
		ENDCG

	}
}

Fallback off

}