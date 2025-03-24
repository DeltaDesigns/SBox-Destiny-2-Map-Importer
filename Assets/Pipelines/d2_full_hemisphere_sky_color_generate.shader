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
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float2 vTexCoord : TEXCOORD0;

	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs		: SV_Position;
	#endif

	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs		: SV_Position;
	#endif
};

VS
{
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        
        o.vPositionPs = float4( i.vPositionOs.xy, 0.0f, 1.0f );
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}
PS
{
	#include "postprocess/common.hlsl"
	#define CUSTOM_TEXTURE_FILTERING
    #define cmp -
	
	RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );

    Texture3D g_t0 < Attribute( "AtmosTexture0" ); SrgbRead(true); >;
	Texture3D g_t1 < Attribute( "AtmosTexture1" ); SrgbRead(true); >;
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(WRAP); AddressV(CLAMP); AddressW(WRAP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float4 cb0_4 < Default4(1.0f, 1.0f, 1.0f, 1.0f ); Attribute( "AtmosSunColor" ); >;
	float4 cb0_6 < Default4(0.05923f, 0.05923f, 0.05923f, 0.05923f ); Attribute( "AtmosSunIntensity"); >; //0x154 float, sun intensity again?
	float4 cb0_22 < Default4(0.0f, 0.0f, 0.0f, 0.0f ); Attribute("AtmosRotation"); >;
	float4 cb0_23 < Default4(1.0f, 1.0f, 1.0f, 1.0f ); Attribute("AtmosIntensity"); >;
	float4 cb0_26 < Default4(0.5f, 0.5f, 0.5f, 0.5f ); Attribute("AtmosTimeOfDay"); >;
	float4 cb0_29 < Default4(-0.30372, -0.59835f, 0.74144f, 0.0f ); Attribute( "AtmosSunDir"); >;
	float4 cb0_38 < Default4(0.05923f, 0.05923f, 0.05923f, 0.05923f ); Attribute( "AtmosSunIntensity"); >; // 'sun' intensity?
	
    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float4 o0,r0,r1,r2;
		
		// hack to go from intended 64px rt to 512px
		// until the "proper" sky hemisphere rendering solution is implemented
		float4 v2 = i.vPositionSs / 8; 
		
		float4 cb0_5 = float4(-0.8365, -0.8365, -0.8365, -0.8365); // 0x150 float
		float4 cb0_35 = float4(0,0,0,0); // 0x1D0 vec4
		float4 cb0_47 = float4(0,0,0,0); // global 43

		r0.xy = float2(-0.5,-0.5) + v2.xy;
		r0.xy = r0.xy * float2(0.015625,0.015625) + float2(-0.5,-0.5);
		r0.xy = r0.xy + r0.xy;
		r0.x = -r0.x * r0.x + 1;
		r0.x = -r0.y * r0.y + r0.x;
		r0.x = max(0, r0.x);
		r0.z = sqrt(r0.x);
		r0.xy = v2.xy * float2(0.03125,0.03125) + float2(-1.015625,-1.015625);
		r0.w = dot(r0.xyz, r0.xyz);
		r0.w = rsqrt(r0.w);
		r0.xyz = r0.xyz * r0.www;
		r0.w = -2 * r0.z;
		r0.xyz = r0.xyz * -r0.www + float3(0,0,-1);
		r0.w = dot(r0.xyz, r0.xyz);
		r0.w = rsqrt(r0.w);
		r0.xyz = r0.xyz * r0.www;
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
		r1.y = r0.z * -0.5 + 0.5;
		r0.x = dot(r0.xyz, -cb0_29.xyz);
		r0.x = -r0.x * 1.99899995 + cb0_5.x;
		r0.x = cb0_5.x * r0.x + 1;
		r0.x = log2(r0.x);
		r0.x = -1.5 * r0.x;
		r0.x = exp2(r0.x);
		r1.z = cb0_26.x;
		r2.xyzw = g_t1.Sample(s1_s, r1.xyz).xyzw;
		r1.xyzw = g_t0.Sample(s1_s, r1.xyz).xyzw;
		r2.xyzw = r2.xyzw + -r1.xyzw;
		r1.xyzw = cb0_47.xxxx * r2.xyzw + r1.xyzw;
		r2.xyzw = cb0_23.xxxx * r1.xyzw;
		r0.yzw = -r1.xyz * cb0_23.xxx + cb0_35.xyz;
		r0.yzw = cb0_35.www * r0.yzw + r2.xyz;
		r0.x = r2.w * r0.x;
		r0.x = cb0_6.x * r0.x;
		r0.x = min(512, r0.x);
		r0.xyz = r0.xxx * cb0_4.xyz + r0.yzw;
		o0.xyz = max(float3(0,0,0), r0.xyz);
		o0.w = 0;
		
		return float4(o0.xyz, o0.w);
    }
}