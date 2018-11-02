Shader "PostProcess/Displacement"
{
	Properties
	{
	_MainTex("Base (RGB)", 2D) = "white" {}
	_OffsetTex("Offset (RGB)", 2D) = "white" {}
	_OffsetScale("Offset Scale", Range(0, 1)) = 1.0
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
			uniform sampler2D _OffsetTex;
			uniform fixed _OffsetScale;

			float4 frag(v2f_img i) : COLOR
			{
				float2 oUV = i.uv;
				oUV.x = fmod(oUV.x + _Time.y * 21365.0, 1.0);
				oUV.y = fmod(oUV.y + _Time.y * 19837.0, 1.0);

				float4 o = tex2D(_OffsetTex, oUV);
				float2 uv = float2(fmod(i.uv.x + o.r * _OffsetScale, 1.0), i.uv.y);

				fixed4 c = tex2D(_MainTex, uv);
				return fixed4(c.b, 0.6 - c.r * 0.6, 1.0 - c.g, 1.0);
			}
			ENDCG
		}
	}
}
