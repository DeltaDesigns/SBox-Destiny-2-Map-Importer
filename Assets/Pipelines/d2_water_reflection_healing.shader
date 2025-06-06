MODES
{
    VrForward();
}

FEATURES
{
}
COMMON
{
    #include "postprocess/shared.hlsl"
	#define TO_INCHES 39.3700787
}

struct VertexInput
{
	uint v0 : SV_VertexID < Semantic( VertexID ); >;
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float4 o0 : SV_Position;
	float4 o1 : TEXCOORD0; // Custom semantic to pass screen position
};

VS
{
	#define cmp -
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        float4 r0,r1;
		
		float3 vCameraPos = g_vCameraPositionWs/39.37;
		float4 cb12[10] = {
			g_matWorldToProjection,
			float4(cross(g_vCameraUpDirWs, -g_vCameraDirWs),1),
			float4(g_vCameraUpDirWs,1),
			float4(-g_vCameraDirWs,1),
			float4(vCameraPos,1),
			float4(g_vViewportSize, g_vInvViewportSize),
			float4(1,0,0,0)
		};
		
		r0.x = (uint)i.v0.x;
		r0.xy = r0.xx * float2(-0.25,0.5) + float2(-0.125,0.25);
		r0.xy = frac(r0.xy);
		r0.xy = cmp(r0.xy >= float2(0.5,0.5));
		r0.xy = r0.xy ? float2(1,1) : 0;
		o.o0.xy = r0.xy * float2(2,2) + float2(-1,-1);
		o.o0.z = cb12[9].x;
		o.o0.w = 1;
					
        return o;
    }
}
PS
{
	#include "postprocess/common.hlsl"
	#include "common/classes/Depth.hlsl"
	#define CUSTOM_TEXTURE_FILTERING
    #define cmp -
	
	RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );

	Texture2D g_t0 < Attribute( "WaterReflectionResolved" ); SrgbRead(true); >;
	SamplerState s1_s < Filter(MIN_MAG_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	float4 cb0_3 < Attribute( "Water_Resolve_RT_Dimensions"); >;

	float4x4 TargetPixelToProjective(float2 size)
	{
		return float4x4(
			2.0f / size.x,  0.0f,          0.0f, 	0.0f,
			0.0f,          -2.0f / size.y, 0.0f, 	0.0f,
			0.0f,           0.0f,          1.0f,	0.0f,
			-1.0f,          1.0f,          0.0f, 	1.0f
		);
	}

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float4 o0,r0,r1,r2,r3,r4,r5,r6;

		float4 v0 = i.o0; 
		
		float4 cb0[4] = 
		{
			float4((1 / g_flFarPlane)*TO_INCHES, ((g_flFarPlane - g_flNearPlane) / (g_flFarPlane * g_flNearPlane))*TO_INCHES,0,0),
			cb0_3, //float4(g_vViewportSize, g_vInvViewportSize),
			float4(5,5,5,5),
			cb0_3
		};
		
		float3 vCameraPos = g_vCameraPositionWs/TO_INCHES;
		float4x4 mWorldToProj = transpose(g_matWorldToProjection);
		float4 cb12[15] = {
			mWorldToProj,
			float4(cross(g_vCameraUpDirWs, -g_vCameraDirWs),0),
			float4(g_vCameraUpDirWs,0),
			float4(-g_vCameraDirWs,0),
			float4(vCameraPos,1),
			g_matProjectionToView * TargetPixelToProjective( g_vViewportSize ),
			float4(g_vViewportSize, g_vInvViewportSize),
			float4(1,0,0,0),
			float4(vCameraPos,1)
		};

		r0.xy = cb0[3].zw * v0.xy;
		r1.xyzw = g_t0.Sample(s1_s, r0.xy).xyzw;
		r1.xyz = r1.xyz * r1.www;
		r0.z = cmp(r1.w < 1);
		if (r0.z != 0) {
		r2.xw = cb0[1].zw;
		r2.yz = float2(1,0);
		r2.xy = cb0[2].xx * r2.xy;
		r2.xy = r2.xy * r2.zw + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xy = cb0[2].xx * cb0[1].zw;
		r2.xy = r2.xy * float2(0,-1) + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xy = cb0[2].xx * cb0[1].zw;
		r2.xy = r2.xy * float2(-1,0) + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xw = float2(1,0);
		r2.yz = cb0[1].wz;
		r2.xy = cb0[2].xx * r2.xy;
		r2.xy = r2.xy * r2.zw + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xy = -cb0[2].xx * cb0[1].zw + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xy = cb0[2].xx * cb0[1].zw + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xw = cb0[1].zw;
		r2.yz = float2(1,-1);
		r2.xy = cb0[2].xx * r2.xy;
		r2.xy = r2.xy * r2.zw + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xw = float2(1,-1);
		r2.yz = cb0[1].wz;
		r0.zw = cb0[2].xx * r2.xy;
		r0.zw = r0.zw * r2.zw + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r0.zw).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.z = cmp(r1.w < 1);
		if (r0.z != 0) {
		r2.xy = cb0[2].xx * cb0[1].zw;
		r2.xy = r2.xy * float2(0,2) + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xy = cb0[2].xx * cb0[1].zw;
		r2.xy = r2.xy * float2(0,-2) + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xy = cb0[2].xx * cb0[1].zw;
		r2.xy = r2.xy * float2(-2,0) + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xy = cb0[2].xx * cb0[1].zw;
		r2.xy = r2.xy * float2(2,0) + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r0.w = cb0[2].x * -2;
		r2.xy = r0.ww * cb0[1].zw + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r0.w = cb0[2].x + cb0[2].x;
		r2.xy = r0.ww * cb0[1].zw + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r2.xy = cb0[2].xx * cb0[1].zw;
		r2.xy = r2.xy * float2(-2,2) + r0.xy;
		r2.xyzw = g_t0.Sample(s1_s, r2.xy).xyzw;
		r2.xyz = r2.xyz * r2.www;
		r1.xyzw = r2.xyzw + r1.xyzw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r0.zw = cb0[2].xx * cb0[1].zw;
		r0.xy = r0.zw * float2(2,-2) + r0.xy;
		r0.xyzw = g_t0.Sample(s1_s, r0.xy).xyzw;
		r0.xyz = r0.xyz * r0.www;
		r1.xyzw = r1.xyzw + r0.xyzw;
		}
		r0.x = cmp(r1.w == 0.000000);
		r1.xyz = r1.xyz / r1.www;
		r1.w = 1;
		o0.xyzw = r0.xxxx ? float4(0,0,0,0) : r1.xyzw;

		return float4(o0.xyz, o0.w);
    }
}