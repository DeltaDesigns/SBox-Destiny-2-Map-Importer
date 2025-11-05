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
	#include "common/classes/_classes.hlsl"
	#define CUSTOM_TEXTURE_FILTERING

	
	RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, true );

	float4 rt < Default4(240.00, 135.00, 0.00417, 0.00741); Attribute( "RTDimensions"); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float2 v2 = float4(g_vViewportSize, g_vInvViewportSize).xy * i.vTexCoord;
		float4 o0,r0,r1;

		r0.xy = v2.xy;
		r1.x = saturate(1-Depth::Get(r0.xy) * 50000);
		r1.x = min(1, r1.x);
		r1.x = max(0, r1.x);
		o0.xyz = float3(r1.x,0,0);
		o0.w = 1;

		return o0;
    }
}