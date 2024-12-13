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
	float4 o2 : TEXCOORD2;
    float4 o3 : SV_Position;
};

VS
{
	#define cmp -
    
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
				
		float4 r0,r1,r2;
		uint4 bitmask, uiDest;
		float4 fDest;

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
		r0.xyz = r0.xxx * float3(0.25,0.5,0.5) + float3(0.125,0.25,0.25);
		r0.xyz = frac(r0.xyz);
		r0.xyz = cmp(r0.xyz >= float3(0.5,0.5,0.5));
		r1.yzw = r0.xyz ? float3(1,1,-1) : float3(-1,-1,1);
		r1.x = r0.x ? 2.000000 : 0;
		r0.xy = float2(0,1) + r1.xw;
		r0.xy = float2(0.5,0.5) * r0.xy;
		r2.xyzw = cb0[2].xyzw * r1.zzzz;
		r2.xyzw = cb0[1].xyzw * r1.yyyy + r2.xyzw;
		o.o3.xy = r1.yz;
		r1.xyzw = cb0[3].xyzw * cb12[9].xxxx + r2.xyzw;
		r1.xyzw = cb0[4].xyzw + r1.xyzw;
		o.o0.xyzw = r1.xyzw;
		r2.xyzw = cb12[5].xyzw * r1.yyyy;
		r2.xyzw = cb12[4].xyzw * r1.xxxx + r2.xyzw;
		r2.xyzw = cb12[6].xyzw * r1.zzzz + r2.xyzw;
		o.o1.xyzw = cb12[7].xyzw * r1.wwww + r2.xyzw;
		o.o2.zw = float2(1,1) * float2(0.5,0.5) + r0.xy; //cb0[33].zw * float2(0.5,0.5) + r0.xy;
		o.o2.xy = r0.xy;
		o.o3.z = cb12[9].x;
		o.o3.w = 1;

        return o;
    }
}

PS
{
	#include "postprocess/common.hlsl"
	#include "common/classes/depth.hlsl"
	#define CUSTOM_TEXTURE_FILTERING
    #define cmp -
	
	SamplerState s0_s < Filter(MIN_MAG_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	SamplerState s2_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(WRAP); AddressV(CLAMP); AddressW(WRAP); ComparisonFunc(NEVER); MaxAniso(1); >;
	SamplerState s3_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float4 cb0_0 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/0"); >;
	float4 cb0_1 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/1"); >;
	float4 cb0_2 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/2"); >;
	float4 cb0_3 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/3"); >;
	float4 cb0_4 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/4"); >;
	float4 cb0_5 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/5"); >;
	float4 cb0_6 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/6"); >;
	float4 cb0_7 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/7"); >;
	float4 cb0_8 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/8"); >;
	float4 cb0_9 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/9"); >;
	float4 cb0_10 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/10"); >;
	float4 cb0_11 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/11"); >;
	float4 cb0_12 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/12"); >;
	float4 cb0_13 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/13"); >;
	float4 cb0_14 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/14"); >;
	float4 cb0_15 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/15"); >;
	float4 cb0_16 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/16"); >;
	float4 cb0_17 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/17"); >;
	float4 cb0_18 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/18"); >;
	float4 cb0_19 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/19"); >;
	float4 cb0_20 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/20"); >;
	float4 cb0_21 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/21"); >;
	float4 cb0_22 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/22"); >;
	float4 cb0_23 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/23"); >;
	float4 cb0_24 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/24"); >;
	float4 cb0_25 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/25"); >;
	float4 cb0_26 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/26"); >;
	float4 cb0_27 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/27"); >;
	float4 cb0_28 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/28"); >;
	float4 cb0_29 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/29"); >;
	float4 cb0_30 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/30"); >;
	float4 cb0_31 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/31"); >;
	float4 cb0_32 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/32"); >;
	float4 cb0_33 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/33"); >;
	float4 cb0_34 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/34"); >;
	float4 cb0_35 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/35"); >;
	float4 cb0_36 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/36"); >;
	float4 cb0_37 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/37"); >;
	float4 cb0_38 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/38"); >;
	float4 cb0_39 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/39"); >;
	float4 cb0_40 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/40"); >;
	float4 cb0_41 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/41"); >;
	float4 cb0_42 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/42"); >;
	float4 cb0_43 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/43"); >;
	float4 cb0_44 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/44"); >;
	float4 cb0_45 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/45"); >;
	float4 cb0_46 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/46"); >;
	float4 cb0_47 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/47"); >;
	float4 cb0_48 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/48"); >;
	float4 cb0_49 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/49"); >;
	float4 cb0_50 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/50"); >;
	float4 cb0_51 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/51"); >;
	float4 cb0_52 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/52"); >;
	float4 cb0_53 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/53"); >;
	float4 cb0_54 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/54"); >;
	float4 cb0_55 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/55"); >;
	float4 cb0_56 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/56"); >;
	float4 cb0_57 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/57"); >;
	float4 cb0_58 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/58"); >;
	float4 cb0_59 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/59"); >;
	float4 cb0_60 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/60"); >;
	float4 cb0_61 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb0/61"); >;
	float4 cb13_0 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb13/0"); >;
	float4 cb13_1 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb13/1"); >;
	float4 cb13_2 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb13/2"); >;
	float4 cb13_3 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb13/3"); >;
	float4 cb13_4 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb13/4"); >;
	float4 cb13_5 < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( "cb13/5"); >;


	Texture2D g_t2 < Attribute( "AtmosFar" ); >;
	Texture2D g_t3 < Attribute( "AtmosNear" ); >;

	Texture2D g_t5 < Attribute( "ColorBuffer" ); SrgbRead( true ); >;

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
		float4 v0 = i.o0;
		float4 v1 = i.o1;
		float4 v2 = i.o2;
		float4 v3 = i.o3;
		float3 vCameraPos = g_vCameraPositionWs/39.37;
		
		float4 cb12[15] = {
			g_matWorldToProjection,
			float4(cross(g_vCameraUpDirWs, g_vCameraDirWs),0),
			float4(g_vCameraUpDirWs,0),
			float4(-g_vCameraDirWs,0),
			float4(vCameraPos,1),
			g_matProjectionToView * TargetPixelToProjective( g_vViewportSize ),
			float4(g_vViewportSize, g_vInvViewportSize),
			float4(1,0,0,0),
			float4(vCameraPos,1)
		};

		float4 o0,r0,r1,r2,r3,r4,r5,r6;
		uint4 bitmask, uiDest;
		float4 fDest;
		
		r0.xy = cb12[12].xy * v2.xy;
		r0.xy = (int2)r0.xy;
		r0.zw = float2(0,0);
		r1.xyz = float4(0.5,0.5,1,0).xyz; // t4.Load(r0.xyz).xyz;
		r0.x = Depth::GetNormalized(r0.xyw).x*0.3937; // t0.Load(r0.xyw).x;
		r0.x = r0.x * cb0_61.y + cb0_61.x;
		r0.x = 1 / r0.x;
		r0.xyz = v0.xyz * r0.xxx;
		r0.x = dot(r0.xyz, r0.xyz);
		r0.x = sqrt(r0.x);
		r0.yzw = r1.xyz * float3(2,2,2) + float3(-1,-1,-1);
		r1.x = dot(r0.yzw, r0.yzw);
		r1.x = sqrt(r1.x);
		r0.yzw = r0.yzw / r1.xxx;
		r1.x = r1.x * 4 + -3;
		r1.y = dot(v1.xyz, v1.xyz);
		r1.y = rsqrt(r1.y);
		r1.yzw = v1.xyz * r1.yyy;
		r0.y = dot(r0.yzw, r1.yzw);
		r0.z = saturate(1 + r0.y);
		r0.w = r0.z * r0.z;
		r0.w = r0.w * r0.w;
		r0.z = r0.z * r0.w;
		r0.w = saturate(-0.5 * r1.x);
		r1.x = saturate(r1.x);
		r1.y = 1 + -r1.x;
		r0.w = r1.y + -r0.w;
		r0.w = saturate(r0.w + r0.w);
		r0.w = 1 + -r0.w;
		r1.y = -r0.z * r0.w + 1;
		r0.z = r0.z * r0.w;
		r0.w = r1.x * 0.600000024 + 0.400000006;
		r0.w = r0.w * r1.y;
		r1.y = r0.y * r0.y;
		r2.x = min(1, r1.y);
		r3.xyzw = g_t5.Sample(s0_s, v2.xy).xyzw;
		r1.y = r3.w * 255 + -127.5;
		r2.y = saturate(0.0078125 * r1.y);
		r2.xyzw = float4(1,1,1,1); // t10.SampleLevel(s3_s, r2.xy, 0).xyzw;
		r2.xyzw = r2.wxyz * r2.wxyz;
		r4.xyz = r3.xyz * r2.xxx;
		r1.y = cmp(0.5 < r3.w);
		r4.xyz = r1.yyy ? r4.xyz : r3.xyz;
		r1.z = cmp(r3.w >= 0.995000005);
		r2.x = r1.z ? 0 : 1;
		r5.x = 1;
		r6.xyz = float4(0,0.75,0,0).xyw; // t6.Sample(s0_s, v2.xy).xyw;
		r6.zw = float2(1,1) + -r6.zx;
		r5.yzw = float3(0.0399999991,0.0399999991,0.0399999991) * r6.www;
		r2.xyzw = r1.yyyy ? r2.xyzw : r5.xyzw;
		r2.yzw = r4.xyz * r6.xxx + r2.yzw;
		r4.xyz = r6.www * r4.xyz;
		r6.z = saturate(r6.z);
		r0.x = -r6.z * cb0_50.x + r0.x;
		r0.x = max(0, r0.x);
		r1.y = saturate(r6.y * 2 + -1.00784314);
		r1.y = r1.y * 13 + -7;
		r1.y = exp2(r1.y);
		r1.y = -0.0078125 + r1.y;
		r5.xyz = r2.yzw * r0.www + r0.zzz;
		r0.z = float4(1,0,0,0).x; // t11.Sample(s0_s, v2.xy).x;
		r0.y = -r0.y + r0.z;
		r0.y = log2(r0.y);
		r0.y = r1.x * r0.y;
		r0.y = exp2(r0.y);
		r0.y = r0.y + r0.z;
		r0.y = saturate(-1 + r0.y);
		r0.y = saturate(cb0_51.x * r0.y + cb0_51.y);
		r0.y = r0.y + -r0.z;
		r0.y = r1.x * r0.y + r0.z;
		r0.yzw = r5.xyz * r0.yyy;
		r5.xyz = float4(1,1,1,1).xyz; // t9.Sample(s0_s, v2.xy).xyz;
		r5.xyz = r5.xyz * r2.xxx;
		r5.xyz = cb0_60.xxx * r5.xyz;
		r0.yzw = r5.xyz * r0.yzw;
		r5.xyz = float4(1,1,1,1).xyz; // t8.Sample(s0_s, v2.xy).xyz;
		r5.xyz = min(cb13_5.zzz, r5.xyz);
		r5.xyz = r5.xyz * r2.xxx;
		r0.yzw = r2.yzw * r5.xyz + r0.yzw;
		r2.yzw = float4(1,1,1,1).xyz; // t7.Sample(s0_s, v2.xy).xyz;
		r2.yzw = min(cb13_5.zzz, r2.yzw);
		r2.xyz = r2.yzw * r2.xxx;
		r0.yzw = r4.xyz * r2.xyz + r0.yzw;
		r1.x = cb13_1.x * cb13_1.w;
		r1.x = r1.x * r1.y;
		r0.yzw = r1.xxx * r3.xyz + r0.yzw;
		r1.x = 0.00100000005 * r0.x;
		r1.y = r1.w * r1.x;
		r1.xz = cb0_43.xy * r1.xx;
		r2.xy = cb0_41.xy / r1.ww;
		r2.zw = cb0_42.xy * r1.yy;
		r1.y = cmp(abs(r1.y) < 9.99999975e-006);
		r2.zw = max(float2(-120,-120), r2.zw);
		r2.zw = exp2(-r2.zw);
		r2.zw = float2(1,1) + -r2.zw;
		r2.xy = r2.xy * r2.zw;
		r1.xy = r1.yy ? r1.xz : r2.xy;
		r1.x = r1.x + r1.y;
		r1.yz = cb0_44.xy * r0.xx + cb0_44.zw;
		r0.x = saturate(r0.x * cb0_47.x + cb0_47.y);
		r1.yzw = float4(0.25,0.25,0.25,1).xyz; // t1.Sample(s2_s, r1.yz).xyz;
		r1.xyz = r1.yzw * r1.xxx;
		r2.xyz = cb0_45.xyz * -r1.xyz;
		r1.xyz = exp2(-r1.xyz);
		r2.xyz = exp2(r2.xyz);
		r0.yzw = r2.xyz * r0.yzw;
		r2.xyzw = g_t2.Sample(s1_s, v2.zw).xyzw;
		r3.xyzw = g_t3.Sample(s1_s, v2.zw).xyzw;
		r2.xyzw = -r3.xyzw + r2.xyzw;
		r2.xyzw = r0.xxxx * r2.xyzw + r3.xyzw;
		r0.x = cb0_46.x * r2.w;
		r3.xyz = -r0.xxx * r1.xyz + r2.www;
		r1.xyz = -r2.xyz * r1.xyz + r2.xyz;
		r1.xyz = r3.xyz * cb0_5.xyz + r1.xyz;
		r1.xyz = cb0_10.xxx * r1.xyz;
		o0.xyz = r1.xyz * cb13_1.xxx + r0.yzw;
		o0.w = 1;

		return o0;
    }
}
