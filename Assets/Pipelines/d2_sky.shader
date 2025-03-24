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
    RenderState( DepthEnable, true );

    Texture2D g_t0 < Attribute( "AtmosFar" ); SrgbRead(true); >;
	float4 AtmosSunColor < Attribute( "AtmosSunColor" ); Default4(1.0f, 1.0f, 1.0f, 1.0f ); >;
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float4 v2 = i.vPositionSs;
		float4 o0,r0,r1,r2,r3;
		float4 cb0_4 = float4(0.74071, 0.99975, 0.80516, 1); // "sun" color
		float cb13_1 = 0.65;
		
		r0.xy = float4(g_vViewportSize, g_vInvViewportSize).zw * v2.xy;
		r0.xyzw = g_t0.Sample(s1_s, r0.xy).xyzw;
		r0.xyz = r0.www * AtmosSunColor.xyz + r0.xyz;
		o0.xyz = cb13_1.xxx * r0.xyz;
		o0.w = 1;

		return o0;
    }
}