Shader "Custom/AdditiveOutline"
{
	Properties
	{
		_Color("Color", Color) = (1,0,1,1)
		_Thickness("Thickness", Range(0.0, 0.03)) = 0.005
	}

	SubShader
	{
		Tags { "Queue"="Transparent" }
		Cull Front
		Blend OneMinusDstColor One // Low-saturation additive blending.
		ZTest LEqual
		ZWrite On // We need to perform a write, otherwise the skybox would stomp us.

		Pass
		{
			CGPROGRAM

			#pragma vertex vertex_func
			#pragma fragment fragment_func
			#pragma multi_compile_fog // support fog effects
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct vertex_to_fragment
			{
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;
			
			vertex_to_fragment vertex_func (appdata input)
			{
				vertex_to_fragment output;
				output.vertex = mul(UNITY_MATRIX_MVP, input.vertex);
				
				UNITY_TRANSFER_FOG(output, output.vertex);

				return output;
			}
			
			fixed4 fragment_func (vertex_to_fragment input) : SV_Target
			{
				fixed4 output = _Color;

				UNITY_APPLY_FOG(input.fogCoord, output);

				return output;
			}

			ENDCG
		}
	}
}
