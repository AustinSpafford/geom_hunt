﻿Shader "Custom/AdditiveOutline"
{
	Properties
	{
		_OutlineColor("OutlineColor", Color) = (1,0,1,1)
		_OutlineThickness("OutlineThickness", Range(0.0, 0.03)) = 0.005
	}

	SubShader
	{
		Pass
		{
			Name "OUTLINE"

			Tags { "Queue"="Transparent" }
			Cull Front
			Blend OneMinusDstColor One // Low-saturation additive blending.
			ZTest LEqual
			ZWrite On // We need to perform a write, otherwise the skybox would stomp us.

			CGPROGRAM

			#pragma vertex vertex_func
			#pragma fragment fragment_func
			#pragma multi_compile_fog // support fog effects
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 position : POSITION;
				float4 normal : NORMAL;
			};

			struct vertex_to_fragment
			{
				UNITY_FOG_COORDS(1)
				float4 position : SV_POSITION;
			};

			fixed4 _OutlineColor;
			float _OutlineThickness;
			
			vertex_to_fragment vertex_func (appdata input)
			{
				vertex_to_fragment output;
				output.position = mul(UNITY_MATRIX_MVP, input.position);
				
				// Horizontally offset the vertex position in view-space.
				{
					float3 viewspace_normal = mul((float3x3)UNITY_MATRIX_IT_MV, input.normal);
					float2 projectionspace_normal = TransformViewToProjection(viewspace_normal.xy);
					float distance_corrected_outline_OutlineThickness = (output.position.z * _OutlineThickness);

					output.position.xy += (projectionspace_normal * distance_corrected_outline_OutlineThickness);
				}
				
				UNITY_TRANSFER_FOG(output, output.position);

				return output;
			}
			
			fixed4 fragment_func (vertex_to_fragment input) : SV_Target
			{
				fixed4 output = _OutlineColor;

				UNITY_APPLY_FOG(input.fogCoord, output);

				return output;
			}

			ENDCG
		}
	}
}