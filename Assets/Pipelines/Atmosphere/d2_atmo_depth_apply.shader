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

	float CurrentTime < Attribute( "CurrentTime" ); Default1( 0.0 ); >;
	float FrameTimeOfDay < Attribute( "FrameTimeOfDay" ); Default1( 0.5 ); >;
	float ExposureScale < Attribute( "ExposureScale" ); Default1( 0.65 ); >;
	float ExposureIllumRelative < Attribute( "ExposureIllumRelative" ); Default1( 1 ); >;
	
    float4 GlobalChannel25 < Attribute("GlobalChannel25"); Default4(40, 0, 0, 0); >;
	float4 GlobalChannel138 < Attribute("GlobalChannel138"); Default4(0.1, 0, 0, 0); >;
	float4 GlobalChannel139 < Attribute("GlobalChannel139"); Default4(1, 0, 0, 0); >;
	float4 GlobalChannel41 < Attribute("GlobalChannel41"); Default4(0, 0, 0, 0); >;
	float4 GlobalChannel37 < Attribute("GlobalChannel37"); Default4(49500, 0, 0, 0); >;
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
	float4 o2 : TEXCOORD2;
    float4 o3 : SV_Position;
};

VS
{
	#define cmp -
    
	float4 cb0_29 < Default4(480.0f, 270.0f, 0.00208f, 0.0037f ); Attribute( "AtmosRTDimensions"); >;

    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
		float4 r0,r1,r2,r3,r4,r5,r6,r7,r8,r9,r10;
		
		float3 vCameraPos = g_vCameraPositionWs/39.37;
		float4 cb12[10] = {
			g_matWorldToProjection,
			float4(cross(g_vCameraUpDirWs, -g_vCameraDirWs),1),
			float4(g_vCameraUpDirWs,1),
			float4(g_vCameraDirWs,1),
			float4(vCameraPos,1),
			float4(g_vViewportSize, g_vInvViewportSize),
			float4(0,0,0,0)
		};
		
		float4 cb0[4] = {
			g_matProjectionToView
		};
		
		r0.x = (uint)i.v0.x;
		r0.xyz = r0.xxx * float3(-0.25,0.5,0.5) + float3(-0.125,0.25,0.25);
		r0.xyz = frac(r0.xyz);
		r0.xyz = cmp(r0.xyz >= float3(0.5,0.5,0.5));
		r1.yzw = r0.xyz ? float3(1,1,-1) : float3(-1,-1,1);
		r1.x = r0.x ? 2.000000 : 0;
		r0.xy = float2(0,1) + r1.xw;
		r0.xy = float2(0.5,0.5) * r0.xy;
		r2.xyzw = cb0[1].xyzw * r1.zzzz;
		r2.xyzw = cb0[0].xyzw * r1.yyyy + r2.xyzw;
		o.o3.xy = r1.yz;
		r1.xyzw = cb0[2].xyzw * cb12[9].xxxx + r2.xyzw;
		r1.xyzw = cb0[3].xyzw + r1.xyzw;
		o.o0.xyzw = r1.xyzw;
		r2.xyzw = cb12[5].xyzw * r1.yyyy;
		r2.xyzw = cb12[4].xyzw * r1.xxxx + r2.xyzw;
		r2.xyzw = cb12[6].xyzw * r1.zzzz + r2.xyzw;
		o.o1.xyzw = cb12[7].xyzw * r1.wwww + r2.xyzw;
		o.o2.zw = cb0_29.zw * float2(0.5,0.5) + r0.xy;
		o.o2.xy = r0.xy;
		o.o3.z = cb12[9].x;
		o.o3.w = 1;
	
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
	
	Texture2D g_t1 < Attribute( "AtmosDensityLookup" ); SrgbRead(true); >;
	Texture2D g_t2 < Attribute( "AtmosFar" ); SrgbRead(true); >;
	Texture2D g_t3 < Attribute( "AtmosNear" ); SrgbRead(true); >;
    Texture2D g_t5 < Attribute( "ColorBuffer" ); SrgbRead(true); >;

	SamplerState s0_s < Filter(MIN_MAG_MIP_LINEAR); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	SamplerState s2_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(WRAP); AddressV(CLAMP); AddressW(WRAP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float4 AtmosSunColor < Attribute( "AtmosSunColor" ); Default4( 1.0, 0.95, 0.85, 1.0 ); >;
	float AtmosFogIntensity < Attribute( "AtmosFogIntensity" ); Default1( 1 ); >;
	float AtmosUnk164 < Attribute( "AtmosUnk164" ); Default1( 1 ); >;
	float AtmosUnk170 < Attribute( "AtmosUnk170" ); Default1( 0.0001 ); >;
	float AtmosUnk194 < Attribute( "AtmosUnk194" ); Default1( 0 ); >;
	float AtmosUnk198 < Attribute( "AtmosUnk198" ); Default1( 0.0001 ); >;
	float AtmosUnk16C < Attribute( "AtmosUnk16C" ); Default1( 1 ); >;
	float AtmosUnk168 < Attribute( "AtmosUnk168" ); Default1( 0 ); >;
	float4 AtmosUnk180 < Attribute( "AtmosUnk180" ); Default4( 0,0,0,0 ); >;
	float AtmosUnk190 < Attribute( "AtmosUnk190" ); Default1( 0 ); >;
	float AtmosUnk1C0 < Attribute( "AtmosUnk1C0" ); Default1( 0 ); >;

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
		float4 cb0[62] =
		{
			float4(0.00, 0.00, 0.00, 0.00),        
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.65224, 0.3788, 0.30706, 1.00), 
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.90, 0.90, 0.90, 0.90),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),        
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(4.82757, 0.02648, 0.00, 0.00),   
			float4(0.04328, 5.9396, 0.00, 0.00),    
			float4(0.14483, 0.10904, 0.00, 0.00),   
			float4(0.00, 0.00005, 0.38524, -0.0006), 
			float4(0.37848, 0.42883, 0.45, 0.45),   
			float4(0.45, -0.55, -0.55, -0.55),      
			float4(0.00005, -0.16, -0.16, -0.16),   
			float4(0.00, 0.00, 0.00, 0.00),        
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(40.00, 0.00, 0.00, 0.00),        
			float4(0.90, 0.10, 0.10, 0.10),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(1.00, 1.00, 1.00, 1.00),         
			float4(0.00002, 6.66665, 0.00, 0.00),   
		};
		
		cb0[5] = AtmosSunColor;
		cb0[10] = float4(AtmosFogIntensity.xxxx);
		cb0[50] = GlobalChannel25;
		cb0[60] = (float4(1,1,1,1).zzzz);
		cb0[61] = float4((1 / g_flFarPlane)*TO_INCHES, ((g_flFarPlane - g_flNearPlane) / (g_flFarPlane * g_flNearPlane))*TO_INCHES,0,0);
		cb0[41] = (float4(1.442695, 1.442695, 0, 0) * float4((float4(AtmosUnk164.xxxx) / float4(AtmosUnk170.xxxx)).x, (float4(AtmosUnk194.xxxx) / float4(AtmosUnk198.xxxx)).xyz));
		cb0[42] = (float4(1.442695, 1.442695, 0, 0) * float4(float4(AtmosUnk170.xxxx).x, float4(AtmosUnk198.xxxx).xyz));
		cb0[43] = (float4(1.442695, 1.442695, 0, 0) * float4(float4(AtmosUnk164.xxxx).x, float4(AtmosUnk194.xxxx).xyz));
		cb0[44] = (float4(float4(float4(float4(0, 0, 0, 0).x, (float4(1, 0, 0, 0) / (float4(AtmosUnk16C.xxxx) - float4(AtmosUnk168.xxxx))).xyz).xy, float4(FrameTimeOfDay.xxxx).xy).xyz, (-(float4(AtmosUnk168.xxxx) / (float4(AtmosUnk16C.xxxx) - float4(AtmosUnk168.xxxx)))).x));
		cb0[45] = (AtmosUnk180 * (float4(AtmosUnk190.xxxx).xxxx));
		cb0[46] = (float4(1, 0, 0, 0) - float4(AtmosUnk1C0.xxxx));
		cb0[47] = float4((float4(1, 0, 0, 0) / (GlobalChannel41 + float4(0.001, 0.001, 0.001, 0.001))).x, (-(GlobalChannel37 / (GlobalChannel41 + float4(0.001, 0.001, 0.001, 0.001)))).xyz);
		cb0[51] = float4(((float4((GlobalChannel138.xxxx).x, (GlobalChannel139.xxxx).xyz).yyyy) - (float4((GlobalChannel138.xxxx).x, (GlobalChannel139.xxxx).xyz).xxxx)).x, (float4((GlobalChannel138.xxxx).x, (GlobalChannel139.xxxx).xyz).xxxx).xyz);

		float4 cb13[8] =
		{ // Frame
			float4(CurrentTime, CurrentTime, 0.05, 0.016),
			float4(ExposureScale, ExposureIllumRelative*16, ExposureScale, ExposureIllumRelative),
			float4((CurrentTime + 33.75) * 1.258699, (CurrentTime + 60.0) * 0.9583125, (CurrentTime + 60.0) * 8.789123, (CurrentTime + 33.75) * 2.311535),
			float4(0.5,0.5,0,0),
			float4(1,1,0,1),
			float4(0,0,512,0),
			float4(0,1,sin(CurrentTime * 6.0) * 0.5 + 0.5,0),
			float4(0,0.5,180,0),
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
		
		float4 v0 = i.o0;
		float4 v1 = i.o1;
		float4 v2 = i.o2;
		float4 v3 = i.o3;
		float4 o0,r0,r1,r2,r3,r4,r5,r6,r7,r8,r9,r10;
		float3 test;
		
		r0.xy = cb12[12].xy * v2.xy;
		r0.xy = (int2)r0.xy;
		r0.zw = float2(0,0);
		r0.x = Depth::GetNormalized(r0.xyw).xxxx; // t0.Load(r0.xyw).x;
		r0.x = r0.x * cb0[61].y + cb0[61].x;
		r0.x = 1 / r0.x;
		r0.xyz = v0.xyz * r0.xxx;
		r0.x = dot(r0.xyz, r0.xyz);
		r0.x = sqrt(r0.x);
		
		r1.y = dot(v1.xyz, v1.xyz);
		r1.y = rsqrt(r1.y);
		r1.yzw = v1.xyz * r1.yyy;
		
		r6.z = saturate(1);
		r0.x = -r6.z * cb0[50].x + r0.x;
		r0.x = max(0, r0.x);
		
		r1.x = 0.00100000005 * r0.x;
		r1.y = r1.w * r1.x;
		r1.xz = cb0[43].xy * r1.xx;
		r2.xy = cb0[41].xy / r1.ww;
		r2.zw = cb0[42].xy * r1.yy;
		r1.y = cmp(abs(r1.y) < 9.99999975e-006);
		r2.zw = max(float2(-120,-120), r2.zw);
		r2.zw = exp2(-r2.zw);
		r2.zw = float2(1,1) + -r2.zw;
		r2.xy = r2.xy * r2.zw;
		r1.xy = r1.yy ? r1.xz : r2.xy;
		r1.x = r1.x + r1.y;
		r1.yz = cb0[44].xy * r0.xx + cb0[44].zw;
		r0.x = saturate(r0.x * cb0[47].x + cb0[47].y);
		r1.yzw = g_t1.Sample(s2_s, r1.yz).xyz;
		r1.xyz = r1.yzw * r1.xxx;
		r2.xyz = cb0[45].xyz * -r1.xyz;
		r1.xyz = exp2(-r1.xyz);
		r2.xyz = exp2(r2.xyz);
		r0.yzw = g_t5.Sample(s0_s, v2.xy).xyz;
		r0.yzw = r2.xyz * r0.yzw;
		
		r2.xyzw = g_t2.Sample(s1_s, v2.zw).xyzw;
		r3.xyzw = g_t3.Sample(s1_s, v2.zw).xyzw;
		r2.xyzw = -r3.xyzw + r2.xyzw;
		r2.xyzw = r0.xxxx * r2.xyzw + r3.xyzw;
		r0.x = cb0[46].x * r2.w;
		r3.xyz = -r0.xxx * r1.xyz + r2.www;
		r1.xyz = -r2.xyz * r1.xyz + r2.xyz;
		r1.xyz = r3.xyz * cb0[5].xyz + r1.xyz;
		r1.xyz = cb0[10].xxx * r1.xyz;
		o0.xyz = r1.xyz * cb13[1].xxx + r0.yzw;
		o0.w = 1;

		return float4(o0.xyz, o0.w);
    }
}