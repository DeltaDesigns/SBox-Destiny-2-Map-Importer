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

	Texture2D g_t1 < Attribute( "FrameBuffer" ); SrgbRead(true); >;
	Texture2D g_t2 < Attribute( "WaterReflectionUVHealed" ); SrgbRead(true); >;
	
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	SamplerState s2_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float4 cb0_1 < Attribute( "Framebuffer_RT_Dimensions"); >;
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
			cb0_1, //float4(g_vViewportSize, g_vInvViewportSize),
			float4(1,1,1,1),
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

		r0.zw = float2(0,0);
		r1.xyzw = cb0[3].zwzw * v0.xyxy;
		r2.xyzw = cb0[2].xxxx * float4(-0.5,-0.5,0.5,-0.5);
		r3.xyzw = r2.xyzw * cb0[1].zwzw + r1.zwzw;
		r1.xyzw = r2.wzzz * cb0[1].zwzw + r1.xyzw;
		r2.xyzw = g_t2.Sample(s2_s, r3.xy).xyzw;
		r3.xyzw = g_t2.Sample(s2_s, r3.zw).xyzw;
		r4.xy = cb12[12].xy * r2.xy;
		r0.xy = (int2)r4.xy;
		r0.x = Depth::GetNormalized(r0.xyz).x;
		r0.x = r0.x * cb0[0].y + cb0[0].x;
		r0.x = 1 / r0.x;
		r0.y = 500 * r2.z;
		r0.x = cmp(r0.x < r0.y);
		r0.x = r0.x ? 0 : 1;
		r0.w = r0.x * r2.w;
		r2.xyz = g_t1.Sample(s1_s, r2.xy).xyz;
		r0.xyz = r2.xyz * r0.www;
		r2.xy = cb12[12].xy * r3.xy;
		r2.xy = (int2)r2.xy;
		r2.zw = float2(0,0);
		r2.x = Depth::GetNormalized(r2.xyz).x;
		r2.x = r2.x * cb0[0].y + cb0[0].x;
		r2.x = 1 / r2.x;
		r2.y = 500 * r3.z;
		r2.x = cmp(r2.x < r2.y);
		r2.x = r2.x ? 0 : 1;
		r2.w = r2.x * r3.w;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r2.xyz = r3.xyz * r2.www;
		r0.xyzw = r2.xyzw + r0.xyzw;
		r2.zw = float2(0,0);
		r3.xyzw = g_t2.Sample(s2_s, r1.xy).xyzw;
		r1.xyzw = g_t2.Sample(s2_s, r1.zw).xyzw;
		r4.xy = cb12[12].xy * r3.xy;
		r2.xy = (int2)r4.xy;
		r2.x = Depth::GetNormalized(r2.xyz).x;
		r2.x = r2.x * cb0[0].y + cb0[0].x;
		r2.x = 1 / r2.x;
		r2.y = 500 * r3.z;
		r2.x = cmp(r2.x < r2.y);
		r2.x = r2.x ? 0 : 1;
		r2.w = r2.x * r3.w;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r2.xyz = r3.xyz * r2.www;
		r0.xyzw = r2.xyzw + r0.xyzw;
		r2.xy = cb12[12].xy * r1.xy;
		r2.xy = (int2)r2.xy;
		r2.zw = float2(0,0);
		r2.x = Depth::GetNormalized(r2.xyz).x;
		r2.x = r2.x * cb0[0].y + cb0[0].x;
		r2.x = 1 / r2.x;
		r1.z = 500 * r1.z;
		r1.z = cmp(r2.x < r1.z);
		r1.z = r1.z ? 0 : 1;
		r2.w = r1.z * r1.w;
		r1.xyz = g_t1.Sample(s1_s, r1.xy).xyz;
		r2.xyz = r1.xyz * r2.www;
		r0.xyzw = r2.xyzw + r0.xyzw;
		r1.x = max(9.99999975e-005, r0.w);
		o0.xyz = r0.xyz / r1.xxx;
		o0.w = 0.25 * r0.w;

		return float4(o0.xyz, o0.w);
    }
}