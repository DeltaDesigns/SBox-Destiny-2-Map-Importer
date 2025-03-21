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
	float4 o1 : TEXCOORD1; // Custom semantic to pass screen position
    float4 o2 : SV_Position;
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
		
		float4 cb0[4] = {
			g_matProjectionToView
		};
		
		r0.x = (uint)i.v0.x;
		r0.xy = r0.xx * float2(-0.25,0.5) + float2(-0.125,0.25);
		r0.xy = frac(r0.xy);
		r0.xy = cmp(r0.xy >= float2(0.5,0.5));
		r0.xy = r0.xy ? float2(1,1) : 0;
		r0.xy = r0.xy * float2(2,2) + float2(-1,-1);
		r1.xyzw = cb0[1].xyzw * r0.yyyy;
		r1.xyzw = cb0[0].xyzw * r0.xxxx + r1.xyzw;
		o.o2.xy = r0.xy;
		r0.xyzw = cb0[2].xyzw * cb12[9].xxxx + r1.xyzw;
		r0.xyzw = cb0[3].xyzw + r0.xyzw;
		o.o0.xyzw = r0.xyzw;
		r1.xyzw = cb12[5].xyzw * r0.yyyy;
		r1.xyzw = cb12[4].xyzw * r0.xxxx + r1.xyzw;
		r1.xyzw = cb12[6].xyzw * r0.zzzz + r1.xyzw;
		o.o1.xyzw = cb12[7].xyzw * r0.wwww + r1.xyzw;
		o.o2.z = cb12[9].x;
		o.o2.w = 1;
		
        //o.o1 = float4( i.vPositionOs.xy, 1.0f, 1.0f );
        //o.o2 = i.vPositionOs.xyzz; // Compute screen space position
        //o.uv = i.vTexCoord;
        return o;
    }
}

PS
{
    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );
	#include "postprocess/common.hlsl"
	#include "common/classes/_classes.hlsl"
	#define CUSTOM_TEXTURE_FILTERING
    #define cmp -
	
    Texture3D g_t0 < Attribute( "AtmosTexture0" ); SrgbRead(true); >;
	Texture3D g_t1 < Attribute( "AtmosTexture1" ); SrgbRead(true); >;
	Texture2D g_t2 < Attribute( "AtmosTexture2" ); SrgbRead(true); >;
	Texture2D g_t3 < Attribute( "AtmosTexture3" ); SrgbRead(true); >;
    
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	SamplerState s2_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(WRAP); AddressV(CLAMP); AddressW(WRAP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float4 cb0_0 < Default4(0,0,0,0); UiGroup( "cb0/0"); >;
	float4 cb0_1 < Default4(0,0,0,0); UiGroup( "cb0/1"); >;
	float4 cb0_2 < Default4(0,0,0,0); UiGroup( "cb0/2"); >;
	float4 cb0_3 < Default4(0,0,0,0); UiGroup( "cb0/3"); >;
	float4 cb0_4 < Default4(0,0,0,0); UiGroup( "cb0/4"); >;
	float4 cb0_5 < Default4(-0.8365f, -0.8365f, -0.8365f, -0.8365f ); UiGroup( "cb0/5"); >;
	float4 cb0_6 < Default4(0.05923f, 0.05923f, 0.05923f, 0.05923f ); UiGroup( "cb0/6"); >;
	float4 cb0_7 < Default4(0,0,0,0); UiGroup( "cb0/7"); >;
	float4 cb0_8 < Default4(0,0,0,0); UiGroup( "cb0/8"); >;
	float4 cb0_9 < Default4(0,0,0,0); UiGroup( "cb0/9"); >;
	float4 cb0_10 < Default4(0,0,0,0); UiGroup( "cb0/10"); >;
	float4 cb0_11 < Default4(0,0,0,0); UiGroup( "cb0/11"); >;
	float4 cb0_12 < Default4(0,0,0,0); UiGroup( "cb0/12"); >;
	float4 cb0_13 < Default4(0,0,0,0); UiGroup( "cb0/13"); >;
	float4 cb0_14 < Default4(0,0,0,0); UiGroup( "cb0/14"); >;
	float4 cb0_15 < Default4(0,0,0,0); UiGroup( "cb0/15"); >;
	float4 cb0_16 < Default4(0,0,0,0); UiGroup( "cb0/16"); >;
	float4 cb0_17 < Default4(0,0,0,0); UiGroup( "cb0/17"); >;
	float4 cb0_18 < Default4(0,0,0,0); UiGroup( "cb0/18"); >;
	float4 cb0_19 < Default4(0,0,0,0); UiGroup( "cb0/19"); >;
	float4 cb0_20 < Default4(0,0,0,0); UiGroup( "cb0/20"); >;
	float4 cb0_21 < Default4(0,0,0,0); UiGroup( "cb0/21"); >;
	
	
	float4 cb0_24 < Default4(0.33713, 0.33713,0.33713,0.33713); UiGroup( "cb0/24"); >;
	float4 cb0_25 < Default4(0,0,0,0); UiGroup( "cb0/25"); >;
	
	float4 cb0_27 < Default4(0,0,0,0); UiGroup( "cb0/27"); >;
	float4 cb0_28 < Default4(0,0,0,0); UiGroup( "cb0/28"); >;
	float4 cb0_29 < Default4(-0.30372, -0.59835f, 0.74144f, 0.0f ); UiGroup( "cb0/29"); >;
	float4 cb0_30 < Default4(0,0,0,0); UiGroup( "cb0/30"); >;
	float4 cb0_31 < Default4(0,0,0,0); UiGroup( "cb0/31"); >;
	float4 cb0_32 < Default4(480.0f, 270.0f, 0.00208f, 0.0037f ); UiGroup( "cb0/32"); >;
	float4 cb0_33 < Default4(0,0,0,0); UiGroup( "cb0/33"); >;
	float4 cb0_34 < Default4(0,0,0,0); UiGroup( "cb0/34"); >;
	float4 cb0_35 < Default4(0,0,0,0); UiGroup( "cb0/35"); >;
	float4 cb0_36 < Default4(0,0,0,0); UiGroup( "cb0/36"); >;
	float4 cb0_37 < Default4(0,0,0,0); UiGroup( "cb0/37"); >;
	float4 cb0_38 < Default4(0.05923f, 0.05923f, 0.05923f, 0.05923f ); UiGroup( "cb0/38"); >;
	float4 cb0_39 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/39"); >;
	float4 cb0_40 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/40"); >;
	float4 cb0_41 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/41"); >;
	float4 cb0_42 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/42"); >;
	float4 cb0_43 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/43"); >;
	float4 cb0_44 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/44"); >;
	float4 cb0_45 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/45"); >;
	float4 cb0_46 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/46"); >;
	float4 cb0_47 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/47"); >;
	float4 cb0_48 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/48"); >;
	float4 cb0_49 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/49"); >;
	float4 cb0_50 < Default4(0.92537f, 0.0f, 0.37906f, 0.37906f ); UiGroup( "cb0/50"); >;
	float4 cb0_51 < Default4(-0.22681f, 0.80123f, 0.5537f, 0.5537f ); UiGroup( "cb0/51"); >;
	float4 cb0_52 < Default4(-0.30372, -0.59835, 0.74144, 0.74144 ); UiGroup( "cb0/52"); >;
	
	float4 cb0_22 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); Attribute("AtmosRotation"); >;
	float4 cb0_23 < Default4(1.0f, 1.0f, 1.0f, 1.0f ); Attribute("AtmosIntensity"); >;
	float4 cb0_26 < Default4(0.5f, 0.5f, 0.5f, 0.5f ); Attribute("AtmosTimeOfDay"); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float4 v1 = i.o1;
		float4 v2 = i.o2;
		float4 o0,r0,r1,r2,r3;

		r0.x = dot(v1.xyz, v1.xyz);
		r0.x = rsqrt(r0.x);
		r0.xyz = v1.xyz * r0.xxx;
		r0.w = abs(r0.x) + -abs(r0.y);
		r1.x = abs(r0.x) + abs(r0.y);
		r0.w = r0.w / r1.x;
		r1.x = cmp(9.99999975e-006 < r1.x);
		r0.w = r0.w * 0.125 + 0.125;
		r1.yz = cmp(r0.xy >= float2(0,0));
		r1.yz = r1.yz ? float2(1,1) : float2(-1,-1);
		r0.w = r0.w * r1.y + 0.25;
		r0.w = r0.w * r1.z;
		r0.w = frac(r0.w);
		r0.w = 1 + -r0.w;
		r0.w = r1.x ? r0.w : 0.5;
		r0.w = cb0_22.x + r0.w;
		r1.x = -r0.w;
		r1.z = cb0_26.x;
		r1.y = r0.z * -0.5 + 0.5;
		r2.xyzw = g_t1.Sample(s2_s, r1.xyz).xyzw;
		r1.xyzw = g_t0.Sample(s2_s, r1.xyz).xyzw;
		r2.xyzw = r2.xyzw + -r1.xyzw;
		r1.xyzw = cb0_47.xxxx * r2.xyzw + r1.xyzw;
		r1.xyzw = cb0_23.xxxx * r1.xyzw;
		r2.xy = float2(-0.5,-0.5) + v2.xy;
		r2.zw = cb0_32.zw * float2(0.5,0.5);
		r2.xy = r2.xy * cb0_32.zw + r2.zw;
		r0.w = normalize(1-Depth::Get(v2.xy * float2(-0.5,-0.5) + float2(0.5,0.5)) * 50000);//g_t2.Sample(s1_s, r2.xy).x;
		r2.x = -1 + r0.w;
		r2.y = dot(r0.xyz, -cb0_29.xyz);
		r2.z = r2.y * -0.5 + 0.5;
		r2.y = -r2.y * 1.99899995 + cb0_5.x;
		r2.y = cb0_5.x * r2.y + 1;
		r2.y = log2(r2.y);
		r2.y = -1.5 * r2.y;
		r2.y = exp2(r2.y);
		r1.w = r2.y * r1.w;
		r1.w = cb0_6.x * r1.w;
		r1.w = min(512, r1.w);
		r2.y = log2(r2.z);
		r2.y = cb0_48.x * r2.y;
		r2.y = exp2(r2.y);
		r2.z = saturate(cb0_24.x * r2.y);
		r2.y = saturate(cb0_39.x * r2.y);
		r2.x = r2.z * r2.x + 1;
		r1.xyz = r2.xxx * r1.xyz;
		r2.x = dot(cb0_52.xyz, r0.xyz);
		r3.z = 1 + r2.x;
		r3.x = dot(cb0_50.xyz, r0.xyz);
		r3.y = dot(cb0_51.xyz, r0.xyz);
		r0.x = dot(r3.xyz, r3.xyz);
		r0.x = rsqrt(r0.x);	
		r0.xy = r3.xy * r0.xx;
		r0.xy = r0.xy * float2(0.5,0.5) + float2(0.5,0.5);
		r0.x = g_t3.Sample(s1_s, r0.xy).x;
		r0.y = -1 + r0.x;
		r0.x = r0.w * r0.x;
		o0.w = r1.w * r0.x;
		r0.x = r2.y * r0.y + 1;
		r0.y = cmp(0.000000 == cb0_38.x);
		r0.x = r0.y ? 1 : r0.x;
		r0.yzw = r1.xyz * r0.xxx;
		r1.xyz = -r1.xyz * r0.xxx + cb0_35.xyz;
		o0.xyz = cb0_35.www * r1.xyz + r0.yzw;

		return o0;
    }
}