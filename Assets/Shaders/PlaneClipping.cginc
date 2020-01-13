#ifndef PLANE_CLIPPING_INCLUDED
#define PLANE_CLIPPING_INCLUDED

#if CLIPPING_ON

// Returns the distance of a point to a plane defined by its position and its normal (http://mathworld.wolfram.com/Point-PlaneDistance.html)
float distanceToPlane(float3 planePosition, float3 planeNormal, float3 pointInWorld)
{
	// Computation of a vector w from plane to point
	float3 w = -(planePosition - pointInWorld);

	// Projection of w onto the normal plane vector to obtain the distance from point to plane
	return dot(planeNormal, w) / sqrt(dot(planeNormal, planeNormal));
}

float4 _ClipPlanePosition;
float4 _ClipPlaneNormal;

// Discards the pixel given in parameter if clip function is called with any number less than zero (because if the distance to the plane is less than zero, a point is behind the plane)
void PlaneClip(float3 posWorld) 
{
	clip(distanceToPlane(_ClipPlanePosition.xyz, _ClipPlaneNormal.xyz, posWorld));
}

// Preprocessor macro that will produce an empty block if clipping is off
#define PLANE_CLIP(posWorld) PlaneClip(posWorld);

#else

// Empty definition
#define PLANE_CLIP(s)

#endif

#endif
