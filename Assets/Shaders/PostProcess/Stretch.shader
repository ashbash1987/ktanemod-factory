Shader "PostProcess/Stretch"
{
	Properties
	{
	_MainTex("Base (RGB)", 2D) = "white" {}
	_StretchTex("Stretch Lookup (RGB)", 2D) = "white" {}
	_Stretch("Stretch", Float) = 1.0
	_Vignette("Vignette", Float) = 0.0
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _StretchTex;
			uniform float _Stretch;
			uniform float _Vignette;

			float4 frag(v2f_img i) : COLOR
			{
				float stretch = ((_Stretch - 1.0) * tex2D(_StretchTex, i.uv)) + 1.0;

				float2 uvOffset = float2(i.uv.x - 0.5, i.uv.y - 0.5);
				float2 newUV = float2(0.5 + uvOffset.x * stretch, 0.5 + uvOffset.y * stretch);

				float4 c = tex2D(_MainTex, newUV);
				c = lerp(c, float4(0, 0, 0, 1), _Vignette * length(uvOffset));
				return c;
			}
			ENDCG
		}
	}
}
