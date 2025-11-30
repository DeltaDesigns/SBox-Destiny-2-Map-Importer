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

	Texture2D g_t1 < Attribute( "WaterReflectionUV" ); SrgbRead(true); >;
	SamplerState s1_s < Filter(MIN_MAG_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float4 cb0_1 < Attribute( "Water_Healing_UV_RT_Dimensions"); >;
	float4 cb0_3 < Attribute( "Water_Healing_UV_RT_Dimensions"); >;

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
			cb0_1,
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
		r1.xyz = g_t1.Sample(s1_s, r0.xy).xyz;
		r0.z = 500 * r1.z;
		r2.xy = cb12[12].xy * r1.xy;
		r2.xy = (int2)r2.xy;
		r2.zw = float2(0,0);
		r0.w = Depth::GetNormalized(r2.xyz).x; //Depth::GetNormalized(r2.xyz).x;
		r0.w = r0.w * cb0[0].y + cb0[0].x;
		r0.w = 1 / r0.w;
		r0.z = cmp(r0.w < r0.z);
		r0.w = cmp(r1.x == 0.000000);
		r0.z = (int)r0.z | (int)r0.w;
		r2.z = r0.z ? 0 : 1;
		r2.xy = r2.zz * r1.xy;
		if (r0.z != 0) {
		r3.xw = cb0[1].zw;
		r3.yz = float2(1,0);
		r1.xy = cb0[2].xx * r3.xy;
		r1.xy = r1.xy * r3.zw + r0.xy;
		r1.xyw = g_t1.Sample(s1_s, r1.xy).xyz;
		r0.w = 500 * r1.w;
		r3.xy = cb12[12].xy * r1.xy;
		r3.xy = (int2)r3.xy;
		r3.zw = float2(0,0);
		r1.w = Depth::GetNormalized(r3.xyz).x;
		r1.w = r1.w * cb0[0].y + cb0[0].x;
		r1.w = 1 / r1.w;
		r0.w = cmp(r1.w < r0.w);
		r1.w = cmp(r1.x == 0.000000);
		r0.w = (int)r0.w | (int)r1.w;
		r3.z = r0.w ? 0 : 1;
		r3.xy = r3.zz * r1.xy;
		r1.xyw = r3.xyz + r2.xyz;
		} else {
		r2.w = 1;
		r1.xyw = r2.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xy = cb0[2].xx * cb0[1].zw;
		r3.xy = r3.xy * float2(0,-1) + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xy = cb0[2].xx * cb0[1].zw;
		r3.xy = r3.xy * float2(-1,0) + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xw = float2(1,0);
		r3.yz = cb0[1].wz;
		r3.xy = cb0[2].xx * r3.xy;
		r3.xy = r3.xy * r3.zw + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xy = -cb0[2].xx * cb0[1].zw + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xy = cb0[2].xx * cb0[1].zw + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xw = cb0[1].zw;
		r3.yz = float2(1,-1);
		r3.xy = cb0[2].xx * r3.xy;
		r3.xy = r3.xy * r3.zw + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xw = float2(1,-1);
		r3.yz = cb0[1].wz;
		r0.zw = cb0[2].xx * r3.xy;
		r0.zw = r0.zw * r3.zw + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r0.zw).xyz;
		r0.z = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r0.w = Depth::GetNormalized(r4.xyz).x;
		r0.w = r0.w * cb0[0].y + cb0[0].x;
		r0.w = 1 / r0.w;
		r0.z = cmp(r0.w < r0.z);
		r0.w = cmp(r3.x == 0.000000);
		r0.z = (int)r0.z | (int)r0.w;
		r4.z = r0.z ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.z = cmp(r1.w < 1);
		if (r0.z != 0) {
		r3.xy = cb0[2].xx * cb0[1].zw;
		r3.xy = r3.xy * float2(0,2) + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xy = cb0[2].xx * cb0[1].zw;
		r3.xy = r3.xy * float2(0,-2) + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xy = cb0[2].xx * cb0[1].zw;
		r3.xy = r3.xy * float2(-2,0) + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xy = cb0[2].xx * cb0[1].zw;
		r3.xy = r3.xy * float2(2,0) + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r0.w = cb0[2].x * -2;
		r3.xy = r0.ww * cb0[1].zw + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r0.w = cb0[2].x + cb0[2].x;
		r3.xy = r0.ww * cb0[1].zw + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r3.xy = cb0[2].xx * cb0[1].zw;
		r3.xy = r3.xy * float2(-2,2) + r0.xy;
		r3.xyz = g_t1.Sample(s1_s, r3.xy).xyz;
		r0.w = 500 * r3.z;
		r3.zw = cb12[12].xy * r3.xy;
		r4.xy = (int2)r3.zw;
		r4.zw = float2(0,0);
		r2.w = Depth::GetNormalized(r4.xyz).x;
		r2.w = r2.w * cb0[0].y + cb0[0].x;
		r2.w = 1 / r2.w;
		r0.w = cmp(r2.w < r0.w);
		r2.w = cmp(r3.x == 0.000000);
		r0.w = (int)r0.w | (int)r2.w;
		r4.z = r0.w ? 0 : 1;
		r4.xy = r4.zz * r3.xy;
		r1.xyw = r4.xyz + r1.xyw;
		}
		r0.w = cmp(r1.w < 1);
		r0.z = r0.w ? r0.z : 0;
		if (r0.z != 0) {
		r0.zw = cb0[2].xx * cb0[1].zw;
		r0.xy = r0.zw * float2(2,-2) + r0.xy;
		r0.xyz = g_t1.Sample(s1_s, r0.xy).xyz;
		r0.z = 500 * r0.z;
		r3.xy = cb12[12].xy * r0.xy;
		r3.xy = (int2)r3.xy;
		r3.zw = float2(0,0);
		r0.w = Depth::GetNormalized(r3.xyz).x;
		r0.w = r0.w * cb0[0].y + cb0[0].x;
		r0.w = 1 / r0.w;
		r0.z = cmp(r0.w < r0.z);
		r0.w = cmp(r0.x == 0.000000);
		r0.z = (int)r0.z | (int)r0.w;
		r3.z = r0.z ? 0 : 1;
		r3.xy = r3.zz * r0.xy;
		r1.xyw = r3.xyz + r1.xyw;
		}
		o0.w = min(1, r1.w);
		r0.x = cmp(r1.w < 1);
		r2.z = 1;
		r0.xyz = r0.xxx ? r2.xyz : r1.xyw;
		o0.xy = r0.xy / r0.zz;
		o0.z = r1.z;
		//o0.y = 1 - o0.y;
		
		return float4(o0.xyz, o0.w);
    }
}