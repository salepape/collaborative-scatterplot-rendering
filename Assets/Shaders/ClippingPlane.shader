


Shader "Custom/ClippingPlane"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_Transparency("Transparency", Range(0.0, 1.0)) = 0.25
	}

	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		LOD 100

		ZWrite Off								// Not rendering on the depth buffer
		Blend SrcAlpha OneMinusSrcAlpha			// Pixels blended with traditional transparency

		Pass
		{
			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				struct vertexInput
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct vertexOuput
				{
					float4 vertex : SV_POSITION;

					UNITY_FOG_COORDS(1)
					UNITY_VERTEX_OUTPUT_STEREO
				};

				sampler2D _MainTex;
				float4 _Color;
				float _Transparency;

				vertexOuput vert(vertexInput vi)
				{
					vertexOuput vo;
					UNITY_SETUP_INSTANCE_ID(vi);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vo);
					vo.vertex = UnityObjectToClipPos(vi.vertex);
					UNITY_TRANSFER_FOG(vo, vo.vertex);

					return vo;
				}

				fixed4 frag(vertexOuput vo) : SV_Target
				{
					fixed4 colorVect = _Color;
					colorVect.a = _Transparency;

					UNITY_APPLY_FOG(vo.fogCoord, col);

					return colorVect;
				}

			ENDCG
		}
	}
}
