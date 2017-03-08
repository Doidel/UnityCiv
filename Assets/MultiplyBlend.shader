Shader "Hidden/MultiplyBlend"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaskTex("Base (RGB)", 2D) = "mask" {}
	}
	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off

CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

	uniform sampler2D _MainTex;
	uniform sampler2D _MaskTex;
	uniform sampler2D _PlayerViewTex;

	fixed4 frag(v2f_img i) : SV_Target
	{
		fixed4 original = tex2D(_MainTex, i.uv);
		fixed alpha = tex2D(_MaskTex, half2(i.uv.r, 1.0 - i.uv.g)).r;
		fixed view = 0.6f + tex2D(_PlayerViewTex, half2(i.uv.r, 1.0 - i.uv.g)).r * 0.4f;
		fixed4 output = original * alpha * view;// +fog * (1.0 - alpha) * 0.8;
		output.a = original.a;
		return output;
	}
ENDCG
		}
	}
	Fallback off
}
