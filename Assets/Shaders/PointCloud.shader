


Shader "Custom/PointCloud"
{
    Properties
    {
		_PolygonSize("Polygon Size", float) = 0.002
		_PolygonNbSides("Polygon Number Of Sides", Range(3, 24)) = 10

		_PolygonTex("Polygon Texture", 2D) = "white" {}
		_PolygonClippedTex("Polygon Clipped Texture", 2D) = "white" {}

		_ClippingThicknessToAdd("Clipping Thickness To Add", Range(0.0, 1.0)) = 0.0

		[Toggle(INFINITE_CLIPPING_PLANE_ON)] _InfiniteClippingPlaneOn("Infinite Clipping Plane On", float) = 0.0
		[Toggle(FINITE_CLIPPING_PLANE_ON)] _FiniteClippingPlaneOn("Finite Clipping Plane On", float) = 0.0
		[Toggle(HIGHLIGHT_PLANES_ON)] _HighlightPlanesOn("Highlight Planes On", float) = 0.0
		[Toggle(CLIPPING_BOX_ON)] _ClippingBoxOn("Clipping Box On", float) = 0.0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "LightMode" = "ForwardBase"}

        LOD 100			// Level Of Detail

        Pass
        {
            CGPROGRAM

				#pragma vertex vert
				#pragma geometry geom
				#pragma fragment frag

				#pragma shader_feature INFINITE_CLIPPING_PLANE_ON
				#pragma shader_feature FINITE_CLIPPING_PLANE_ON
				#pragma shader_feature HIGHLIGHT_PLANES_ON
				#pragma shader_feature CLIPPING_BOX_ON

				#include "UnityCG.cginc"					// for TRANSFORM_TEX, UnityObjectToWorldNormal, ShadeSH9	
				#include "UnityLightingCommon.cginc"		// for _LightColor0




				// Structure corresponding to a vertex input automatically filled by Unity
				struct vertInput
				{
					float4 pos : POSITION;
					float2 uv : TEXCOORD0;
					float3 normal : NORMAL;
				};

				// Structure corresponding to a vertex output AND a fragment input too
				struct vertOutput
				{
					float4 clipPos : SV_POSITION;			// SV_POSITION semantic is used to indicate output the final clip space position of a vertex, so that the GPU knows where on the screen to rasterize it, and at which depth
					float2 uv : TEXCOORD0;					// TEXCOORD semantics are used to indicate arbitrary high precision data (texture coordinates, positions,...)
#if defined (INFINITE_CLIPPING_PLANE_ON) || defined (FINITE_CLIPPING_PLANE_ON) || defined (HIGHLIGHT_PLANES_ON) || defined (CLIPPING_BOX_ON)
					float colorVal : COLOR0;				// COLOR0 semantics are used to indicate arbitrary low-precision data with 0–1 range (simple color values,...) -> Here diffuse lighting color				
					float3 worldPos : TEXCOORD1;		
#endif
				};

				// Structure corresponding to a fragment output (struct is facultative here because frag shader returns only one value most of the time)
				struct fragOutput
				{
					fixed4 colorVect : SV_Target;			// SV_Target semantic is used to indicate the render target
				};

				float _PolygonSize;
				int _PolygonNbSides;

				// needed for TRANSFORM_TEX use
				fixed4 _MainTex_ST;

				sampler2D _PolygonTex;
				sampler2D _PolygonClippedTex;

#if defined (INFINITE_CLIPPING_PLANE_ON) || defined (FINITE_CLIPPING_PLANE_ON) || defined (HIGHLIGHT_PLANES_ON) || defined(CLIPPING_BOX_ON)
#if defined (INFINITE_CLIPPING_PLANE_ON) || defined (FINITE_CLIPPING_PLANE_ON) || defined (HIGHLIGHT_PLANES_ON)
				float4 _ClipPlaneCenter;
				float4 _ClipPlaneNormal;
				float4 _ClipPlaneCenter2;
				float4 _ClipPlaneNormal2;
				// Projection of w onto the UNIT normal plane vector ??? NOT WORKING ANYMORE ???
				inline float distancePointToPlane(float3 pointInWorld, float4 planeCenter, float4 planeNormal)
				{
					return dot(pointInWorld - planeCenter, planeNormal); // / sqrt(dot(planeNormal, planeNormal)) to obtain a UNIT normal plane vector 
				}

				float _ClippingThicknessToAdd;

				float2 _PlaneXBounds;
				float2 _PlaneYBounds;
				float2 _PlaneZBounds;
				bool isPointInPlane(float3 pointValues)
				{
					return((pointValues.x > min(_PlaneXBounds[0], _PlaneXBounds[1]) - _ClippingThicknessToAdd && pointValues.x < max(_PlaneXBounds[0], _PlaneXBounds[1]) + _ClippingThicknessToAdd) &&
						(pointValues.y > min(_PlaneYBounds[0], _PlaneYBounds[1]) && pointValues.y < max(_PlaneYBounds[0], _PlaneYBounds[1])) &&
						(pointValues.z > min(_PlaneZBounds[0], _PlaneZBounds[1]) && pointValues.z < max(_PlaneZBounds[0], _PlaneZBounds[1])));
				}

				float2 _PlaneXBounds2;
				float2 _PlaneYBounds2;
				float2 _PlaneZBounds2;
				bool isPointInPlane2(float3 pointValues)
				{
					return((pointValues.x > min(_PlaneXBounds2[0], _PlaneXBounds2[1]) - _ClippingThicknessToAdd && pointValues.x < max(_PlaneXBounds2[0], _PlaneXBounds2[1]) + _ClippingThicknessToAdd) &&
						(pointValues.y > min(_PlaneYBounds2[0], _PlaneYBounds2[1]) && pointValues.y < max(_PlaneYBounds2[0], _PlaneYBounds2[1])) &&
						(pointValues.z > min(_PlaneZBounds2[0], _PlaneZBounds2[1]) && pointValues.z < max(_PlaneZBounds2[0], _PlaneZBounds2[1])));
				}

				fixed _ClipBoxLinkedPlaneSide;
				float4 _ClipBoxLinkedPlaneSize;
				float4x4 _ClipBoxLinkedPlaneInverseTransform;

				fixed _ClipBoxLinkedPlaneSide2;
				float4 _ClipBoxLinkedPlaneSize2;
				float4x4 _ClipBoxLinkedPlaneInverseTransform2;
#endif
				fixed _ClipBoxSide;
				float4 _ClipBoxSize;
				float4x4 _ClipBoxInverseTransform;
				inline float distancePointToBox(float3 pointInWorld, float3 boxSize, float4x4 boxInverseTransform)
				{
					float3 distance = abs(mul(boxInverseTransform, float4(pointInWorld, 1.0))) - boxSize; 
					return length(max(distance, 0.0)) + min(max(distance.x, max(distance.y, distance.z)), 0.0);
				}
#endif

				// Vertex shader
				vertOutput vert (vertInput vi)
				{
					vertOutput vo;

					// Transforms a point from object space to the camera’s clip space in homogeneous coordinates
					vo.clipPos = UnityObjectToClipPos(vi.pos);							// <-> mul(UNITY_MATRIX_MVP, float4(pos, 1.0)) with UNITY_MATRIX_MVP = current model "view" projection matrix
					vo.uv = TRANSFORM_TEX(vi.uv, _MainTex);								// make sure texture scale and offset is applied correctly

#if defined (INFINITE_CLIPPING_PLANE_ON) || defined (FINITE_CLIPPING_PLANE_ON) || defined (HIGHLIGHT_PLANES_ON) || defined (CLIPPING_BOX_ON)
					half3 worldNormal = UnityObjectToWorldNormal(vi.normal);
					// Diffuse lambertian reflectance : dot product between normal and light direction
					vo.colorVal = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz)) * _LightColor0;
					//vo.colorVal += ShadeSH9(half4(worldNormal, 1));

					vo.worldPos = mul(unity_ObjectToWorld, vi.pos).xyz;					// unity_ObjectToWorld = current model matrix	
#endif

					return vo;
				}		
			




				// Geometry shader (with max number of vertices that might come out of this geometry shader, including created / deleted vertices during this phase)
				[maxvertexcount(100)]
				void geom(point vertOutput existingVertex[1], inout TriangleStream<vertOutput> polygonStrips)
				{
					const float PI2 = 3.14159 * 2;
					float angle;											// angle between horizontal line and another line that passes by a polygon vertex (both passing by the center of the polygon)
					float xAspect = 1;
					float yAspect = _ScreenParams.x / _ScreenParams.y;		// make square

					// Creation of the first new vertex of the future polygon corresponding to 0PI in the trigonometric circle
					vertOutput first;

					first.uv = existingVertex[0].uv;

#if defined (INFINITE_CLIPPING_PLANE_ON) || defined (FINITE_CLIPPING_PLANE_ON) || defined (HIGHLIGHT_PLANES_ON) 
					float distanceToPlane = distancePointToPlane(existingVertex[0].worldPos.xyz, _ClipPlaneCenter, _ClipPlaneNormal);
					float distanceToPlane2 = distancePointToPlane(existingVertex[0].worldPos.xyz, _ClipPlaneCenter2, _ClipPlaneNormal2);

					if ((abs(distanceToPlane) <= _PolygonSize && isPointInPlane(existingVertex[0].worldPos.xyz)) || (abs(distanceToPlane2) <= _PolygonSize) && isPointInPlane2(existingVertex[0].worldPos.xyz))
						first.clipPos = existingVertex[0].clipPos + float4(cos(0) * xAspect, sin(0) * yAspect, 0, 0) * 2.5 * _PolygonSize;
					else
						first.clipPos = existingVertex[0].clipPos + float4(cos(0) * xAspect, sin(0) * yAspect, 0, 0) * _PolygonSize;
					
					first.colorVal = existingVertex[0].colorVal;
					first.worldPos = existingVertex[0].worldPos;

#elif defined (CLIPPING_BOX_ON)
					float distanceToBox = distancePointToBox(existingVertex[0].worldPos.xyz, _ClipBoxSize.xyz, _ClipBoxInverseTransform) * _ClipBoxSide;
					if (abs(distanceToBox) <= _PolygonSize)
						first.clipPos = existingVertex[0].clipPos + float4(cos(0) * xAspect, sin(0) * yAspect, 0, 0) * 2.5 * _PolygonSize;
					else
						first.clipPos = existingVertex[0].clipPos + float4(cos(0) * xAspect, sin(0) * yAspect, 0, 0) * _PolygonSize;

					first.colorVal = existingVertex[0].colorVal;
					first.worldPos = existingVertex[0].worldPos;

#else
					first.clipPos = existingVertex[0].clipPos + float4(cos(0) * xAspect, sin(0) * yAspect, 0, 0) * _PolygonSize;
#endif
				
					vertOutput second;
					angle = PI2 / _PolygonNbSides;
					second.uv = existingVertex[0].uv;

#if defined (INFINITE_CLIPPING_PLANE_ON) || defined (FINITE_CLIPPING_PLANE_ON) || defined (HIGHLIGHT_PLANES_ON) 
					if ((abs(distanceToPlane) <= _PolygonSize && isPointInPlane(existingVertex[0].worldPos.xyz)) || (abs(distanceToPlane2) <= _PolygonSize) && isPointInPlane2(existingVertex[0].worldPos.xyz))
						second.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * 2.5 *_PolygonSize;
					else
						second.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * _PolygonSize;

					second.colorVal = existingVertex[0].colorVal;
					second.worldPos = existingVertex[0].worldPos;

#elif defined (CLIPPING_BOX_ON)
					if (abs(distanceToBox) <= _PolygonSize)
						second.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * 2.5 *_PolygonSize;
					else
						second.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * _PolygonSize;

					second.colorVal = existingVertex[0].colorVal;
					second.worldPos = existingVertex[0].worldPos;

#else
					second.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * _PolygonSize;
#endif

					int i = 2;
					for(; i <= _PolygonNbSides; i++)
					{
						vertOutput next;
						angle = (PI2 * i) / _PolygonNbSides;
						next.uv = existingVertex[0].uv;

#if defined (INFINITE_CLIPPING_PLANE_ON) || defined (FINITE_CLIPPING_PLANE_ON) || defined (HIGHLIGHT_PLANES_ON) 
						if ((abs(distanceToPlane) <= _PolygonSize && isPointInPlane(existingVertex[0].worldPos.xyz)) || (abs(distanceToPlane2) <= _PolygonSize) && isPointInPlane2(existingVertex[0].worldPos.xyz))
							next.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * 2.5 * _PolygonSize;
						else
							next.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * _PolygonSize;

						next.colorVal = existingVertex[0].colorVal;
						next.worldPos = existingVertex[0].worldPos;

#elif defined (CLIPPING_BOX_ON)
						if (abs(distanceToBox) <= _PolygonSize)
							next.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * 2.5 * _PolygonSize;
						else
							next.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * _PolygonSize;

						next.colorVal = existingVertex[0].colorVal;
						next.worldPos = existingVertex[0].worldPos;
#else
						next.clipPos = existingVertex[0].clipPos + float4(cos(angle) * xAspect, sin(angle) * yAspect, 0, 0) * _PolygonSize;
#endif

						// Add a complementary triangle adjacent to the previous one stored in TriangleStream output (except for the first loop pass)
						polygonStrips.Append(first);
						polygonStrips.Append(second);
						polygonStrips.Append(next);
						second = next;
					}	

					// Can be commented; function that explicitely end current polygon (stored as a bunch of adjacent triangles previously computed) strip
					polygonStrips.RestartStrip();
				}





				// Fragment shader
				fragOutput frag (vertOutput vo)
				{
					fragOutput fo;

#if defined(INFINITE_CLIPPING_PLANE_ON) 
					float distanceToPlane = distancePointToPlane(vo.worldPos.xyz, _ClipPlaneCenter, _ClipPlaneNormal);
					float distanceToPlane2 = distancePointToPlane(vo.worldPos.xyz, _ClipPlaneCenter2, _ClipPlaneNormal2);
					if (distanceToPlane < -_PolygonSize)			
					{
						clip(distanceToPlane);
					}
					else if (distanceToPlane2 < -_PolygonSize)
					{
						clip(distanceToPlane2);
					}
					else if ((abs(distanceToPlane) <= _PolygonSize && isPointInPlane(vo.worldPos.xyz)) || (abs(distanceToPlane2) <= _PolygonSize && isPointInPlane2(vo.worldPos.xyz)))
					{
						fo.colorVect = tex2D(_PolygonClippedTex, vo.uv);
						fo.colorVect *= vo.colorVal;
					}
					else
					{
						fo.colorVect = tex2D(_PolygonTex, vo.uv);
					}
#elif defined (FINITE_CLIPPING_PLANE_ON) 
					float distanceToPlane = distancePointToPlane(vo.worldPos.xyz, _ClipPlaneCenter, _ClipPlaneNormal);
					float distanceToPlane2 = distancePointToPlane(vo.worldPos.xyz, _ClipPlaneCenter2, _ClipPlaneNormal2);
					float distanceToBoxLinkedPlane = distancePointToBox(vo.worldPos.xyz, _ClipBoxLinkedPlaneSize.xyz, _ClipBoxLinkedPlaneInverseTransform) * _ClipBoxLinkedPlaneSide;
					float distanceToBoxLinkedPlane2 = distancePointToBox(vo.worldPos.xyz, _ClipBoxLinkedPlaneSize2.xyz, _ClipBoxLinkedPlaneInverseTransform2) * _ClipBoxLinkedPlaneSide2;

					if (distanceToBoxLinkedPlane < -_PolygonSize)			// if the vertex is in the zone formed by the width and height of the plane number 1 and of infinite width (so in the direction of the normal)
					{
						clip(distanceToBoxLinkedPlane);
					}
					else if (distanceToBoxLinkedPlane2 < -_PolygonSize)			// if the vertex is in the zone formed by the width and height of the plane number 1 and of infinite width (so in the direction of the normal)
					{
						clip(distanceToBoxLinkedPlane2);
					}
					else if ((abs(distanceToPlane) <= _PolygonSize && isPointInPlane(vo.worldPos.xyz)) || (abs(distanceToPlane2) <= _PolygonSize && isPointInPlane2(vo.worldPos.xyz)))
					{
						fo.colorVect = tex2D(_PolygonClippedTex, vo.uv);
						fo.colorVect *= vo.colorVal;
					}
					else
					{
						fo.colorVect = tex2D(_PolygonTex, vo.uv);
					}

					//clip(fo.colorVect.a);					
					//fo.colorVect.a = 1.0;
#elif defined (HIGHLIGHT_PLANES_ON)
					float distanceToPlane = distancePointToPlane(vo.worldPos.xyz, _ClipPlaneCenter, _ClipPlaneNormal);
					float distanceToPlane2 = distancePointToPlane(vo.worldPos.xyz, _ClipPlaneCenter2, _ClipPlaneNormal2);

					if (distanceToPlane < -_PolygonSize || distanceToPlane2 < -_PolygonSize)
					{
						fo.colorVect = tex2D(_PolygonTex, vo.uv);
						fo.colorVect *= vo.colorVal;
					}
					else if (((abs(distanceToPlane) <= _PolygonSize && isPointInPlane(vo.worldPos.xyz)) || (abs(distanceToPlane2) <= _PolygonSize && isPointInPlane2(vo.worldPos.xyz)))
						 || ((distanceToPlane > _PolygonSize && !isPointInPlane(vo.worldPos.xyz)) || (distanceToPlane2 > _PolygonSize && !isPointInPlane2(vo.worldPos.xyz))))
					{
						fo.colorVect = tex2D(_PolygonClippedTex, vo.uv);
						fo.colorVect *= vo.colorVal;
					}
#elif defined (CLIPPING_BOX_ON)
					float distanceToBox = distancePointToBox(vo.worldPos.xyz, _ClipBoxSize.xyz, _ClipBoxInverseTransform) * _ClipBoxSide;

					if (distanceToBox < -_PolygonSize) 			// if the vertex is in the zone formed by the width and height of the box and of infinite width behind box (so in the opposite direction of the normal)
					{
						clip(distanceToBox);
					}
					else if (abs(distanceToBox) <= _PolygonSize)
					{
						fo.colorVect = tex2D(_PolygonClippedTex, vo.uv);
						fo.colorVect *= vo.colorVal;
					}
					else
					{
						fo.colorVect = tex2D(_PolygonTex, vo.uv);
					}
#else
					fo.colorVect = tex2D(_PolygonTex, vo.uv);
#endif

					return fo;
				}

            ENDCG
        }
    }
}
