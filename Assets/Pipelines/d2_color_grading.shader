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
		r0.xyz = r0.xxx * float3(0.25,0.5,0.5) + float3(0.125,0.25,0.25);
		r0.xyz = frac(r0.xyz);
		r0.xyz = cmp(r0.xyz >= float3(0.5,0.5,0.5));
		r1.yzw = r0.xyz ? float3(1,1,-1) : float3(-1,-1,1);
		r1.x = r0.x ? 2 : 0;
		r0.xyzw = float4(0,1,0,1) + r1.xwxw;
		o.o0.xy = r1.yz;
		o.o1.xyzw = float4(0.5,0.5,0.5,0.5) * r0.xyzw;
		o.o0.z = cb12[9].x;
		o.o0.w = 1;

        return o;
    }
}

PS
{
	#define CUSTOM_TEXTURE_FILTERING
    #define cmp -
	#include "postprocess/common.hlsl" 
	#include "common/classes/_classes.hlsl"
	
    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );
	
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	SamplerState s2_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(MIRROR); AddressV(MIRROR); AddressW(MIRROR); ComparisonFunc(NEVER); MaxAniso(1); >;
	
	Texture2D g_t0 < Attribute("ColorBuffer"); SrgbRead(true); >;
	Texture2D g_t1 < Attribute("Unk1"); >;
	Texture2D g_t2 < Attribute("Unk2"); >;
	Texture2D g_t3 < Attribute("Vignette"); SrgbRead(true);>;
	Texture2D g_t4 < Attribute("Unk4"); >;
	Texture3D g_t5 < Attribute("ColorLUT"); >;
	
	float4 Unk < Attribute("ColorGradingUnk2"); Default4(0.03125, -5.00, 14.00, 2.50); >;
	float Brightness < Attribute("ColorGradingBrightness"); Default(0.9968); >;
	float ChromaticAberration < Attribute("ColorGradingChromaticAberration"); Default(0.05); >;
	float2 Distortion < Attribute("ColorGradingDistortion"); Default2(0.02, 0.50); >;
	
	float CurrentTime < Attribute( "CurrentTime" ); Default1( 0.0 ); >;
	float ExposureScale < Attribute( "ExposureScale" ); Default1( 0.65 ); >;
	float ExposureIllumRelative < Attribute( "ExposureIllumRelative" ); Default1( 1 ); >;
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float4 r0,r1,r2,r3,r4,r5,o0,o1;
		float3 test;
		float2 v1 = g_vInvViewportSize.xy * i.vPositionSs.xy;
	
		float4 cb0[27] =
		{
			float4(0.00, 0.00, 0.00, 0.00),
			float4(32.00, 1024.00, 0.00, 0.00),
			Unk, //float4(0.03125, -5.00, 14.00, 2.50),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			Brightness.xxxx, //float4(0.9968, 0.9968, 0.9968, 0.9968),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			ChromaticAberration.xxxx, //float4(0.05, 0.05, 0.05, 0.05), // Chromatic aberration?
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(5.00, 5.00, 0.00, 0.00),
			Distortion.xyyy, //float4(0.02, 0.50, 0.50, 0.50), // Some type of distortion
			float4(0.30, 0.30, 0.30, 0.30),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(1.00, 1.00, 1.00, 1.00),
			float4(1.00, 1.00, 1.00, 1.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(2.00, 2.00, 2.00, 2.00),
			float4(0.50, 0.50, 0.50, 0.50),
		};
		
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

		r0.xy = float2(-0.5,-0.5) + v1.xy;
		r1.x = cb0[21].x * r0.x;
		r1.y = cb0[22].x * r0.y;
		r0.x = dot(r1.xy, r1.xy);
		r0.y = sqrt(r0.x);
		r0.x = min(cb0[14].x, r0.x);
		r0.zw = -cb0[19].xy + r0.yy;
		r0.zw = saturate(cb0[17].xy * r0.zw);
		r0.zw = r0.zw * cb0[18].xy + cb0[20].xy;
		r0.y = max(9.99999975e-005, r0.y);
		r0.x = r0.x / r0.y;
		r0.x = r0.z * r0.x;
		r0.yz = r0.xx * r1.xy + v1.xy;
		r1.xy = -r0.xx * r1.xy + v1.xy;
		r2.xyz = g_t0.Sample(s2_s, v1.xy).xyz;
		test = r2.xyz;
		r0.x = g_t0.Sample(s2_s, r0.yz).x;
		r0.z = g_t0.Sample(s2_s, r1.xy).z;
		r0.y = r2.y;
		r0.xyz = r0.xyz + -r2.xyz;
		r0.xyz = r0.www * r0.xyz + r2.xyz;
		r1.xyzw = g_t1.Sample(s1_s, v1.xy).xyzw;
		r2.xyz = g_t2.Sample(s1_s, v1.xy).xyz;
		r3.xyz = g_t3.Sample(s1_s, v1.xy).xyz;
		r0.xyz = cb0[11].xxx * r0.xyz;
		r0.xyz = r0.xyz * r1.www + r1.xyz;
		r0.xyz = max(float3(0,0,0), r0.xyz);
		r0.xyz = min(cb13[5].zzz, r0.xyz);
		r1.xy = cmp(float2(1,2) == cb0[25].xx);
		r0.w = cmp(cb0[26].x < v1.x);
		r0.w = r0.w ? r1.x : 0;
		r0.w = (int)r1.y | (int)r0.w;
		if (r0.w != 0) {
		r1.xyz = cb0[2].xxx + r0.xyz;
		r1.xyz = log2(r1.xyz);
		r1.xyz = -cb0[2].yyy + r1.xyz;
		r1.xyz = saturate(r1.xyz / cb0[2].zzz);
		r1.xyz = float3(1,1,1) + -r1.xyz;
		r1.xyz = log2(r1.xyz);
		r1.xyz = cb0[2].www * r1.xyz;
		r1.xyz = exp2(r1.xyz);
		r1.xyz = float3(1,1,1) + -r1.xyz;
		r1.xyz = max(float3(0,0,0), r1.xyz);
		r1.w = cb0[1].x + -1;
		r1.w = r1.w / cb0[1].x;
		r2.w = 0.5 / cb0[1].x;
		r1.xyz = r1.xyz * r1.www + r2.www;
		r1.xyz = g_t5.Sample(s1_s, r1.xyz).xyz;
		r1.xyz = max(float3(0,0,0), r1.xyz);
		r4.w = dot(r1.xyz, float3(0.300000012,0.589999974,0.109999999));
		r4.xyz = r1.xyz * r3.xyz + r2.xyz;
		}
		if (r0.w == 0) {
		r0.xyz = float3(3.20000005,3.20000005,3.20000005) * r0.xyz;
		r1.x = dot(float3(0.597190022,0.354579985,0.0482299998), r0.xyz);
		r1.y = dot(float3(0.0759999976,0.908339977,0.0156599991), r0.xyz);
		r1.z = dot(float3(0.0284000002,0.133829996,0.837769985), r0.xyz);
		r0.xyz = float3(0.0245785993,0.0245785993,0.0245785993) + r1.xyz;
		r0.xyz = r1.xyz * r0.xyz + float3(-9.05370034e-005,-9.05370034e-005,-9.05370034e-005);
		r5.xyz = r1.xyz * float3(0.983729005,0.983729005,0.983729005) + float3(0.432951003,0.432951003,0.432951003);
		r1.xyz = r1.xyz * r5.xyz + float3(0.238080993,0.238080993,0.238080993);
		r0.xyz = r0.xyz / r1.xyz;
		r1.x = saturate(dot(float3(1.60475004,-0.531080008,-0.0736699998), r0.xyz));
		r1.y = saturate(dot(float3(-0.102080002,1.10812998,-0.00604999997), r0.xyz));
		r1.z = saturate(dot(float3(-0.00326999999,-0.0727600008,1.07602), r0.xyz));
		r0.xyz = r1.xyz * float3(0.96875,0.96875,0.96875) + float3(0.015625,0.015625,0.015625);
		r4.xyzw = g_t4.Sample(s1_s, r0.xyz).xyzw;
		r4.xyz = r4.xyz * r3.xyz + r2.xyz;
		}
		o0.xyzw = r4.xyzw;
		o1.xyzw = r4.wwww;
		
		return float4(o0.xyz, 1);
    }
}