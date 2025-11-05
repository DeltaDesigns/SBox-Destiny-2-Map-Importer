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
}

struct VertexInput
{
	uint v0 : SV_VertexID < Semantic( VertexID ); >;
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float4 o0 : TEXCOORD0;
	float4 o1 : TEXCOORD1;
	
	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs		: SV_Position;
	#endif

	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs		: SV_Position;
	#endif
};

VS
{
	#define cmp -
    
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
		float4 r0,r1;
		o.vPositionPs = float4( i.vPositionOs.xy, 0.0f, 1.0f );
		
		float3 vCameraPos = g_vCameraPositionWs/39.37;
		float4 cb12[10] = {
			g_matWorldToProjection,
			float4(cross(g_vCameraUpDirWs, -g_vCameraDirWs),1),
			float4(g_vCameraUpDirWs,1),
			float4(-g_vCameraDirWs,1),
			float4(vCameraPos,1),
			float4(g_vViewportSize, g_vInvViewportSize),
			float4(0,1,0,0)
		};
		
		r0.x = (uint)i.v0.x;
		r0.xy = r0.xx * float2(0.25,0.5) + float2(0.125,0.25);
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
	#include "common/classes/_classes.hlsl"
	#define CUSTOM_TEXTURE_FILTERING

	
	RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );
	
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	Texture2D g_t4 < Attribute( "RadialBlur8" ); SrgbRead(false); >;
    
	float4 cb0_3 < Default4(0,0,0,0); Attribute( "LightShaftDir"); >;
	float4 cb0_10 < Default4(240.00, 135.00, 0.00417, 0.00741); Attribute( "RTDimensions"); >;
	
	float4 cb0_5 < Default4(0.84737, 0.72063, 0.61491, 0.52224); Attribute( "QualitySteps1"); >;
	float4 cb0_6 < Default4(0.44253, 0.37634, 0.32113, 0.27273); Attribute( "QualitySteps2"); >;
	float4 cb0_7 < Default4(0,0,0,0); Attribute( "QualitySteps3"); >;
	float4 cb0_8 < Default4(0,0,0,0); Attribute( "QualitySteps4"); >;
	float4 cb0_9 < Default4(0.24284,0,0,0); Attribute( "QualitySteps5"); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
    {
		float4 v0 = i.vPositionSs;
		float4 o0,r0,r1,r2,r3,r4;
		
		float4 cb0_0 = float4(0.00, 0.00, 0.00, 0.00);         
		float4 cb0_1 = float4(0.08, 0.08, 0.08, 0.08);         
		float4 cb0_2 = float4(0.05867, 0.05867, 0.05867, 0.05867);     
		float4 cb0_4 = float4(0.6666667, 0.00, 0.00, 0.00);     
		
		r0.z = 2;
		r1.z = 0.5;
		r1.xy = cb0_10.zw * v0.xy;
		r2.xyz = float3(1,1,1) + -r1.xyz;
		r3.xy = -r1.xy * cb0_3.zz + cb0_3.xy;
		r0.xy = float2(1,1) / r3.xy;
		r2.xyz = r2.xyz * r0.xyz;
		r0.xyz = -r1.xyz * r0.xyz;
		r0.xyz = max(r2.xyz, r0.xyz);
		r0.xy = min(r0.xx, r0.yz);
		r0.x = min(r0.x, r0.y);
		r0.xy = r0.xx * r3.xy;
		r0.xy = cb0_1.xx * r0.xy;
		r0.z = dot(r0.xy, r0.xy);
		r0.xy = cb0_4.xx * r0.xy;
		r0.z = sqrt(r0.z);
		r0.w = 9.99999997e-007 + r0.z;
		r0.z = min(cb0_2.x, r0.z);
		r0.xy = r0.xy / r0.ww;
		r0.xy = r0.xy * r0.zz;
		r0.zw = v0.xy * cb0_10.zw + r0.xy;
		r2.xyzw = g_t4.Sample(s1_s, r0.zw).xyzw;
		r2.xyzw = cb0_5.yyyy * r2.xyzw;
		r3.xyzw = g_t4.Sample(s1_s, r1.xy).xyzw;
		r2.xyzw = cb0_5.xxxx * r3.xyzw + r2.xyzw;
		r0.zw = r0.xy * float2(2,2) + r1.xy;
		r3.xyzw = g_t4.Sample(s1_s, r0.zw).xyzw;
		r2.xyzw = cb0_5.zzzz * r3.xyzw + r2.xyzw;
		r3.xyzw = r0.xyxy * float4(3,3,4,4) + r1.xyxy;
		r4.xyzw = g_t4.Sample(s1_s, r3.xy).xyzw;
		r3.xyzw = g_t4.Sample(s1_s, r3.zw).xyzw;
		r2.xyzw = cb0_5.wwww * r4.xyzw + r2.xyzw;
		r2.xyzw = cb0_6.xxxx * r3.xyzw + r2.xyzw;
		r3.xyzw = r0.xyxy * float4(5,5,6,6) + r1.xyxy;
		r4.xyzw = g_t4.Sample(s1_s, r3.xy).xyzw;
		r3.xyzw = g_t4.Sample(s1_s, r3.zw).xyzw;
		r2.xyzw = cb0_6.yyyy * r4.xyzw + r2.xyzw;
		r2.xyzw = cb0_6.zzzz * r3.xyzw + r2.xyzw;
		r3.xyzw = r0.xyxy * float4(7,7,8,8) + r1.xyxy;
		r4.xyzw = g_t4.Sample(s1_s, r3.xy).xyzw;
		r3.xyzw = g_t4.Sample(s1_s, r3.zw).xyzw;
		r2.xyzw = cb0_6.wwww * r4.xyzw + r2.xyzw;
		r2.xyzw = cb0_7.xxxx * r3.xyzw + r2.xyzw;
		r3.xyzw = r0.xyxy * float4(9,9,10,10) + r1.xyxy;
		r0.xy = r0.xy * float2(11,11) + r1.xy;
		r0.xyzw = g_t4.Sample(s1_s, r0.xy).xyzw;
		r1.xyzw = g_t4.Sample(s1_s, r3.xy).xyzw;
		r3.xyzw = g_t4.Sample(s1_s, r3.zw).xyzw;
		r1.xyzw = cb0_7.yyyy * r1.xyzw + r2.xyzw;
		r1.xyzw = cb0_7.zzzz * r3.xyzw + r1.xyzw;
		r0.xyzw = cb0_7.wwww * r0.xyzw + r1.xyzw;
		o0.xyzw = cb0_9.xxxx * r0.xyzw;

		return o0;
    }
}